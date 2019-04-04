using Android.App;
using Android.Content;
using Android.Media;
using Android.Runtime;
using Java.IO;
using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Droid
{
    public class PlaybackService :
        Java.Lang.Object,
        MediaPlayer.IOnCompletionListener,
        MediaPlayer.IOnErrorListener,
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
        }

        public async Task Play(long scheduleId, Context context = null)
        {
            //another alarm is already in effect
            if (currentTrackDetail != null)
            {
                return;
            }

            playList = await playlistService.NextTracks(scheduleId, TimeSpan.FromHours(1));

            var path = cacheService.GetCacheFilePath(playList[playIndex].Url);
            var file = new File(path);

            if (file.Exists() && file.CanRead())
            {

            }
            this.player = new MediaPlayer();
            this.player.SetDataSource(context,
                Android.Net.Uri.FromFile(file));

            currentTrackDetail = playList[playIndex].PlayDetail;
            this.player.SetOnCompletionListener(this);
            this.player.SetOnErrorListener(this);

            this.player.Prepare();
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

            this.player.Prepare();
            this.player.Start();
        }

        public bool OnError(MediaPlayer mp, [GeneratedEnum] MediaError what, int extra)
        {
            System.Diagnostics.Debug.WriteLine("OnError()");

            System.Diagnostics.Debug.WriteLine(what);
            mp.Reset();

            return true;
        }
    }
}
