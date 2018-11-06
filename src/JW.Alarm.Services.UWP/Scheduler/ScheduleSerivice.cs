using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace JW.Alarm.Services.Uwp
{
    public class UwpScheduleService : IAlarmService
    {
        private INotificationService notificationService;
        private IBibleReadingScheduleService bibleReadingScheduleService;
        private IMediaPlayService mediaPlayService;
        private IMediaCacheService mediaCacheService;

        public UwpScheduleService(IDatabase database,
            IBibleReadingScheduleService bibleReadingScheduleService,
            INotificationService notificationService, IMediaPlayService mediaPlayService,
            IMediaCacheService mediaCacheService)
        {
            this.notificationService = notificationService;
            this.bibleReadingScheduleService = bibleReadingScheduleService;
            this.mediaPlayService = mediaPlayService;
            this.mediaCacheService = mediaCacheService;
        }

        public async Task Create(AlarmSchedule schedule)
        {
            await createNotifications(schedule);
            var task = Task.Run(async () => await mediaCacheService.SetupAlarmCache(schedule.Id));
        }

        public Task Delete(int scheduleId)
        {
            removeNotifications(scheduleId);
            return Task.FromResult(false);
        }

        public async Task Update(AlarmSchedule schedule)
        {
            if (schedule.IsEnabled)
            {
                removeNotifications(schedule.Id);
                await createNotifications(schedule);
                var task = Task.Run(async () => await mediaCacheService.SetupAlarmCache(schedule.Id));
            }
            else
            {
                removeNotifications(schedule.Id);
            }
        }

        private async Task createNotifications(AlarmSchedule schedule)
        {
            notificationService.Clear();

            var duration = new TimeSpan(3, 0, 0);
            var items = await mediaPlayService.ItemsToPlay(schedule.Id, duration);
            var nextFire = schedule.NextFireDate();
            foreach (var item in items)
            {
                notificationService.Add(schedule.Id.ToString(), nextFire, schedule.Name, schedule.Name, item.Url);
                nextFire = nextFire.AddSeconds(item.Duration.TotalSeconds + 3);
            }

        }

        private void removeNotifications(int scheduleId)
        {
            notificationService.Remove(scheduleId.ToString());
        }

    }
}
