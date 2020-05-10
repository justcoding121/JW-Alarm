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
using UserNotifications;

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
            _ = iosAlarmHandler.Handle(scheduleId, true);
        }

        public async Task ScheduleNotification(long scheduleId, DateTimeOffset time,
            string title, string body)
        {
            await Task.Delay(0).
                ContinueWith((x) =>
                {
                    var @params = new Dictionary<string, string>
                        {
                            {"ScheduleId", scheduleId.ToString()},
                        };

                    if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                    {
                        var content = new UNMutableNotificationContent();
                        content.Title = title;
                        content.Body = body;
                        content.Badge = 1;       
                        content.Sound = UNNotificationSound.GetSound("cool-alarm-tone-notification-sound.caf");
                        content.UserInfo = @params.ToNSDictionary();

                        var trigger = UNCalendarNotificationTrigger.CreateTrigger(time.LocalDateTime.ToNSDateComponents(), true);

                        var requestID = scheduleId.ToString();
                        var request = UNNotificationRequest.FromIdentifier(requestID, content, trigger);

                        UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) =>
                        {
                            if (err != null)
                            {
                                logger.Error($"An error happened when scheduling ios notification. code: {err.Code}");
                            }
                        });
                    }
                    else
                    {
                        var notification = new UILocalNotification();
                        var diff = time.UtcDateTime - DateTime.UtcNow;
                        notification.FireDate = NSDate.FromTimeIntervalSinceNow(diff.TotalSeconds);
                        notification.AlertAction = title;
                        notification.AlertBody = body;
                        notification.ApplicationIconBadgeNumber = 1;
                        notification.SoundName = "cool-alarm-tone-notification-sound.caf";
                        notification.RepeatInterval = NSCalendarUnit.Day;
                        notification.TimeZone = NSTimeZone.LocalTimeZone;

                        notification.UserInfo = @params.ToNSDictionary();

                        UIApplication.SharedApplication.ScheduleLocalNotification(notification);
                    }

                }, taskScheduler);

        }

        public async Task Remove(long scheduleId)
        {
            await Task.Delay(0).
               ContinueWith((x) =>
               {
                   if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                   {
                       var pending = UNUserNotificationCenter.Current.GetPendingNotificationRequestsAsync().Result;

                       if (pending != null)
                       {
                           foreach (var notification in pending)
                           {
                               if (notification.Identifier == scheduleId.ToString())
                               {
                                   UNUserNotificationCenter.Current.RemovePendingNotificationRequests(new[] { scheduleId.ToString() });
                               }
                           }
                       }

                   }
                   else
                   {
                       var pending = UIApplication.SharedApplication.ScheduledLocalNotifications;

                       if (pending != null)
                       {
                           foreach (var notification in pending)
                           {
                               var userInfo = notification.UserInfo.ToDictionary();

                               if (userInfo.ContainsKey("ScheduleId")
                                   && userInfo["ScheduleId"] == scheduleId.ToString())
                               {
                                   UIApplication.SharedApplication.CancelLocalNotification(notification);
                               }
                           }
                       }
                   }

               }, taskScheduler);
        }

        public async Task<bool> IsScheduled(long scheduleId)
        {
            return await Task.Delay(0).
                 ContinueWith((x) =>
                 {
                     if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                     {
                         var pending = UNUserNotificationCenter.Current.GetPendingNotificationRequestsAsync().Result;

                         if (pending != null)
                         {
                             foreach (var notification in pending)
                             {
                                 if (notification.Identifier == scheduleId.ToString())
                                 {
                                     return true;
                                 }
                             }
                         }

                         return false;
                     }
                     else
                     {
                         var pending = UIApplication.SharedApplication.ScheduledLocalNotifications;

                         if (pending != null)
                         {
                             foreach (var notification in pending)
                             {
                                 var userInfo = notification.UserInfo.ToDictionary();

                                 if (userInfo.ContainsKey("ScheduleId")
                                   && userInfo["ScheduleId"] == scheduleId.ToString())
                                 {
                                     return true;
                                 }
                             }
                         }

                         return false;
                     }

                 }, taskScheduler);
        }

        public async Task<bool> CanSchedule()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                return await Task.Delay(0).
                   ContinueWith((x) =>
                   {
                       var taskCompletionSource = new TaskCompletionSource<bool>();

                       UNUserNotificationCenter.Current.GetNotificationSettings((settings) =>
                        {
                            var result = settings.AlertSetting == UNNotificationSetting.Enabled;
                            taskCompletionSource.SetResult(result);
                        });

                       return taskCompletionSource.Task.Result;

                   }, taskScheduler);
            }
            else
            {
                var types = UIApplication.SharedApplication.CurrentUserNotificationSettings?.Types;

                if (types == UIUserNotificationType.Alert)
                {
                    return true;
                }

                return false;
            }

        }

        public void Dispose()
        {

        }
    }

}
