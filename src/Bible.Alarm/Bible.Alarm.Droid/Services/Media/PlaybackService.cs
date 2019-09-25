using Advanced.Algorithms.DataStructures.Foundation;
using Android.App;
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private long currentScheduleId;
        private Dictionary<IMediaItem, NotificationDetail> currentlyPlaying
            = new Dictionary<IMediaItem, NotificationDetail>();

        private bool disposed;

        public event EventHandler<MediaPlayerState> StateChanged;

        public PlaybackService(IMediaManager mediaManager,
            IPlaylistService playlistService,
            IAlarmService alarmService,
            IMediaCacheService cacheService,
            IStorageService storageService,
            INetworkStatusService networkStatusService)
        {
            this.mediaManager = mediaManager;
            this.playlistService = playlistService;
            this.alarmService = alarmService;
            this.cacheService = cacheService;
            this.storageService = storageService;
            this.networkStatusService = networkStatusService;

            this.mediaManager.MediaItemChanged += markTrackAsPlayed;
            this.mediaManager.MediaItemFinished += markTrackAsFinished;
            this.mediaManager.StateChanged += stateChanged;

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    if (disposed)
                    {
                        Dispose();
                        break;
                    }
                }
            });
        }

        private void stateChanged(object sender, StateChangedEventArgs e)
        {
            StateChanged?.Invoke(this, e.State);

            //var notificationManager = (this.mediaManager as MediaManagerImplementation).Notification
            //    as MediaManager.Platforms.Android.Notifications.NotificationManager;

            //notificationManager.PlayerNotificationManager.SetOngoing(false);
            //notificationManager.PlayerNotificationManager.Invalidate();
        }

        private async void markTrackAsPlayed(object sender, MediaItemEventArgs e)
        {
            if (currentlyPlaying.ContainsKey(e.MediaItem))
            {
                var track = currentlyPlaying[e.MediaItem];
                await playlistService.MarkTrackAsPlayed(track);
            }
        }

        private async void markTrackAsFinished(object sender, MediaItemEventArgs e)
        {
            if (currentlyPlaying.ContainsKey(e.MediaItem))
            {
                var track = currentlyPlaying[e.MediaItem];
                await playlistService.MarkTrackAsFinished(track);
            }
        }

        public async void Dismiss()
        {
            if (this.mediaManager.IsPlaying() && !disposed)
            {
                await this.mediaManager.Stop();
            }

            this.currentScheduleId = 0;
            disposed = true;
        }

        public async Task Play(long scheduleId)
        {
            if (!this.mediaManager.IsPlaying())
            {
                this.mediaManager.RepeatMode = RepeatMode.Off;

                this.currentScheduleId = scheduleId;

                var nextTracks = await playlistService.NextTracks(scheduleId, TimeSpan.FromHours(1));

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

                this.mediaManager.Volume.CurrentVolume = this.mediaManager.Volume.MaxVolume;
                await this.mediaManager.Play(mergedMediaItems.Select(x => x.Value));

                currentlyPlaying = new Dictionary<IMediaItem, NotificationDetail>();

                foreach (var track in mergedMediaItems)
                {
                    currentlyPlaying.Add(track.Value, playDetailMap[track.Key]);
                }
            }
        }

        public async Task Snooze()
        {
            if (this.mediaManager.IsPlaying() && !disposed)
            {
                await this.mediaManager.Stop();
                await this.alarmService.Snooze(this.currentScheduleId);
            }

            disposed = true;
        }

        public void Dispose()
        {
            this.mediaManager.MediaItemChanged -= markTrackAsPlayed;
            this.mediaManager.MediaItemFinished -= markTrackAsFinished;
        }
    }
}
