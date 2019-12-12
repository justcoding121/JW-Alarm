using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NLog;
using System.Threading;

namespace Bible.Alarm.Services.Droid.Tasks
{
    public class SchedulerTask : IDisposable
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        private ScheduleDbContext scheduleDbContext;
        private IMediaCacheService mediaCacheService;
        private IAlarmService alarmService;
        private INotificationService notificationService;

        private static SemaphoreSlim @lock = new SemaphoreSlim(1);

        public SchedulerTask(ScheduleDbContext scheduleDbContext, IMediaCacheService mediaCacheService,
              IAlarmService alarmService, INotificationService notificationService)
        {

            this.scheduleDbContext = scheduleDbContext;
            this.mediaCacheService = mediaCacheService;
            this.alarmService = alarmService;
            this.notificationService = notificationService;
        }

        public async Task Handle()
        {
            if (await @lock.WaitAsync(1000))
            {
                try
                {
                    await mediaCacheService.CleanUp();

                    var schedules = await scheduleDbContext.AlarmSchedules.Where(x => x.IsEnabled).ToListAsync();

                    foreach (var schedule in schedules)
                    {
                        if (!notificationService.IsScheduled(schedule.Id))
                        {
                            await alarmService.Create(schedule);
                        }
                        else
                        {
                            await mediaCacheService.SetupAlarmCache(schedule.Id);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to process scheduler task. Db directory: {Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}");
                }
                finally
                {
                    @lock.Release();
                }
            }
        }

        public void Dispose()
        {
            scheduleDbContext.Dispose();
            mediaCacheService.Dispose();
            alarmService.Dispose();
            notificationService.Dispose();
        }

    }
}
