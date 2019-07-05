﻿using Bible.Alarm.Services.Contracts;
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
using System.Collections.ObjectModel;
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
        private IToastService popUpService;
        private INavigationService navigationService;

        private AlarmMusic current;
        private AlarmMusic tentative;

        private List<IDisposable> disposables = new List<IDisposable>();

        public SongBookSelectionViewModel()
        {
            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.popUpService = IocSetup.Container.Resolve<IToastService>();
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

                     IsBusy = false;
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
                IsBusy = true;

                ReduxContainer.Store.Dispatch(new TrackSelectionAction()
                {
                    TentativeMusic = new AlarmMusic()
                    {
                        Fixed = current.Fixed,
                        MusicType = MusicType.Vocals,
                        LanguageCode = CurrentLanguage.Code,
                        PublicationCode = x.Code
                    }
                });

                var viewModel = IocSetup.Container.Resolve<TrackSelectionViewModel>();
                await navigationService.Navigate(viewModel);

                IsBusy = false;
            });

            OpenModalCommand = new Command(async () =>
            {
                IsBusy = true;
                await navigationService.ShowModal("LanguageModal", this);
                IsBusy = false;
            });

            BackCommand = new Command(async () =>
            {
                IsBusy = true;
                await navigationService.GoBack();
                ReduxContainer.Store.Dispatch(new BackAction(this));
                IsBusy = false;
            });

            CloseModalCommand = new Command(async () =>
            {
                await navigationService.CloseModal();
            });

            SelectLanguageCommand = new Command<LanguageListViewItemModel>(async x =>
            {
                IsBusy = true;
                CurrentLanguage = x;
                await navigationService.CloseModal();
                await populateSongBooks(x.Code);
                IsBusy = false;
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
            if (SelectedSongBook != null)
            {
                SelectedSongBook.IsSelected = false;
                SelectedSongBook = null;
            }

            if (current.LanguageCode == tentative.LanguageCode)
            {
                SelectedSongBook = songBookVMsMapping.ContainsKey(current.PublicationCode) ? songBookVMsMapping[current.PublicationCode] : null;

                if (SelectedSongBook != null)
                {
                    SelectedSongBook.IsSelected = true;
                }
            }

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

        private ObservableCollection<PublicationListViewItemModel> songBooks;
        public ObservableCollection<PublicationListViewItemModel> SongBooks
        {
            get => songBooks;
            set => this.Set(ref songBooks, value);
        }

        private ObservableCollection<LanguageListViewItemModel> languages;
        public ObservableCollection<LanguageListViewItemModel> Languages
        {
            get => languages;
            set => this.Set(ref languages, value);
        }

        private LanguageListViewItemModel currentLanguage;
        public LanguageListViewItemModel CurrentLanguage
        {
            get => currentLanguage;
            set => this.Set(ref currentLanguage, value);
        }

        private string languageSearchTerm;
        public string LanguageSearchTerm
        {
            get => languageSearchTerm;
            set => this.Set(ref languageSearchTerm, value);
        }

        public PublicationListViewItemModel SelectedSongBook { get; set; }

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
            var languages = await mediaService.GetVocalMusicLanguages();
            var languageVMs = new ObservableCollection<LanguageListViewItemModel>();

            foreach (var language in languages.Select(x => x.Value).Where(x => searchTerm == null
                    || x.Name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                var languageVM = new LanguageListViewItemModel(language);

                languageVMs.Add(languageVM);

                if (languageVM.Code == tentative.LanguageCode)
                {
                    languageVM.IsSelected = true;
                    CurrentLanguage = languageVM;
                }
            }

            Languages = languageVMs;
        }

        private Dictionary<string, PublicationListViewItemModel> songBookVMsMapping
            = new Dictionary<string, PublicationListViewItemModel>();

        private async Task populateSongBooks(string languageCode)
        {
            SelectedSongBook = null;

            songBookVMsMapping.Clear();

            var songBooks = await mediaService.GetVocalMusicReleases(languageCode);
            var songBookVMs = new ObservableCollection<PublicationListViewItemModel>();

            foreach (var release in songBooks.Select(x => x.Value))
            {
                var songBookListViewItemModel = new PublicationListViewItemModel(release);

                songBookVMs.Add(songBookListViewItemModel);
                songBookVMsMapping.Add(songBookListViewItemModel.Code, songBookListViewItemModel);

                if (current.MusicType == MusicType.Vocals
                    && current.LanguageCode == languageCode
                    && current.PublicationCode == release.Code)
                {
                    SelectedSongBook = songBookListViewItemModel;
                    SelectedSongBook.IsSelected = true;
                }
            }

            SongBooks = songBookVMs;
        }

        public void Dispose()
        {
            navigationService.NavigatedBack -= onNavigated;
            disposables.ForEach(x => x.Dispose());
        }
    }
}
