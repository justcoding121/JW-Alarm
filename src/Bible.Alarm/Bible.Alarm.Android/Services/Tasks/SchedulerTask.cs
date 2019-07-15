using Bible.Alarm.Services.Infrastructure;
using JW.Alarm.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLog;

namespace JW.Alarm.Services.Droid.Tasks
{
    public class SchedulerTask
    {
        private static Logger logger => LogManager.GetCurrentClassLogger();

        private ScheduleDbContext scheduleDbContext;
        private IMediaCacheService mediaCacheService;
        private IAlarmService alarmService;
        private INotificationService notificationService;

        public SchedulerTask(ScheduleDbContext scheduleDbContext, IMediaCacheService mediaCacheService,
              IAlarmService alarmService, INotificationService notificationService)
        {
            LogSetup.Initialize();

            this.scheduleDbContext = scheduleDbContext;
            this.mediaCacheService = mediaCacheService;
            this.alarmService = alarmService;
            this.notificationService = notificationService;
        }

        public async Task Handle()
        {
            try
            {
                await mediaCacheService.CleanUp();

                var schedules = await scheduleDbContext.AlarmSchedules.Where(x => x.IsEnabled).ToListAsync();

                foreach (var schedule in schedules)
                {
                    if (!notificationService.IsScheduled(schedule.Id))
                    {
                        await alarmService.Create(schedule, true);
                    }
                    else
                    {
                        await mediaCacheService.SetupAlarmCache(schedule.Id);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to process scheduler task.");
            }
        }
    }
}
