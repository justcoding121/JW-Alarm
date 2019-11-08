using Advanced.Algorithms.DataStructures.Foundation;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Net;
using Bible.Alarm.Contracts.Network;
using Bible.Alarm.Models;
using Bible.Alarm.Services.Contracts;
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
        private readonly IMediaManager mediaManager;
        private IAlarmService alarmService;
        private IPlaylistService playlistService;
        private IMediaCacheService cacheService;
        private IStorageService storageService;
        private INetworkStatusService networkStatusService;
        private INotificationService notificationService;

        private long currentScheduleId;
        private Dictionary<IMediaItem, NotificationDetail> currentlyPlaying
            = new Dictionary<IMediaItem, NotificationDetail>();

        private SemaphoreSlim @lock = new SemaphoreSlim(1);

        private IMediaItem firstChapter;

        private bool disposed;

        public event EventHandler<MediaPlayerState> StateChanged;

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

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);

                    await @lock.WaitAsync();

                    try
                    {
                        if (this.mediaManager.IsPlaying())
                        {
                            var mediaItem = this.mediaManager.Queue.Current;
                            if (currentlyPlaying.ContainsKey(mediaItem))
                            {
                                var track = currentlyPlaying[mediaItem];

                                if (!mediaManager.Position.Equals(default(TimeSpan)))
                                {
                                    track.FinishedDuration = mediaManager.Position;
                                    await this.playlistService.MarkTrackAsPlayed(track);
                                }
                            }
                        }
                    }
                    finally
                    {
                        @lock.Release();
                    }

                    if (disposed)
                    {
                        Dispose();
                        break;
                    }
                }
            });
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
                        if (!track.FinishedDuration.Equals(default(TimeSpan)) && mediaItem == firstChapter)
                        {
                            await this.mediaManager.SeekTo(track.FinishedDuration);
                            firstChapter = null;
                        }
                        break;
                }
            }


            StateChanged?.Invoke(this, e.State);
        }

        private async void markTrackAsFinished(object sender, MediaItemEventArgs e)
        {
            await @lock.WaitAsync();

            try
            {
                if (currentlyPlaying.ContainsKey(e.MediaItem))
                {
                    var track = currentlyPlaying[e.MediaItem];
                    await playlistService.MarkTrackAsFinished(track);

                    this.notificationService.ClearAll();
                }
            }
            finally
            {
                @lock.Release();
            }
        }

        public async void Dismiss()
        {
            if (this.mediaManager.IsPrepared()
                 && !disposed)
            {
                await this.mediaManager.Stop();
            }

            this.currentScheduleId = 0;
            disposed = true;
        }

        public async Task Play(long scheduleId)
        {
            await @lock.WaitAsync();

            try
            {
                if (!this.mediaManager.IsPrepared())
                {
                    this.currentScheduleId = scheduleId;

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

                    var downloadedMediaItems = (await downloadedTracks.Select(x => x.Value).CreateMediaItems()).ToList();
                    var streamableMediaItems = (await streamingTracks.Select(x => x.Value).CreateMediaItems()).ToList();

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
                    if (!mergedMediaItems.Any())
                    {
                        this.mediaManager.RepeatMode = RepeatMode.All;
                        var item = await this.mediaManager.Play(new FileInfo(Path.Combine(this.storageService.StorageRoot, "cool-alarm-tone-notification-sound.mp3")));
                        return;
                    }

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
        }

        public async Task Snooze()
        {
            await @lock.WaitAsync();

            try
            {
                if (this.mediaManager.IsPrepared() && !disposed)
                {
                    await this.mediaManager.Stop();
                    await this.alarmService.Snooze(this.currentScheduleId);
                }
            }
            finally
            {
                @lock.Release();
            }

            disposed = true;
        }

        public void Dispose()
        {
            this.mediaManager.MediaItemFinished -= markTrackAsFinished;
        }
    }
}
