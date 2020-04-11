using Bible.Alarm.Common.Helpers;
using Bible.Alarm.iOS.Helpers;
using Bible.Alarm.iOS.Models;
using Bible.Alarm.iOS.Services.Handlers;
using Bible.Alarm.Models.Schedule;
using Bible.Alarm.Services.Contracts;
using Foundation;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
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
        private Logger logger => LogManager.GetCurrentClassLogger();

        private readonly IContainer container;
        private readonly ScheduleDbContext dbContext;

        public iOSNotificationService(IContainer container, ScheduleDbContext dbContext)
        {
            this.container = container;
            this.dbContext = dbContext;
        }

        public void ShowNotification(long scheduleId)
        {
            var iosAlarmHandler = container.Resolve<iOSAlarmHandler>();
            var task = iosAlarmHandler.Handle(scheduleId);
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

            var deviceToken = NSUserDefaults.StandardUserDefaults.StringForKey("PushDeviceToken");
            //TODO: schedule local notifications here


            try
            {
                //Send to server
                var result = await PnsService.ScheduleAlarm(deviceId,
                                     deviceToken,
                                     alarmTime.UtcDateTime,
                                     notification.Id);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    notification.Sent = true;
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened when scheduling push notification.");
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

                //remove local notifications here

                //Send to server
                try
                {
                    var deviceId = NSUserDefaults.StandardUserDefaults.StringForKey("DeviceId");
                    var deviceToken = NSUserDefaults.StandardUserDefaults.StringForKey("PushDeviceToken");
                    
                    //Send to server
                    var result = await PnsService.RemoveAlarm(deviceId, deviceToken, notification.Id);

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        notification.Cancelled = true;
                        await dbContext.SaveChangesAsync();
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "An error happened when scheduling push notification.");
                }   
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
