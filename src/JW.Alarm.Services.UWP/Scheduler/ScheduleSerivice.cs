using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace JW.Alarm.Services.Uwp
{
    public class UwpScheduleService : ScheduleService
    {
        public UwpScheduleService(IStorageService storageService)
            : base(storageService)
        {
        }

        public override async Task Create(AlarmSchedule schedule)
        {
            createNotification(schedule);
            await base.Create(schedule);
        }

        public override async Task Delete(int scheduleId)
        {
            removeNotification(scheduleId.ToString());
            await base.Delete(scheduleId);
        }

        public override async Task Update(AlarmSchedule schedule)
        {
            removeNotification(schedule.Id.ToString());

            if(schedule.IsEnabled)
            {
                createNotification(schedule);
            }

            await base.Update(schedule);
        }

        private void createNotification(AlarmSchedule schedule)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            var scheduledNotification = generateAlarmNotification(schedule, schedule.NextFireDate());
            notifier.AddToSchedule(scheduledNotification);
        }

        private bool removeNotification(string tagName)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            var notifications = notifier.GetScheduledToastNotifications();

            var notification = notifications.FirstOrDefault(x => x.Tag == tagName);

            if (notification != null)
            {
                notifier.RemoveFromSchedule(notification);
                return true;
            }

            return false;
        }

        private static ScheduledToastNotification generateAlarmNotification(AlarmSchedule alarm, DateTimeOffset alarmTime)
        {
            var content = new ToastContent()
            {
                Scenario = ToastScenario.Reminder,
                DisplayTimestamp = alarmTime,
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Alarm",
                                HintMaxLines = 1
                            },
                            new AdaptiveText()
                            {
                                Text = alarm.Name
                            }
                        }
                    }
                },

                Actions = new ToastActionsSnoozeAndDismiss()
            };

            // We can easily enable Universal Dismiss by generating a RemoteId for the alarm that will be
            // the same on both devices. We'll just use the alarm delivery time. If an alarm on one device
            // has the same delivery time as an alarm on another device, it'll be dismissed when one of the
            // alarms is dismissed.
            string remoteId = (alarmTime.Ticks / 10000000 / 60).ToString(); // Minutes

            return new ScheduledToastNotification(content.GetXml(), alarmTime)
            {
                Group = "alarms",
                Tag = alarm.Id.ToString(),
                RemoteId = remoteId
            };
        }

    }
}
