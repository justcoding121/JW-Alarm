using Advanced.Algorithms.DataStructures.Foundation;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Contracts.Network;
using Bible.Alarm.Models;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.ViewModels.Shared;
using MediaManager;
using MediaManager.Library;
using MediaManager.Media;
using MediaManager.Playback;
using MediaManager.Player;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Droid
{
    public class PlaybackService : IPlaybackService, IDisposable
    {
        private Logger logger => LogHelper.GetLogger(global::Xamarin.Forms.Forms.IsInitialized);

        private readonly IMediaManager mediaManager;
        private IAlarmService alarmService;
        private IPlaylistService playlistService;
        private IMediaCacheService cacheService;
        private IStorageService storageService;
        private INetworkStatusService networkStatusService;
        private INotificationService notificationService;

        private static long currentScheduleId;
        private Dictionary<IMediaItem, NotificationDetail> currentlyPlaying
            = new Dictionary<IMediaItem, NotificationDetail>();

        private SemaphoreSlim @lock = new SemaphoreSlim(1);

        private IMediaItem firstChapter;

        private bool readyTodispose;
        long IPlaybackService.CurrentlyPlayingScheduleId => currentScheduleId;

        public event EventHandler<MediaPlayerState> Stopped;

        public PlaybackService(IMediaManager mediaManager,
            IPlaylistService playlistService,
            IAlarmService alarmService,
            IMediaCacheService cacheService,
            IStorageService storageService,
            INetworkStatusService networkStatusService,
            INotificationService notificationService)
        {
            this.mediaManager = mediaManager;
            this.playlistService = playlistService;
            this.alarmService = alarmService;
            this.cacheService = cacheService;
            this.storageService = storageService;
            this.networkStatusService = networkStatusService;
            this.notificationService = notificationService;

            this.mediaManager.MediaItemFinished += markTrackAsFinished;
            this.mediaManager.StateChanged += stateChanged;
        }

        private Task watcher;
        private bool wasPlaying;
        private DateTime playStartTime = DateTime.Now.AddDays(7);

        private static IMediaExtractor mediaExtractor => CrossMediaManager.Current.Extractor;

        public async Task Play(long scheduleId)
        {
            //already playing
            if (this.mediaManager.IsPrepared())
            {
                Dispose();
                Stopped?.Invoke(this, MediaPlayerState.Stopped);
                return;
            }

            playStartTime = DateTime.Now;

            await @lock.WaitAsync();

            try
            {
                currentScheduleId = scheduleId;

                var nextTracks = await playlistService.NextTracks(scheduleId);

                var downloadedTracks = new OrderedDictionary<int, FileInfo>();
                var streamingTracks = new OrderedDictionary<int, string>();

                var playDetailMap = new Dictionary<int, NotificationDetail>();

                var internetOn = await networkStatusService.IsInternetAvailable();

                var i = 0;
                foreach (var item in nextTracks)
                {
                    playDetailMap[i] = item.PlayDetail;

                    if (await cacheService.Exists(item.Url))
                    {
                        downloadedTracks.Add(i, new FileInfo(this.cacheService.GetCacheFilePath(item.Url)));
                    }
                    else
                    {

                        if (internetOn)
                        {
                            streamingTracks.Add(i, item.Url);
                        }
                        else
                        {
                            break;
                        }
                    }
                    i++;
                }

                if (IocSetup.Container.RegisteredTypes.Any(x => x == typeof(Xamarin.Forms.INavigation)))
                {
                    await Messenger<object>.Publish(Messages.ShowMediaProgessModal, IocSetup.Container.Resolve<MediaProgressViewModal>());
                }

                var preparedTracks = 0;
                var totalTracks = nextTracks.Count;

                if (IocSetup.Container.RegisteredTypes.Any(y => y == typeof(Xamarin.Forms.INavigation)))
                {
                    await Messenger<object>.Publish(Messages.MediaProgress, new Tuple<int, int>(preparedTracks, totalTracks));
                }

                var downloadedMediaItems = (await Task.WhenAll(downloadedTracks.Select(x =>
                {
                    return Task.Run(async () =>
                    {
                        var item = await mediaExtractor.CreateMediaItem(x.Value);
                        if (IocSetup.Container.RegisteredTypes.Any(y => y == typeof(Xamarin.Forms.INavigation)))
                        {
                            await Messenger<object>.Publish(Messages.ShowMediaProgessModal, IocSetup.Container.Resolve<MediaProgressViewModal>());
                            await Messenger<object>.Publish(Messages.MediaProgress, new Tuple<int, int>(++preparedTracks, totalTracks));
                        }

                        return item;
                    });

                }))).ToList();

                var streamableMediaItems = (await Task.WhenAll(streamingTracks.Select(x =>
               {
                   return Task.Run(async () =>
                   {
                       var item = await mediaExtractor.CreateMediaItem(x.Value);
                       if (IocSetup.Container.RegisteredTypes.Any(y => y == typeof(Xamarin.Forms.INavigation)))
                       {
                           await Messenger<object>.Publish(Messages.ShowMediaProgessModal, IocSetup.Container.Resolve<MediaProgressViewModal>());
                           await Messenger<object>.Publish(Messages.MediaProgress, new Tuple<int, int>(++preparedTracks, totalTracks));
                       }

                       return item;
                   });

               }))).ToList();

                if (IocSetup.Container.RegisteredTypes.Any(x => x == typeof(Xamarin.Forms.INavigation)))
                {
                    await Messenger<object>.Publish(Messages.HideMediaProgressModal, null);
                }

                var mergedMediaItems = new OrderedDictionary<int, IMediaItem>();

                i = 0;
                foreach (var item in downloadedTracks)
                {
                    mergedMediaItems.Add(item.Key, downloadedMediaItems[i]);
                    i++;
                }

                i = 0;
                foreach (var item in streamingTracks)
                {
                    mergedMediaItems.Add(item.Key, streamableMediaItems[i]);
                    i++;
                }

                //play default ring tone if we don't have the files downloaded
                //and internet is not available
                if (!downloadedMediaItems.Any() && !await networkStatusService.IsInternetAvailable())
                {
                    this.mediaManager.RepeatMode = RepeatMode.All;
                    var item = await this.mediaManager.Play(new FileInfo(Path.Combine(this.storageService.StorageRoot, "cool-alarm-tone-notification-sound.mp3")));
                }
                else
                {
                    currentlyPlaying = new Dictionary<IMediaItem, NotificationDetail>();

                    foreach (var track in mergedMediaItems)
                    {
                        currentlyPlaying.Add(track.Value, playDetailMap[track.Key]);
                    }

                    firstChapter = currentlyPlaying.First(x => x.Value.IsBibleReading).Key;

                    await this.mediaManager.Play(mergedMediaItems.Select(x => x.Value));
                }
            }
            finally
            {
                @lock.Release();
            }

            if (watcher == null)
            {
                watcher = Task.Run(async () =>
                {
                    while (!disposed)
                    {
                        await @lock.WaitAsync();

                        try
                        {
                            if (this.mediaManager.IsPlaying())
                            {
                                wasPlaying = true;

                                var mediaItem = this.mediaManager.Queue.Current;
                                if (currentlyPlaying.ContainsKey(mediaItem))
                                {
                                    var track = currentlyPlaying[mediaItem];

                                    if (track.FinishedDuration.TotalSeconds > 0
                                        && firstChapter != null
                                        && mediaItem == firstChapter)
                                    {
                                        await this.mediaManager.SeekTo(track.FinishedDuration);
                                        firstChapter = null;
                                    }
                                    else if (mediaManager.Position.TotalSeconds > 0)
                                    {
                                        if (mediaItem == firstChapter)
                                        {
                                            firstChapter = null;
                                        }

                                        track.FinishedDuration = mediaManager.Position;
                                        await this.playlistService.MarkTrackAsPlayed(track);
                                    }
                                }
                            }

                            //was playing and now stopped or play stopped without even getting played for last 60 seconds
                            if ((wasPlaying || DateTime.Now.Subtract(playStartTime).TotalSeconds > 60)
                                && this.mediaManager.IsStopped())
                            {
                                readyTodispose = true;
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "An error happened when updating finished track duration.");
                        }
                        finally
                        {
                            @lock.Release();
                        }

                        if (readyTodispose)
                        {
                            await Messenger<object>.Publish(Messages.HideAlarmModal, null);
                            this.notificationService.ClearAll();
                            Dispose();
                            Stopped?.Invoke(this, MediaPlayerState.Stopped);

                            break;
                        }

                        await Task.Delay(1000);
                    }
                });
            }
        }

        public async Task Snooze()
        {
            await @lock.WaitAsync();

            try
            {
                if (this.mediaManager.IsPrepared() && !readyTodispose)
                {
                    await this.mediaManager.Stop();
                    await this.alarmService.Snooze(currentScheduleId);
                    currentScheduleId = 0;
                }
            }
            finally
            {
                @lock.Release();
            }

            readyTodispose = true;
        }

        public async Task Dismiss()
        {
            if (this.mediaManager.IsPrepared()
                 && !readyTodispose)
            {
                await this.mediaManager.Stop();
            }

            currentScheduleId = 0;
            readyTodispose = true;
        }



        private async void stateChanged(object sender, StateChangedEventArgs e)
        {
            var mediaItem = this.mediaManager.Queue.Current;
            if (currentlyPlaying.ContainsKey(mediaItem))
            {
                var track = currentlyPlaying[mediaItem];

                switch (e.State)
                {
                    case MediaPlayerState.Playing:
                        if (track.FinishedDuration.TotalSeconds > 0
                            && firstChapter != null
                            && mediaItem == firstChapter)
                        {
                            await this.mediaManager.SeekTo(track.FinishedDuration);
                            firstChapter = null;
                        }
                        break;
                }
            }

        }

        private async void markTrackAsFinished(object sender, MediaItemEventArgs e)
        {
            await @lock.WaitAsync();

            try
            {
                if (currentlyPlaying.ContainsKey(e.MediaItem))
                {
                    var track = currentlyPlaying[e.MediaItem];

                    if (track.IsLastTrack)
                    {
                        await playlistService.MarkTrackAsFinished(track);
                        await Dismiss();
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error happened when marking track as finished.");
            }
            finally
            {
                @lock.Release();
            }
        }

        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                this.mediaManager.MediaItemFinished -= markTrackAsFinished;

                this.playlistService.Dispose();
                this.alarmService.Dispose();
                this.cacheService.Dispose();
                this.storageService.Dispose();
                this.networkStatusService.Dispose();
                this.notificationService.Dispose();

                @lock.Dispose();
            }
        }
    }
}
