using Bible.Alarm.Models;
using Bible.Alarm.Services;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.ViewModels.Redux;
using Bible.Alarm.ViewModels.Redux.Actions.Music;
using Mvvmicro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bible.Alarm.ViewModels
{
    public class TrackSelectionViewModel : ViewModel, IDisposable
    {
        private readonly IContainer container;

        private MediaService mediaService;
        private IToastService toastService;
        private IPreviewPlayService playService;
        private INavigationService navigationService;

        private AlarmMusic current;
        private AlarmMusic tentative;

        private readonly List<IDisposable> subscriptions = new List<IDisposable>();

        public TrackSelectionViewModel(IContainer container)
        {
            this.container = container;

            this.mediaService = this.container.Resolve<MediaService>();
            this.toastService = this.container.Resolve<IToastService>();
            this.playService = this.container.Resolve<IPreviewPlayService>();
            this.navigationService = this.container.Resolve<INavigationService>();

            subscriptions.Add(mediaService);

            BackCommand = new Command(async () =>
            {
                IsBusy = true;
                playService.Stop();
                await navigationService.GoBack();
                IsBusy = false;
            });

            SetTrackCommand = new Command<MusicTrackListViewItemModel>(x =>
            {
                IsBusy = true;
                if (SelectedTrack != null)
                {
                    SelectedTrack.IsSelected = false;
                    SelectedTrack.Repeat = false;
                }

                SelectedTrack = x;
                SelectedTrack.IsSelected = true;

                tentative.TrackNumber = x.Number;
                tentative.Repeat = x.Repeat;

                ReduxContainer.Store.Dispatch(new TrackSelectedAction()
                {
                    CurrentMusic = new AlarmMusic()
                    {
                        MusicType = tentative.MusicType,
                        LanguageCode = tentative.LanguageCode,
                        PublicationCode = tentative.PublicationCode,
                        TrackNumber = tentative.TrackNumber,
                        Repeat = tentative.Repeat
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

            subscriptions.Add(subscription1);
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

        private SemaphoreSlim @lock = new SemaphoreSlim(1);

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
                                     await @lock.WaitAsync();

                                     try
                                     {
                                         if (currentlyPlaying != null && currentlyPlaying != y)
                                         {
                                             currentlyPlaying.Play = false;
                                             currentlyPlaying.IsBusy = false;
                                         }

                                         currentlyPlaying = y;

                                         currentlyPlaying.IsBusy = true;
                                         try
                                         {
                                             await Task.Run(() => playService.Play(y.Url));
                                         }
                                         catch
                                         {
                                             currentlyPlaying.Play = false;
                                             await toastService.ShowMessage("Failed to download the file.");
                                         }

                                         currentlyPlaying.IsBusy = false;
                                     }
                                     finally { @lock.Release(); }

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
                                 .Do(async y =>
                                 {
                                     await @lock.WaitAsync();

                                     try
                                     {
                                         if (currentlyPlaying != null)
                                         {
                                             currentlyPlaying.Play = false;
                                             currentlyPlaying.IsBusy = false;
                                             currentlyPlaying = null;
                                         }
                                     }
                                     finally { @lock.Release(); }
                                 })
                                 .Subscribe();

            var subscription4 = Tracks.Select(added =>
            {
                return Observable.FromEvent<PropertyChangedEventHandler, KeyValuePair<string, MusicTrackListViewItemModel>>(
                               onNextHandler => (object sender, PropertyChangedEventArgs e)
                                             => onNextHandler(new KeyValuePair<string, MusicTrackListViewItemModel>(e.PropertyName,
                                                                        (MusicTrackListViewItemModel)sender)),
                                               handler => added.PropertyChanged += handler,
                                               handler => added.PropertyChanged -= handler)
                                               .Where(kv => kv.Key == "Repeat")
                                               .Select(y => y.Value);
            })
                             .Merge()
                             .Do(x =>
                             {
                                 tentative.Repeat = x.Repeat;
                                 tentative.TrackNumber = x.Number;

                                 ReduxContainer.Store.Dispatch(new TrackSelectedAction()
                                 {
                                     CurrentMusic = new AlarmMusic()
                                     {
                                         MusicType = tentative.MusicType,
                                         LanguageCode = tentative.LanguageCode,
                                         PublicationCode = tentative.PublicationCode,
                                         TrackNumber = tentative.TrackNumber,
                                         Repeat = tentative.Repeat
                                     }
                                 });

                                 if (x.Repeat)
                                 {
                                     toastService.ShowMessage("Alarm will always repeat this track.");
                                 }

                             })
                             .Subscribe();

            subscriptions.AddRange(new[] { subscription1, subscription2, subscription3, subscription4 });
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
                    SelectedTrack.Repeat = current.Repeat;
                }
            }

            Tracks = trackVMs;
        }

        public void Dispose()
        {
            subscriptions.ForEach(x => x.Dispose());

            mediaService.Dispose();
            toastService.Dispose();
            playService.Dispose();
            @lock.Dispose();
        }
    }

    public class MusicTrackListViewItemModel : ViewModel, IComparable
    {
        private readonly MusicTrack track;
        public MusicTrackListViewItemModel(MusicTrack track)
        {
            this.track = track;
            TogglePlayCommand = new Command(() => Play = !Play);
            ToggleRepeatCommand = new Command(() => Repeat = !Repeat);
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => this.Set(ref isSelected, value);
        }

        public int Number => track.Number;

        public string Title => track.Title;
        public string Url => track.Source.Url;
        public TimeSpan Duration => track.Source.Duration;

        private bool play;
        public bool Play
        {
            get => play;
            set => this.Set(ref play, value);
        }

        private bool repeat;
        public bool Repeat
        {
            get => repeat;
            set => this.Set(ref repeat, value);
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => this.Set(ref isBusy, value);
        }

        public ICommand TogglePlayCommand { get; set; }
        public ICommand ToggleRepeatCommand { get; set; }
        public int CompareTo(object obj)
        {
            return Number.CompareTo((obj as MusicTrackListViewItemModel).Number);
        }
    }
}
