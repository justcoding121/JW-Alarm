using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using JW.Alarm.Services;
using JW.Alarm.Services.Contracts;
using Mvvmicro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace JW.Alarm.ViewModels
{
    public class SongBookSelectionViewModel : ViewModelBase
    {
        private MediaService mediaService;
        private IPopUpService popUpService;
        private IThreadService threadService;

        private readonly AlarmMusic current;
        private readonly AlarmMusic tentative;

        private List<IDisposable> subscriptions = new List<IDisposable>();

        public SongBookSelectionViewModel(AlarmMusic current, AlarmMusic tentative)
        {
            this.current = current;
            this.tentative = tentative;

            publicationCode = current.PublicationCode;

            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();
            this.threadService = IocSetup.Container.Resolve<IThreadService>();

            Task.Run(() => initializeAsync(current.LanguageCode));
        }

        public ObservableHashSet<PublicationListViewItemModel> Translations { get; } = new ObservableHashSet<PublicationListViewItemModel>();
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

        private async Task initializeAsync(string languageCode)
        {
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
            await popUpService.ShowProgressRing();

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

            await popUpService.HideProgressRing();
        }

        private async Task populateSongBooks(string languageCode)
        {
            await popUpService.ShowProgressRing();

            await threadService.RunOnUIThread(() =>
            {
                Translations.Clear();
            });

            var releases = await mediaService.GetVocalMusicReleases(languageCode);
            foreach (var release in releases.Select(x => x.Value))
            {
                var translationVM = new PublicationListViewItemModel(release);

                await threadService.RunOnUIThread(() => Translations.Add(translationVM));

                if (current.LanguageCode == languageCode
                    && current.PublicationCode == release.Code)
                {
                    selectedSongBook = translationVM;
                }
            }

            await threadService.RunOnUIThread(() =>
            {
                RaiseProperty("SelectedTranslation");
            });

            await popUpService.HideProgressRing();
        }
    }
}
