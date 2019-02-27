using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using JW.Alarm.Services;
using JW.Alarm.Services.Contracts;
using JW.Alarm.ViewModels.Redux;
using Mvvmicro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace JW.Alarm.ViewModels
{
    public class SongBookSelectionViewModel : ViewModel
    {
        private MediaService mediaService;
        private IPopUpService popUpService;
        private IThreadService threadService;

        private AlarmMusic current;
        private AlarmMusic tentative;

        private List<IDisposable> subscriptions = new List<IDisposable>();

        public SongBookSelectionViewModel()
        {
            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();
            this.threadService = IocSetup.Container.Resolve<IThreadService>();

            //set schedules from initial state.
            //this should fire only once 
            ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
                .Select(state => new { state.CurrentMusic, state.TentativeMusic })
                .Where(x => x.CurrentMusic != null && x.TentativeMusic != null)
                .DistinctUntilChanged()
                .Take(1)
                .Subscribe(async x =>
                {
                    current = x.CurrentMusic;
                    tentative = x.TentativeMusic;
                    publicationCode = current.PublicationCode;
                    await initialize();
                });
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => this.Set(ref isBusy, value);
        }

        public ObservableHashSet<PublicationListViewItemModel> SongBooks { get; } = new ObservableHashSet<PublicationListViewItemModel>();
        public ObservableHashSet<LanguageListViewItemModel> Languages { get; } = new ObservableHashSet<LanguageListViewItemModel>();

        private LanguageListViewItemModel selectedLanguage;
        public LanguageListViewItemModel SelectedLanguage
        {
            get => selectedLanguage;
            set
            {
                selectedLanguage = value;
                RaiseProperty("SelectedLanguage");
            }
        }

        private string publicationCode;
        public string PublicationCode
        {
            get => publicationCode;
            set => this.Set(ref publicationCode, value);
        }

        private string languageSearchTerm;
        public string LanguageSearchTerm
        {
            get => languageSearchTerm;
            set => this.Set(ref languageSearchTerm, value);
        }


        private PublicationListViewItemModel selectedSongBook;
        public PublicationListViewItemModel SelectedSongBook
        {
            get => selectedSongBook;
            set
            {
                selectedSongBook = value;
                RaiseProperty("SelectedSongBook");
            }
        }

        public TrackSelectionViewModel GetTrackSelectionViewModel(PublicationListViewItemModel selectedSongBook)
        {
            tentative.LanguageCode = selectedLanguage.Code;
            tentative.PublicationCode = selectedSongBook.Code;

            return new TrackSelectionViewModel(current, tentative);
        }

        private async Task initialize()
        {
            var languageCode = tentative.LanguageCode;

            if (languageCode == null)
            {
                var languages = await mediaService.GetVocalMusicLanguages();
                if (languages.ContainsKey("E"))
                {
                    languageCode = "E";
                }
                else
                {
                    languageCode = languages.First().Key;
                }
            }

            tentative.LanguageCode = languageCode;

            await populateLanguages();
            await populateSongBooks(languageCode);

            var subscription1 = Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, object>>(
                                                    onNextHandler => (object sender, PropertyChangedEventArgs e)
                                                    => onNextHandler(new KeyValuePair<string, object>(e.PropertyName, sender)),
                                                    handler => PropertyChanged += handler,
                                                    handler => PropertyChanged -= handler)
                                .Where(x => x.Key == "SelectedLanguage")
                                .Where(x => SelectedLanguage != null)
                                .Do(async x => await populateSongBooks(SelectedLanguage.Code))
                                .Subscribe();

            subscriptions.Add(subscription1);

            var subscription2 = Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, object>>(
                                              onNextHandler => (object sender, PropertyChangedEventArgs e)
                                              => onNextHandler(new KeyValuePair<string, object>(e.PropertyName, sender)),
                                              handler => PropertyChanged += handler,
                                              handler => PropertyChanged -= handler)
                          .Where(x => x.Key == "LanguageSearchTerm")
                          .Do(async x => await populateLanguages(LanguageSearchTerm))
                          .Subscribe();

            subscriptions.Add(subscription2);
        }

        private async Task populateLanguages(string searchTerm = null)
        {
            await threadService.RunOnUIThread(() =>
            {
                IsBusy = true;
            });

            var languages = await mediaService.GetVocalMusicLanguages();

            await threadService.RunOnUIThread(() =>
            {
                Languages.Clear();
            });

            foreach (var language in languages.Select(x => x.Value).Where(x => searchTerm == null
                    || x.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var languageVM = new LanguageListViewItemModel(language);

                await threadService.RunOnUIThread(() => Languages.Add(languageVM));

                if (languageVM.Code == tentative.LanguageCode)
                {
                    selectedLanguage = languageVM;
                }
            }

            await threadService.RunOnUIThread(() =>
            {
                RaiseProperty("SelectedLanguage");
            });

            await threadService.RunOnUIThread(() =>
            {
                IsBusy = false;
            });
        }

        private async Task populateSongBooks(string languageCode)
        {
            await threadService.RunOnUIThread(() =>
            {
                IsBusy = true;
            });

            await threadService.RunOnUIThread(() =>
            {
                SongBooks.Clear();
            });

            var releases = await mediaService.GetVocalMusicReleases(languageCode);
            foreach (var release in releases.Select(x => x.Value))
            {
                var songBookListViewItemModel = new PublicationListViewItemModel(release);

                await threadService.RunOnUIThread(() => SongBooks.Add(songBookListViewItemModel));

                if (current.MusicType == MusicType.Vocals
                    && current.LanguageCode == languageCode
                    && current.PublicationCode == release.Code)
                {
                    selectedSongBook = songBookListViewItemModel;
                }
            }

            await threadService.RunOnUIThread(() =>
            {
                RaiseProperty("SelectedSongBook");
            });

            await threadService.RunOnUIThread(() =>
            {
                IsBusy = false;
            });
        }
    }
}
