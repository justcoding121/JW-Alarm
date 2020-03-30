using Bible.Alarm.Common.Helpers;
using Bible.Alarm.iOS.Models;
using Bible.Alarm.iOS.Services.Handlers;
using Bible.Alarm.Models.Schedule;
using Bible.Alarm.Services.Contracts;
using Foundation;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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

            var alarmTime = alarmSchedule.NextFireDate();
            var notification = new Models.Schedule.AlarmNotification()
            {
                AlarmScheduleId = alarmSchedule.Id,
                Fired = false,
                ScheduledTime = alarmTime,
                Sent = false
            };

            alarmSchedule.AlarmNotifications.Add(notification);
            await dbContext.SaveChangesAsync();

            alarmSchedule.LatestAlarmNotificationId = notification.Id;
            await dbContext.SaveChangesAsync();

            var deviceId = NSUserDefaults.StandardUserDefaults.StringForKey("DeviceId");

            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = Guid.NewGuid().ToString();
                NSUserDefaults.StandardUserDefaults.SetString(deviceId, "DeviceId");
            }

#if DEBUG
            var url = "http://192.168.1.64:5010/api/v1/ScheduleAlarm";
#else
                    var url = "https://production-push.jthomas.info/api/v1/ScheduleAlarm";
#endif

            var request = new ScheduleAlarmRequest()
            {
                DeviceId = deviceId,
                DeviceToken = NSUserDefaults.StandardUserDefaults.StringForKey("PushDeviceToken"),
                AlarmTime = alarmTime.UtcDateTime,
                NotificationId = notification.Id
            };

            //Send to server
            var result = await RetryHelper.Retry(async () =>
            {
                using var client = new HttpClient();
                var payload = JsonConvert.SerializeObject(request);
                HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                return await client.PostAsync(url, content);

            }, 3, true);


            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {

            }
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
