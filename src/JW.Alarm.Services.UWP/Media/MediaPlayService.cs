using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace JW.Alarm.Services.UWP
{

    public class UwpMediaPlayService : MediaPlayService
    {

        private Dictionary<int, MediaPlayer> alarmToMediaPlayersMap = new Dictionary<int, MediaPlayer>();
        private Dictionary<MediaPlayer, int> mediaPlayersToAlarmMap = new Dictionary<MediaPlayer, int>();

        public UwpMediaPlayService(IScheduleService scheduleService, MediaService mediaService)
            : base(scheduleService, mediaService)
        {
        }

        public override async Task Play(int scheduleId)
        {
            var nextPlayItem = await NextUrlToPlay(scheduleId);

            var mediaPlayer = new MediaPlayer();
            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(nextPlayItem.Url));
            mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(nextPlayItem.Second);
            mediaPlayer.PlaybackSession.BufferingEnded += onTrackEnd;
            mediaPlayer.Play();

            alarmToMediaPlayersMap.Add(scheduleId, mediaPlayer);
            mediaPlayersToAlarmMap.Add(mediaPlayer, scheduleId);
        }

        //move to next track on track end
        private async void onTrackEnd(MediaPlaybackSession sender, object args)
        {
            var mediaPlayer = sender.MediaPlayer;

            var scheduleId = mediaPlayersToAlarmMap[mediaPlayer];
           
            SetNextItemToPlay(scheduleId).Wait();

            var nextPlayItem = await NextUrlToPlay(scheduleId);

            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(nextPlayItem.Url));
            mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(nextPlayItem.Second);
            mediaPlayer.Play();
        }


        public override async Task Stop(AlarmSchedule schedule)
        {
            var mediaPlayer = alarmToMediaPlayersMap[schedule.Id];
            var second = mediaPlayer.PlaybackSession.Position.TotalSeconds;
            await UpdatePlayedSeconds(schedule, (int)second < 0 ? 0 : (int)second);
            mediaPlayer.Dispose();

            await base.Stop(schedule);
        }


    }
}
