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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace JW.Alarm.ViewModels
{
    public class TrackSelectionViewModel : ViewModel, IDisposable
    {
        private MediaService mediaService;
        private IToastService popUpService;
        private IPreviewPlayService playService;
        private INavigationService navigationService;

        private AlarmMusic current;
        private AlarmMusic tentative;

        private readonly List<IDisposable> disposables = new List<IDisposable>();

        public TrackSelectionViewModel()
        {
            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.popUpService = IocSetup.Container.Resolve<IToastService>();
            this.playService = IocSetup.Container.Resolve<IPreviewPlayService>();
            this.navigationService = IocSetup.Container.Resolve<INavigationService>();

            disposables.Add(mediaService);

            BackCommand = new Command(async () =>
            {
                IsBusy = true;
                playService.Stop();
                await navigationService.GoBack();
                ReduxContainer.Store.Dispatch(new BackAction(this));
                IsBusy = false;
            });

            SetTrackCommand = new Command<MusicTrackListViewItemModel>(x =>
            {
                IsBusy = true;
                if (SelectedTrack != null)
                {
                    SelectedTrack.IsSelected = false;
                }

                SelectedTrack = x;
                SelectedTrack.IsSelected = true;

                tentative.TrackNumber = x.Number;

                ReduxContainer.Store.Dispatch(new TrackSelectedAction()
                {
                    CurrentMusic = new AlarmMusic()
                    {
                        MusicType = tentative.MusicType,
                        LanguageCode = tentative.LanguageCode,
                        PublicationCode = tentative.PublicationCode,
                        TrackNumber = tentative.TrackNumber
                    }
                });
                IsBusy = false;
            });

            //set schedules from initial state.
            //this should fire only once 
            var subscription1 = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
                   .Select(state => new { state.CurrentMusic, state.TentativeMusic })
                   .Where(x => x.CurrentMusic != null && x.TentativeMusic != null)
                   .DistinctUntilChanged()
                   .Take(1)
                   .Subscribe(async x =>
                   {
                       IsBusy = true;
                       current = x.CurrentMusic;
                       tentative = x.TentativeMusic;
                       await initialize(tentative.LanguageCode, tentative.PublicationCode);
                       IsBusy = false;
                   });

            disposables.Add(subscription1);
        }

        public ICommand BackCommand { get; set; }
        public ICommand SetTrackCommand { get; set; }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => this.Set(ref isBusy, value);
        }

        public ObservableCollection<MusicTrackListViewItemModel> Tracks { get; set; } = new ObservableCollection<MusicTrackListViewItemModel>();

        public MusicTrackListViewItemModel SelectedTrack { get; set; }

        private MusicTrackListViewItemModel currentlyPlaying;

        private async Task initialize(string languageCode, string publicationCode)
        {
            await populateTracks(languageCode, publicationCode);

            var subscription1 = Tracks.Select(added =>
                                {
                                    return Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, MusicTrackListViewItemModel>>(
                                        onNextHandler => (object sender, PropertyChangedEventArgs e)
                                             => onNextHandler(new KeyValuePair<string, MusicTrackListViewItemModel>(e.PropertyName,
                                                                        (MusicTrackListViewItemModel)sender)),
                                               handler => added.PropertyChanged += handler,
                                               handler => added.PropertyChanged -= handler)
                                               .Where(kv => kv.Key == "Play")
                                               .Select(y => y.Value)
                                               .Where(y => y.Play);
                                }).Merge()
                                 .Do(async y =>
                                 {
                                     if (currentlyPlaying != null)
                                     {
                                         currentlyPlaying.Play = false;
                                         currentlyPlaying.IsBusy = false;
                                     }

                                     currentlyPlaying = y;

                                     currentlyPlaying.IsBusy = true;
                                     await Task.Delay(500);
                                     await playService.Play(y.Url);
                                     currentlyPlaying.IsBusy = false;

                                 })
                                 .Subscribe();

            var subscription2 = Tracks.Select(added =>
                                {
                                    return Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, MusicTrackListViewItemModel>>(
                                                   onNextHandler => (object sender, PropertyChangedEventArgs e)
                                                                 => onNextHandler(new KeyValuePair<string, MusicTrackListViewItemModel>(e.PropertyName,
                                                                                            (MusicTrackListViewItemModel)sender)),
                                                                   handler => added.PropertyChanged += handler,
                                                                   handler => added.PropertyChanged -= handler)
                                                                   .Where(kv => kv.Key == "Play")
                                                                   .Select(y => y.Value)
                                                                   .Where(y => !y.Play);
                                })
                                .Merge()
                                .Do(y =>
                                {
                                    playService.Stop();
                                })
                                .Subscribe();

            var subscription3 = Observable.FromEvent(ev => playService.OnStopped += ev,
                                                     ev => playService.OnStopped -= ev)
                                 .Do(y =>
                                 {
                                     if (currentlyPlaying != null)
                                     {
                                         currentlyPlaying.Play = false;
                                         currentlyPlaying.IsBusy = false;
                                     }
                                 })
                                 .Subscribe();

            disposables.AddRange(new[] { subscription1, subscription2, subscription3 });
        }

        private async Task populateTracks(string languageCode, string publicationCode)
        {
            var tracks = languageCode != null ? await mediaService.GetVocalMusicTracks(languageCode, publicationCode)
               : await mediaService.GetMelodyMusicTracks((await this.mediaService.GetMelodyMusicReleases()).First().Value.Code);

            var trackVMs = new ObservableCollection<MusicTrackListViewItemModel>();

            foreach (var track in tracks.Select(x => x.Value))
            {
                var musicTrackListViewItemViewModel = new MusicTrackListViewItemModel(track);

                trackVMs.Add(musicTrackListViewItemViewModel);

                if (current.MusicType == tentative.MusicType
                    && current.TrackNumber == track.Number
                    && (current.MusicType == MusicType.Melodies ||
                     (current.LanguageCode == tentative.LanguageCode
                        && current.PublicationCode == tentative.PublicationCode))
                    )
                {
                    SelectedTrack = musicTrackListViewItemViewModel;
                    SelectedTrack.IsSelected = true;
                }
            }

            Tracks = trackVMs;
        }

        public void Dispose()
        {
            disposables.ForEach(x => x.Dispose());
            disposables.Clear();
            playService.Dispose();
        }
    }

    public class MusicTrackListViewItemModel : ViewModel, IComparable
    {
        private readonly MusicTrack chapter;
        public MusicTrackListViewItemModel(MusicTrack chapter)
        {
            this.chapter = chapter;
            TogglePlayCommand = new Command(() => Play = !Play);
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => this.Set(ref isSelected, value);
        }

        public int Number => chapter.Number;

        public string Title => chapter.Title;
        public string Url => chapter.Source.Url;
        public TimeSpan Duration => chapter.Source.Duration;

        private bool play;
        public bool Play
        {
            get => play;
            set => this.Set(ref play, value);
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => this.Set(ref isBusy, value);
        }

        public ICommand TogglePlayCommand { get; set; }

        public int CompareTo(object obj)
        {
            return Number.CompareTo((obj as MusicTrackListViewItemModel).Number);
        }
    }
}
