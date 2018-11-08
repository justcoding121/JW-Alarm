using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Uwp
{
    public class UwpScheduleService : IAlarmService
    {
        private INotificationService notificationService;
        private IPlaylistService mediaPlayService;
        private IMediaCacheService mediaCacheService;

        public UwpScheduleService(IDatabase database,
            INotificationService notificationService, IPlaylistService mediaPlayService,
            IMediaCacheService mediaCacheService)
        {
            this.notificationService = notificationService;
            this.mediaPlayService = mediaPlayService;
            this.mediaCacheService = mediaCacheService;
        }

        public async Task Create(AlarmSchedule schedule)
        {
            await scheduleNotification(schedule);
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
                await scheduleNotification(schedule);
                var task = Task.Run(async () => await mediaCacheService.SetupAlarmCache(schedule.Id));
            }
            else
            {
                removeNotification(schedule.Id);
            }
        }

        private async Task scheduleNotification(AlarmSchedule schedule)
        {
            notificationService.Remove(schedule.Id);

            var track = await mediaPlayService.NextTrack(schedule.Id);
            var nextFire = schedule.NextFireDate();

            notificationService.Add(schedule.Id, JsonConvert.SerializeObject(track.PlayDetail),
                nextFire, schedule.Name, schedule.Name, track.Url);
        }

        private void removeNotification(int scheduleId)
        {
            notificationService.Remove(scheduleId);
        }
    }
}
