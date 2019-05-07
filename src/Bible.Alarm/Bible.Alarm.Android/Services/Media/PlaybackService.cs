using Android.App;
using Android.Content;
using Android.Media;
using Android.Runtime;
using Java.IO;
using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using MediaManager;
using MediaManager.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Droid
{
    public class PlaybackService : IPlaybackService, IDisposable
    {
        private readonly IMediaManager mediaManager;
        private IAlarmService alarmService;
        private IPlaylistService playlistService;
        private IMediaCacheService cacheService;

        private long currentScheduleId;
        private Dictionary<IMediaItem, NotificationDetail> currentlyPlaying
            = new Dictionary<IMediaItem, NotificationDetail>();

        private bool disposed;

        public PlaybackService(IMediaManager mediaManager,
            IPlaylistService playlistService,
            IAlarmService alarmService,
            IMediaCacheService cacheService)
        {
            this.mediaManager = mediaManager;
            this.playlistService = playlistService;
            this.alarmService = alarmService;
            this.cacheService = cacheService;

            this.mediaManager.MediaItemChanged += markTrackAsPlayed;
            this.mediaManager.MediaItemFinished += markTrackAsFinished;

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
                this.currentScheduleId = scheduleId;
                var nextTracks = await playlistService.NextTracks(scheduleId, TimeSpan.FromHours(1));

                var nextTrackUris = nextTracks
                    .Select(x => this.cacheService.GetCacheFilePath(x.Url))
                    .ToList();

                var mediaItems = (await this.mediaManager.Play(nextTrackUris)).ToList();

                currentlyPlaying = new Dictionary<IMediaItem, NotificationDetail>();
                for (int i = 0; i < nextTracks.Count; i++)
                {
                    var track = nextTracks[i];
                    var mediaItem = mediaItems[i];
                    currentlyPlaying.Add(mediaItem, track.PlayDetail);
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
