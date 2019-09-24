
using AVFoundation;
using Bible.Alarm.Models;
using Bible.Alarm.Services.Contracts;
using MediaManager.Playback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.iOS
{
    public class PlaybackService :
        IPlaybackService
    {
        private AVQueuePlayer player;
        private IPlaylistService playlistService;
        private IMediaCacheService cacheService;
        private IAlarmService alarmService;

        private NotificationDetail currentTrackDetail;
        private List<PlayItem> playList;
        private int playIndex = 0;

        public PlaybackService(AVQueuePlayer player, IPlaylistService playlistService,
            IMediaCacheService cacheService, IAlarmService alarmService)
        {
            this.player = player;
            this.playlistService = playlistService;
            this.cacheService = cacheService;
            this.alarmService = alarmService;
        }

        public event EventHandler<MediaManager.Player.MediaPlayerState> StateChanged;

        public void Dismiss()
        {
            throw new NotImplementedException();
        }

        public async Task Play(long scheduleId)
        {
            throw new NotImplementedException();
        }

        public async Task Snooze()
        {
            throw new NotImplementedException();
        }

      
    }
}
