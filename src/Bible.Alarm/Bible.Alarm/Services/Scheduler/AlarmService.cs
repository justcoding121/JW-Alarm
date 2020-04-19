using Bible.Alarm.Models;
using Bible.Alarm.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Threading.Tasks;

namespace Bible.Alarm.Services
{
    public class AlarmService : IAlarmService
    {
        private readonly IContainer container;
        private INotificationService notificationService;
        private IMediaCacheService mediaCacheService;
        private ScheduleDbContext scheduleDbContext;

        private Logger logger => LogManager.GetCurrentClassLogger();

        public AlarmService(IContainer container,
            INotificationService notificationService,
            IMediaCacheService mediaCacheService,
            ScheduleDbContext scheduleDbContext)
        {
            this.container = container;
            this.notificationService = notificationService;
            this.mediaCacheService = mediaCacheService;
            this.scheduleDbContext = scheduleDbContext;
        }

        public Task Create(AlarmSchedule schedule)
        {
            scheduleNotification(schedule, false);
            return Task.CompletedTask;
        }

        public void Update(AlarmSchedule schedule)
        {
            removeNotification(schedule.Id);

            if (schedule.IsEnabled)
            {
                scheduleNotification(schedule, false);
            }
        }

        public async Task Snooze(long scheduleId)
        {
            var schedule = await scheduleDbContext.
                AlarmSchedules
                .AsNoTracking()
                .FirstAsync(x => x.Id == scheduleId);

            scheduleNotification(schedule, true);
        }

        public void Delete(long scheduleId)
        {
            removeNotification(scheduleId);
        }

        private void scheduleNotification(AlarmSchedule schedule, bool isSnoozeNotification)
        {
            notificationService.ScheduleNotification(schedule.Id, isSnoozeNotification ?
                DateTimeOffset.Now.AddMinutes(schedule.SnoozeMinutes) : schedule.NextFireDate(), schedule.Name,
                schedule.MusicEnabled ? "Playing alarm music." : "Playing Bible.");
        }

        private void removeNotification(long scheduleId)
        {
            notificationService.Remove(scheduleId);
        }

        public void Dispose()
        {
            scheduleDbContext.Dispose();
            notificationService.Dispose();
            mediaCacheService.Dispose();
        }
    }
}
