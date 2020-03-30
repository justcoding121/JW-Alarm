using Bible.Alarm.iOS.Services.Handlers;
using Bible.Alarm.Models.Schedule;
using Bible.Alarm.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.iOS
{
    public class iOSNotificationService : INotificationService
    {
        private readonly IContainer container;
        private readonly ScheduleDbContext dbContext;

        public iOSNotificationService(IContainer container, ScheduleDbContext dbContext)
        {
            this.container = container;
            this.dbContext = dbContext;
        }

        public async void ShowNotification(long scheduleId)
        {
            var iosAlarmHandler = container.Resolve<iOSAlarmHandler>();
            await iosAlarmHandler.Handle(scheduleId);
        }

        public async Task ScheduleNotification(long scheduleId, DateTimeOffset time,
            string title, string body)
        {
            var alarmSchedule = await dbContext.AlarmSchedules
                .Include(x => x.AlarmNotifications)
                .FirstAsync(x => x.Id == scheduleId);

            var notification = new Models.Schedule.AlarmNotification()
            {
                AlarmScheduleId = alarmSchedule.Id,
                Fired = false,
                ScheduledTime = alarmSchedule.NextFireDate(),
                Sent = false
            };

            alarmSchedule.AlarmNotifications.Add(notification);
            await dbContext.SaveChangesAsync();

            alarmSchedule.LatestAlarmNotificationId = notification.Id;
            await dbContext.SaveChangesAsync();


            //Send to server
        }

        public async Task Remove(long scheduleId)
        {
            var alarmSchedule = await dbContext.AlarmSchedules.FirstAsync(x => x.Id == scheduleId);
            var notification = await dbContext.AlarmNotifications
                                .Where(x => x.Id == alarmSchedule.LatestAlarmNotificationId)
                                .FirstOrDefaultAsync();

            if (notification != null)
            {
                notification.CancellationRequested = true;

                await dbContext.SaveChangesAsync();

                //Send to server

                notification.Cancelled = true;
                await dbContext.SaveChangesAsync();

            }

        }

        public async Task<bool> IsScheduled(long scheduleId)
        {
            var alarmSchedule = await dbContext.AlarmSchedules.FirstAsync(x => x.Id == scheduleId);
            var notification = await dbContext.AlarmNotifications
                                .Where(x => x.Id == alarmSchedule.LatestAlarmNotificationId)
                                .FirstOrDefaultAsync();

            var nextFireDate = alarmSchedule.NextFireDate();

            if (notification != null
                && !notification.CancellationRequested
                && notification.ScheduledTime.Equals(nextFireDate))
            {
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            dbContext.Dispose();
        }
    }

}
