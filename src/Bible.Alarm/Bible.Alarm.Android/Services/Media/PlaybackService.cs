using Android.App;
using Android.Media;
using Java.IO;
using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.UWP
{
    public class PlaybackService :
        Java.Lang.Object,
        MediaPlayer.IOnCompletionListener,
        IPlaybackService
    {
        private MediaPlayer player;
        private IPlaylistService playlistService;
        private IMediaCacheService cacheService;
        private IAlarmService alarmService;

        private NotificationDetail currentTrackDetail;
        private List<PlayItem> playList;
        private int playIndex = 0;

        public PlaybackService(MediaPlayer player, IPlaylistService playlistService,
            IMediaCacheService cacheService, IAlarmService alarmService)
        {
            this.player = player;
            this.playlistService = playlistService;
            this.cacheService = cacheService;
            this.alarmService = alarmService;
        }

        public void Dismiss()
        {
            this.player.Stop();
            currentTrackDetail = null;
        }

        public async Task Play(long scheduleId)
        {
            //another alarm is already in effect
            if (currentTrackDetail != null)
            {
                return;
            }

            playList = await playlistService.NextTracks(scheduleId, TimeSpan.FromHours(1));

            this.player.SetDataSource(Application.Context,
                Android.Net.Uri.FromFile(new File(playList[playIndex].Url)));

            currentTrackDetail = playList[playIndex].PlayDetail;
            this.player.SetOnCompletionListener(this);
            this.player.Start();
        }

        public async Task Snooze()
        {
            this.player.Stop();
            await this.alarmService.Snooze(currentTrackDetail.ScheduleId);
            currentTrackDetail = null;
        }

        public async void OnCompletion(MediaPlayer mp)
        {
            await playlistService.MarkTrackAsFinished(currentTrackDetail);

            playIndex++;

            if (playIndex == playList.Count)
            {
                return;
            }

            this.player.SetDataSource(Application.Context,
                Android.Net.Uri.FromFile(new File(playList[playIndex].Url)));

            currentTrackDetail = playList[playIndex].PlayDetail;
            this.player.Start();
        }

    }
}
