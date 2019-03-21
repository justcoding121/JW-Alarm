using Bible.Alarm.ViewModels.Redux.Actions.Bible;
using Bible.Alarm.ViewModels.Redux.Actions.Music;
using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using JW.Alarm.ViewModels.Redux;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace JW.Alarm.Services.UWP
{
    public class PlaybackService : IPlaybackService
    {
        private MediaPlayer player;
        private IPlaylistService playlistService;
        private IMediaCacheService cacheService;
        private IAlarmService alarmService;
        private ScheduleDbContext scheduleDbContext;

        private NotificationDetail currentTrackDetail;

        public PlaybackService(MediaPlayer player, IPlaylistService playlistService,
            IMediaCacheService cacheService, IAlarmService alarmService, ScheduleDbContext scheduleDbContext)
        {
            this.player = player;
            this.playlistService = playlistService;
            this.cacheService = cacheService;
            this.alarmService = alarmService;
            this.scheduleDbContext = scheduleDbContext;
        }

        public void Dismiss()
        {
            this.player.Pause();
            currentTrackDetail = null;
        }

        public async Task Play(long scheduleId)
        {
            //another alarm is already in effect
            if (currentTrackDetail != null)
            {
                return;
            }

            var nextTracks = await playlistService.NextTracks(scheduleId, TimeSpan.FromHours(1));

            this.player.Source = toPlaybackList(nextTracks);
            this.player.Play();
        }

        public async Task Snooze()
        {
            this.player.Pause();
            await this.alarmService.Snooze(currentTrackDetail.ScheduleId);
            currentTrackDetail = null;
        }

        private async void trackChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (args.NewItem != null)
            {
                var playbackItem = args.NewItem;

                var detail = playbackItem.Source.CustomProperties["playDetail"] as NotificationDetail;

                if (currentTrackDetail != null)
                {
                    await playlistService.MarkTrackAsFinished(currentTrackDetail);
                }

                currentTrackDetail = detail;

            }

        }


        private MediaPlaybackList toPlaybackList(List<PlayItem> playItems)
        {
            var playbackList = new MediaPlaybackList();

            playbackList.AutoRepeatEnabled = false;

            // Add playback items to the list
            foreach (var mediaItem in playItems)
            {
                playbackList.Items.Add(toPlaybackItem(mediaItem));
            }

            playbackList.CurrentItemChanged += trackChanged;

            return playbackList;
        }


        private MediaPlaybackItem toPlaybackItem(PlayItem playItem)
        {
            var src = cacheService.GetCacheFilePath(playItem.Url);

            // Create the media source from the Uri
            var source = MediaSource.CreateFromUri(new Uri(src));

            // Create a configurable playback item backed by the media source
            var playbackItem = new MediaPlaybackItem(source);

            source.CustomProperties["playDetail"] = playItem.PlayDetail;

            return playbackItem;
        }

    }
}
