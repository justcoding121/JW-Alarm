using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Uwp
{
    public class UwpAlarmService : IAlarmService
    {
        private INotificationService notificationService;
        private IPlaylistService mediaPlayService;
        private IMediaCacheService mediaCacheService;

        public UwpAlarmService(IDatabase database,
            INotificationService notificationService,
            IPlaylistService mediaPlayService,
            IMediaCacheService mediaCacheService)
        {
            this.notificationService = notificationService;
            this.mediaPlayService = mediaPlayService;
            this.mediaCacheService = mediaCacheService;
        }

        public async Task Create(AlarmSchedule schedule)
        {
            var nextTrack = await mediaPlayService.NextTrack(schedule.Id);
            nextTrack.PlayDetail.NotificationTime = schedule.NextFireDate();
            await scheduleNotification(schedule, nextTrack, nextTrack.PlayDetail.NotificationTime);
            var task = Task.Run(async () => await mediaCacheService.SetupAlarmCache(schedule.Id));
        }

        public async Task Create(AlarmSchedule schedule, PlayDetail playDetail)
        {
            var nextTrack = await mediaPlayService.NextTrack(playDetail);
            nextTrack.PlayDetail.NotificationTime = DateTimeOffset.Now.Add(playDetail.Duration);
            await scheduleNotification(schedule, nextTrack, nextTrack.PlayDetail.NotificationTime);
            var task = Task.Run(async () => await mediaCacheService.SetupAlarmCache(schedule.Id));
        }

        public Task Delete(int scheduleId)
        {
            removeNotification(scheduleId);
            return Task.FromResult(false);
        }

        public async Task Update(AlarmSchedule schedule)
        {
            if (schedule.IsEnabled)
            {
                removeNotification(schedule.Id);
                await scheduleNotification(schedule, await mediaPlayService.NextTrack(schedule.Id), schedule.NextFireDate());
                var task = Task.Run(async () => await mediaCacheService.SetupAlarmCache(schedule.Id));
            }
            else
            {
                removeNotification(schedule.Id);
            }
        }

        private async Task scheduleNotification(AlarmSchedule schedule, PlayItem track, DateTimeOffset nextFire)
        {
            await notificationService.Add(schedule.Id, track.PlayDetail,
                  nextFire, schedule.Name, schedule.Name, track.Url);
        }

        private void removeNotification(int scheduleId)
        {
            notificationService.Remove(scheduleId);
        }
    }
}
