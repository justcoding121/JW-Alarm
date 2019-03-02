using Bible.Alarm.Services.Contracts;
using Bible.Alarm.ViewModels.Redux.Actions;
using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using JW.Alarm.Services;
using JW.Alarm.Services.Contracts;
using JW.Alarm.ViewModels.Redux;
using Mvvmicro;
using System;
using System.Collections.Generic;
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
        private IThreadService threadService;
        private IPopUpService popUpService;
        private IPreviewPlayService playService;
        private INavigationService navigationService;

        private AlarmMusic current;
        private AlarmMusic tentative;

        private readonly List<IDisposable> disposables = new List<IDisposable>();

        public TrackSelectionViewModel()
        {
            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.threadService = IocSetup.Container.Resolve<IThreadService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();
            this.playService = IocSetup.Container.Resolve<IPreviewPlayService>();
            this.navigationService = IocSetup.Container.Resolve<INavigationService>();

            disposables.Add(mediaService);

            BackCommand = new Command(async () =>
            {
                playService.Stop();
                await navigationService.GoBack();
                ReduxContainer.Store.Dispatch(new BackAction(this));
            });

            SetTrackCommand = new Command<MusicTrackListViewItemModel>(x =>
            {
                selectedTrack = x;
                RaiseProperty("SelectedTrack");

                tentative.TrackNumber = x.Number;

                current.MusicType = tentative.MusicType;
                current.LanguageCode = tentative.LanguageCode;
                current.PublicationCode = tentative.PublicationCode;
                current.TrackNumber = tentative.TrackNumber;
            });

            //set schedules from initial state.
            //this should fire only once 
            var subscription = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
                   .Select(state => new { state.CurrentMusic, state.TentativeMusic })
                   .Where(x => x.CurrentMusic != null && x.TentativeMusic != null)
                   .DistinctUntilChanged()
                   .Take(1)
                   .Subscribe(async x =>
                   {
                       current = x.CurrentMusic;
                       tentative = x.TentativeMusic;
                       await initialize(tentative.LanguageCode, tentative.PublicationCode);
                   });

            disposables.Add(subscription);

        }

        public ICommand BackCommand { get; set; }
        public ICommand SetTrackCommand { get; set; }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => this.Set(ref isBusy, value);
        }

        public ObservableHashSet<MusicTrackListViewItemModel> Tracks { get; set; } = new ObservableHashSet<MusicTrackListViewItemModel>();

        private MusicTrackListViewItemModel selectedTrack;
        public MusicTrackListViewItemModel SelectedTrack
        {
            get => selectedTrack;
            set
            {
                //this is a hack since selection is not working in one-way mode 
                //make two-way mode behave like one way mode
                Raise();
            }
        }

        private MusicTrackListViewItemModel currentlyPlaying;

        private async Task initialize(string languageCode, string publicationCode)
        {
            var scheduleObservable = Observable.FromEventPattern((EventHandler<NotifyCollectionChangedEventArgs> ev)
                              => new NotifyCollectionChangedEventHandler(ev),
                                    ev => Tracks.CollectionChanged += ev,
                                    ev => Tracks.CollectionChanged -= ev);

            var subscription1 = scheduleObservable
                                .SelectMany(x =>
                                {
                                    var newItems = x.EventArgs.NewItems?.Cast<MusicTrackListViewItemModel>();
                                    if (newItems == null)
                                    {
                                        return Enumerable.Empty<IObservable<MusicTrackListViewItemModel>>();
                                    }

                                    return newItems.Select(added =>
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
                                    });

                                })
                                 .Merge()
                                 .Do(async x => await threadService.RunOnUIThread(() =>
                                 {
                                     IsBusy = true;
                                 }))
                                 .Do(y =>
                                 {
                                     if (currentlyPlaying != null)
                                     {
                                         currentlyPlaying.Play = false;
                                     }

                                     currentlyPlaying = y;
                                     playService.Play(y.Url);

                                 })
                                 .Do(async x => await threadService.RunOnUIThread(() =>
                                 {
                                     IsBusy = false;
                                 }))
                                 .Subscribe();

            var subscription2 = scheduleObservable
                               .SelectMany(x =>
                               {
                                   var newItems = x.EventArgs.NewItems?.Cast<MusicTrackListViewItemModel>();
                                   if (newItems == null)
                                   {
                                       return Enumerable.Empty<IObservable<MusicTrackListViewItemModel>>();
                                   }

                                   return newItems.Select(added =>
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
                                   });

                               })
                                .Merge()
                                .Do(async x => await threadService.RunOnUIThread(() =>
                                {
                                    IsBusy = true;
                                }))
                                .Do(y =>
                                {
                                    playService.Stop();
                                })
                                .Do(async x => await threadService.RunOnUIThread(() =>
                                {
                                    IsBusy = false;
                                }))
                                .Subscribe();

            var subscription3 = Observable.FromEvent(ev => playService.OnStopped += ev,
                                                     ev => playService.OnStopped -= ev)
                                 .Do(y =>
                                 {
                                     if (currentlyPlaying != null)
                                     {
                                         currentlyPlaying.Play = false;
                                     }
                                 })
                                 .Subscribe();

            disposables.AddRange(new[] { subscription1, subscription2, subscription3 });

            await populateTracks(languageCode, publicationCode);
        }

        private async Task populateTracks(string languageCode, string publicationCode)
        {
            await threadService.RunOnUIThread(() =>
            {
                IsBusy = true;
            });

            var tracks = languageCode != null ? await mediaService.GetVocalMusicTracks(languageCode, publicationCode)
               : await mediaService.GetMelodyMusicTracks((await this.mediaService.GetMelodyMusicReleases()).First().Value.Code);

            await threadService.RunOnUIThread(() =>
            {
                Tracks.Clear();
            });

            foreach (var track in tracks.Select(x => x.Value))
            {
                var musicTrackListViewItemViewModel = new MusicTrackListViewItemModel(track);

                await threadService.RunOnUIThread(() =>
                {
                    Tracks.Add(musicTrackListViewItemViewModel);
                });

                if (current.MusicType == tentative.MusicType
                    && current.TrackNumber == track.Number
                    && (current.MusicType == MusicType.Melodies ||
                     (current.LanguageCode == tentative.LanguageCode
                        && current.PublicationCode == tentative.PublicationCode))
                    )
                {
                    selectedTrack = musicTrackListViewItemViewModel;
                }
            }

            await threadService.RunOnUIThread(() =>
            {
                RaiseProperty("SelectedTrack");
            });

            await threadService.RunOnUIThread(() =>
            {
                IsBusy = false;
            });
        }

        public void Dispose()
        {
            disposables.ForEach(x => x.Dispose());
        }
    }

    public class MusicTrackListViewItemModel : ViewModel, IComparable
    {
        private readonly MusicTrack chapter;
        public MusicTrackListViewItemModel(MusicTrack chapter)
        {
            this.chapter = chapter;
            TogglePlayCommand = new Command(async () =>
            {
                Play = !Play;
            });
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

        public ICommand TogglePlayCommand { get; set; }

        public int CompareTo(object obj)
        {
            return Number.CompareTo((obj as MusicTrackListViewItemModel).Number);
        }
    }
}
