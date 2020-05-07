using Bible.Alarm.iOS.Extensions;
using Bible.Alarm.iOS.Services.Handlers;
using Bible.Alarm.Services.Contracts;
using Foundation;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace Bible.Alarm.Services.iOS
{
    public class iOSNotificationService : INotificationService
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        private readonly IContainer container;
        private readonly TaskScheduler taskScheduler;

        public iOSNotificationService(IContainer container)
        {
            this.container = container;
            taskScheduler = container.Resolve<TaskScheduler>();
        }

        public void ShowNotification(long scheduleId)
        {
            var iosAlarmHandler = container.Resolve<iOSAlarmHandler>();
            _ = iosAlarmHandler.Handle(scheduleId);
        }

        public async Task ScheduleNotification(long scheduleId, DateTimeOffset time,
            string title, string body)
        {
            await Task.Delay(0).
                ContinueWith((x) =>
                {
                    var notification = new UILocalNotification();
                    var diff = time.UtcDateTime - DateTime.UtcNow;
                    notification.FireDate = NSDate.FromTimeIntervalSinceNow(diff.TotalSeconds);
                    notification.AlertAction = title;
                    notification.AlertBody = body;
                    notification.ApplicationIconBadgeNumber = 1;
                    notification.SoundName = UILocalNotification.DefaultSoundName;

                    var @params = new Dictionary<string, string>
                    {
                        {"ScheduleId", scheduleId.ToString()},
                    };

                    notification.UserInfo = @params.ToNSDictionary();

                    UIApplication.SharedApplication.ScheduleLocalNotification(notification);

                }, taskScheduler);

        }

        public async Task Remove(long scheduleId)
        {
            await Task.Delay(0).
               ContinueWith((x) =>
               {
                   foreach (var notification in UIApplication.SharedApplication.ScheduledLocalNotifications)
                   {
                       var userInfo = notification.UserInfo.ToDictionary();

                       if(userInfo["ScheduleId"] == scheduleId.ToString())
                       {
                           UIApplication.SharedApplication.CancelLocalNotification(notification);
                       }

                   }

               }, taskScheduler);
        }

        public async Task<bool> IsScheduled(long scheduleId)
        {
           return await Task.Delay(0).
                ContinueWith((x) =>
                {
                    foreach (var notification in UIApplication.SharedApplication.ScheduledLocalNotifications)
                    {
                        var userInfo = notification.UserInfo.ToDictionary();

                        if (userInfo["ScheduleId"] == scheduleId.ToString())
                        {
                            return true;
                        }

                    }

                    return false;

                }, taskScheduler);
        }

        public void Dispose()
        {

        }
    }

}
