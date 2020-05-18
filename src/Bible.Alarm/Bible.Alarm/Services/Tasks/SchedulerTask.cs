﻿using Bible.Alarm.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Tasks
{
    public class SchedulerTask : IDisposable
    {
        private Logger logger => LogManager.GetCurrentClassLogger();
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
        public async Task<bool> Handle()
        {
            var downloaded = false;
            if (await @lock.WaitAsync(1000))
            {
                try
                {
                    await mediaCacheService.CleanUp();
                    var schedules = await scheduleDbContext.AlarmSchedules.Where(x => x.IsEnabled).ToListAsync();
                    foreach (var schedule in schedules)
                    {
                        if (!await notificationService.IsScheduled(schedule.Id))
                        {
                            downloaded = true;
                            await alarmService.Create(schedule);
                        }
                        else
                        {
                            downloaded = await mediaCacheService.SetupAlarmCache(schedule.Id);
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
            return downloaded;
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