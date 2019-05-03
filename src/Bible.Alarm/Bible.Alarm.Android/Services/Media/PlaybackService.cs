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
    public class PlaybackService : IPlaybackService
    {
        private readonly IMediaManager mediaManager;
        private IAlarmService alarmService;
        private IPlaylistService playlistService;
        private IMediaCacheService cacheService;

        private long currentScheduleId;
        public PlaybackService(IMediaManager mediaManager,
            IPlaylistService playlistService,
            IAlarmService alarmService,
            IMediaCacheService cacheService)
        {
            this.mediaManager = mediaManager;
            this.playlistService = playlistService;
            this.alarmService = alarmService;
            this.cacheService = cacheService;
        }

        public async void Dismiss()
        {
            if (this.mediaManager.IsPlaying())
            {
                await this.mediaManager.Stop();
            }

            this.currentScheduleId = 0;
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

                await this.mediaManager.Play(nextTrackUris);
            }
        }

        public async Task Snooze()
        {
            if (this.mediaManager.IsPlaying())
            {
                await this.mediaManager.Stop();
                await this.alarmService.Snooze(this.currentScheduleId);
            }

        }
 
    }
}
