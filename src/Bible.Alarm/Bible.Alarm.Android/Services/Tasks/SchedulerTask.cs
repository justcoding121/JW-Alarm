﻿using JW.Alarm.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Windows.ApplicationModel.Background;

namespace JW.Alarm.Services.Droid.Tasks
{
    public class SchedulerTask
    {
        private ScheduleDbContext scheduleDbContext;
        private IMediaCacheService mediaCacheService;
        private IAlarmService alarmService;
        private INotificationService notificationService;

        public SchedulerTask(ScheduleDbContext scheduleDbContext, IMediaCacheService mediaCacheService,
              IAlarmService alarmService, INotificationService notificationService)
        {
            this.scheduleDbContext = scheduleDbContext;
            this.mediaCacheService = mediaCacheService;
            this.alarmService = alarmService;
            this.notificationService = notificationService;
        }

        public async void Handle(IBackgroundTaskInstance backgroundTask)
        {
            var deferral = backgroundTask.GetDeferral();

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

         
            deferral.Complete();
        }
    }
}