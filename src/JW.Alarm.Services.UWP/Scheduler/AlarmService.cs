using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Uwp
{
    public class UwpAlarmService : IAlarmService
    {
        private INotificationService notificationService;
        private IPlaylistService playlistService;
        private IMediaCacheService mediaCacheService;

        public UwpAlarmService(IDatabase database,
            INotificationService notificationService,
            IPlaylistService mediaPlayService,
            IMediaCacheService mediaCacheService)
        {
            this.notificationService = notificationService;
            this.playlistService = mediaPlayService;
            this.mediaCacheService = mediaCacheService;
        }

        public async Task Create(AlarmSchedule schedule)
        {
            await removeNotification(0);

            var nextTrack = await playlistService.NextTrack(schedule.Id);
            nextTrack.PlayDetail.NotificationTime = schedule.NextFireDate();
            await scheduleNotification(schedule, nextTrack);
            var task = Task.Run(async () => await mediaCacheService.SetupAlarmCache(schedule.Id));
        }

        public async Task Update(AlarmSchedule schedule)
        {
            if (schedule.IsEnabled)
            {
                await removeNotification(schedule.Id);
                var nextTrack = await playlistService.NextTrack(schedule.Id);
                nextTrack.PlayDetail.NotificationTime = schedule.NextFireDate();
                await scheduleNotification(schedule, nextTrack);
                var task = Task.Run(async () => await mediaCacheService.SetupAlarmCache(schedule.Id));
            }
            else
            {
                await removeNotification(schedule.Id);
            }
        }

        public async Task ScheduleNextTrack(AlarmSchedule schedule, NotificationDetail detail)
        {
            var nextTrack = await playlistService.NextTrack(detail);
            nextTrack.PlayDetail.NotificationTime = DateTimeOffset.Now.AddSeconds(3);
            await scheduleNotification(schedule, nextTrack);
        }

        public async Task Delete(long scheduleId)
        {
            await removeNotification(scheduleId);
        }

        private async Task scheduleNotification(AlarmSchedule schedule, PlayItem currentTrack)
        {
            var fireDate = currentTrack.PlayDetail.NotificationTime;

            await notificationService.Add(schedule.Id.ToString(), currentTrack.PlayDetail,
                fireDate, schedule.Name, schedule.Name, currentTrack.Url);

            fireDate = fireDate.Add(TimeSpan.FromSeconds(5)).AddSeconds(1);
            notificationService.AddSilent("Clear", fireDate);
        }

        private async Task removeNotification(long scheduleId)
        {
            await notificationService.Remove(scheduleId);
        }
    }
}
