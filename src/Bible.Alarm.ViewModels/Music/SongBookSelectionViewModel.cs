using Bible.Alarm.Services.Contracts;
using Bible.Alarm.ViewModels.Redux.Actions;
using Bible.Alarm.ViewModels.Redux.Actions.Music;
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
using System.Windows.Input;
using Xamarin.Forms;

namespace JW.Alarm.ViewModels
{
    public class SongBookSelectionViewModel : ViewModel, IDisposable
    {
        private MediaService mediaService;
        private IPopUpService popUpService;
        private INavigationService navigationService;

        private AlarmMusic current;
        private AlarmMusic tentative;

        private List<IDisposable> disposables = new List<IDisposable>();

        public SongBookSelectionViewModel()
        {
            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();
            this.navigationService = IocSetup.Container.Resolve<INavigationService>();

            disposables.Add(mediaService);

            //set schedules from initial state.
            //this should fire only once 
            var subscription1 = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
                 .Select(state => new { state.CurrentMusic, state.TentativeMusic })
                 .Where(x => x.CurrentMusic != null && x.TentativeMusic != null)
                 .DistinctUntilChanged()
                 .Take(1)
                 .Subscribe(async x =>
                 {
                     current = x.CurrentMusic;
                     tentative = x.TentativeMusic;

                     await initialize();
                 });

            disposables.Add(subscription1);

            var subscription2 = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
             .Select(state => new { state.CurrentMusic, state.TentativeMusic })
             .Where(x => x.CurrentMusic != null && x.TentativeMusic != null)
             .DistinctUntilChanged()
             .Skip(1)
             .Subscribe(x =>
             {
                 current = x.CurrentMusic;
                 tentative = x.TentativeMusic;
             });

            disposables.Add(subscription2);

            TrackSelectionCommand = new Command<PublicationListViewItemModel>(async x =>
            {
                ReduxContainer.Store.Dispatch(new TrackSelectionAction()
                {
                    TentativeMusic = new AlarmMusic()
                    {
                        Fixed = current.Fixed,
                        MusicType = MusicType.Vocals,
                        LanguageCode = selectedLanguage.Code,
                        PublicationCode = x.Code
                    }
                });

                var viewModel = IocSetup.Container.Resolve<TrackSelectionViewModel>();
                await navigationService.Navigate(viewModel);
            });

            OpenModalCommand = new Command(async () =>
            {
                await navigationService.ShowModal("LanguageModal", this);
            });

            BackCommand = new Command(async () =>
            {
                await navigationService.GoBack();
                ReduxContainer.Store.Dispatch(new BackAction(this));
            });

            CloseModalCommand = new Command(async () =>
            {
                await navigationService.CloseModal();
            });

            SelectLanguageCommand = new Command<LanguageListViewItemModel>(async x =>
            {
                selectedLanguage = x;
                RaiseProperty("SelectedLanguage");
                await navigationService.CloseModal();
                await populateSongBooks(x.Code);
            });

            navigationService.NavigatedBack += onNavigated;
        }

        private void onNavigated(object viewModal)
        {
            if (viewModal.GetType() == this.GetType())
            {
                setSelectedSongBook();
            }
        }

        private void setSelectedSongBook()
        {
            if (current.LanguageCode == tentative.LanguageCode)
            {
                selectedSongBook = SongBooks.FirstOrDefault(y => y.Code == current.PublicationCode);
            }
            else
            {
                selectedSongBook = null;
            }

            RaiseProperty("SelectedSongBook");
        }

        public ICommand BackCommand { get; set; }
        public ICommand TrackSelectionCommand { get; set; }
        public ICommand OpenModalCommand { get; set; }
        public ICommand CloseModalCommand { get; set; }
        public ICommand SelectLanguageCommand { get; set; }
        public ICommand SelectSongBookCommand { get; set; }

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
                //this is a hack since selection is not working in one-way mode 
                //make two-way mode behave like one way mode
                Raise();
            }
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
                //this is a hack since selection is not working in one-way mode 
                //make two-way mode behave like one way mode
                Raise();
            }
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
                          .Where(x => x.Key == "LanguageSearchTerm")
                          .Do(async x => await populateLanguages(LanguageSearchTerm))
                          .Subscribe();

            disposables.Add(subscription1);
        }

        private async Task populateLanguages(string searchTerm = null)
        {
            IsBusy = true;
            var languages = await mediaService.GetVocalMusicLanguages();
            Languages.Clear();

            foreach (var language in languages.Select(x => x.Value).Where(x => searchTerm == null
                    || x.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var languageVM = new LanguageListViewItemModel(language);

                Languages.Add(languageVM);

                if (languageVM.Code == tentative.LanguageCode)
                {
                    selectedLanguage = languageVM;
                }
            }

            RaiseProperty("SelectedLanguage");
            IsBusy = false;
        }

        private async Task populateSongBooks(string languageCode)
        {
            IsBusy = true;

            SongBooks.Clear();
            selectedSongBook = null;

            var releases = await mediaService.GetVocalMusicReleases(languageCode);
            foreach (var release in releases.Select(x => x.Value))
            {
                var songBookListViewItemModel = new PublicationListViewItemModel(release);

                SongBooks.Add(songBookListViewItemModel);

                if (current.MusicType == MusicType.Vocals
                    && current.LanguageCode == languageCode
                    && current.PublicationCode == release.Code)
                {
                    selectedSongBook = songBookListViewItemModel;
                }
            }

            RaiseProperty("SelectedSongBook");
            IsBusy = false;
        }

        public void Dispose()
        {
            navigationService.NavigatedBack -= onNavigated;
            disposables.ForEach(x => x.Dispose());
        }
    }
}
