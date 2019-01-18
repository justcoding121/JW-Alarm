using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using JW.Alarm.Services;
using JW.Alarm.Services.Contracts;
using Mvvmicro;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace JW.Alarm.ViewModels
{
    public class TrackSelectionViewModel : ViewModelBase, IDisposable
    {
        private MediaService mediaService;
        private IThreadService threadService;
        private IPopUpService popUpService;
        private IPlayService playService;
        private AlarmMusic current;
        private AlarmMusic tentative;

        private readonly List<IDisposable> disposables = new List<IDisposable>();

        public TrackSelectionViewModel(AlarmMusic current, AlarmMusic tentative)
        {
            this.current = current;
            this.tentative = tentative;

            this.current = current;

            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.threadService = IocSetup.Container.Resolve<IThreadService>();
            this.popUpService = IocSetup.Container.Resolve<IPopUpService>();
            this.playService = IocSetup.Container.Resolve<IPlayService>();

            initialize();
        }

        private void initialize()
        {
            Task.Run(() => initializeAsync(tentative.LanguageCode, tentative.PublicationCode));
        }

        public ObservableHashSet<MusicTrackListViewItemModel> Tracks { get; set; } = new ObservableHashSet<MusicTrackListViewItemModel>();

        private MusicTrackListViewItemModel selectedTrack;
        public MusicTrackListViewItemModel SelectedTrack
        {
            get => selectedTrack;
            set
            {
                selectedTrack = value;
                RaiseProperty("SelectedTrack");
            }
        }

        public void SetTrack(MusicTrackListViewItemModel musicTrackListViewItemModel)
        {
            SelectedTrack = musicTrackListViewItemModel;

            tentative.TrackNumber = musicTrackListViewItemModel.Number;

            current.MusicType = tentative.MusicType;
            current.LanguageCode = tentative.LanguageCode;
            current.PublicationCode = tentative.PublicationCode;
            current.TrackNumber = tentative.TrackNumber;
        }

        private MusicTrackListViewItemModel currentlyPlaying;

        private async Task initializeAsync(string languageCode, string publicationCode)
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
                                 .Do(async x => await popUpService.ShowProgressRing())
                                 .Do(y =>
                                 {
                                     if (currentlyPlaying != null)
                                     {
                                         currentlyPlaying.Play = false;
                                     }

                                     currentlyPlaying = y;
                                     playService.Play(y.Url);

                                 })
                                 .Do(async x => await popUpService.HideProgressRing())
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
                                .Do(async x => await popUpService.ShowProgressRing())
                                .Do(y =>
                                {
                                    playService.Stop();
                                })
                                .Do(async x => await popUpService.HideProgressRing())
                                .Subscribe();

            disposables.AddRange(new[] { subscription1, subscription2 });

            await populateTracks(languageCode, publicationCode);
        }

        private async Task populateTracks(string languageCode, string publicationCode)
        {
            await popUpService.ShowProgressRing();

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

            await popUpService.HideProgressRing();
        }

        public void Dispose()
        {
            disposables.ForEach(x => x.Dispose());
            disposables.Clear();
        }
    }

    public class MusicTrackListViewItemModel : ViewModelBase, IComparable
    {
        private readonly MusicTrack chapter;
        public MusicTrackListViewItemModel(MusicTrack chapter)
        {
            this.chapter = chapter;
        }

        public int Number => chapter.Number;

        public string Title => chapter.Title;
        public string Url => chapter.Url;
        public TimeSpan Duration => chapter.Duration;

        private bool play;
        public bool Play
        {
            get => play;
            set => this.Set(ref play, value);
        }

        public int CompareTo(object obj)
        {
            return Number.CompareTo((obj as MusicTrackListViewItemModel).Number);
        }
    }
}
