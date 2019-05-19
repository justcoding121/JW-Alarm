using Android.App;
using Android.Content;
using Android.Media;
using Android.Runtime;
using Java.IO;
using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using MediaManager;
using MediaManager.Media;
using MediaManager.Playback;
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

        public event EventHandler<MediaPlayerState> StateChanged;

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

                await cacheService.SetupAlarmCache(scheduleId);

                var nextTracks = await playlistService.NextTracks(scheduleId, TimeSpan.FromHours(1));

                //play default ring tone if we don't have the files downloaded.
                if (await nextTracks.AnyAsync(async x => !await cacheService.Exists(x.Url)))
                {
                    var notification = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);

                    if (notification == null)
                    {
                        // alert is null, using backup
                        notification = RingtoneManager.GetDefaultUri(RingtoneType.Notification);

                        // I can't see this ever being null (as always have a default notification)
                        // but just incase
                        if (notification == null)
                        {
                            // alert backup is null, using 2nd backup
                            notification = RingtoneManager.GetDefaultUri(RingtoneType.Ringtone);
                        }
                    }

                    var r = RingtoneManager.GetRingtone(Application.Context, notification);
                    r.Play();

                    return;
                }

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
