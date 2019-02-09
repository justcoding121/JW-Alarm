using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Notifications;

namespace JW.Alarm.Services.UWP
{
    public class UwpNotificationService : INotificationService
    {
        IMediaCacheService mediaCacheService;

        public UwpNotificationService(IMediaCacheService mediaCacheService)
        {
            this.mediaCacheService = mediaCacheService;
        }

        public void Add(long scheduleId, DateTimeOffset time,
            string title, string body, Uri audio)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            var content = new ToastContent()
            {
                Audio = new ToastAudio() { Src = audio ?? new Uri("ms-appx:///Assets/Media/1.5-second-silence.mp3") },
                Scenario = ToastScenario.Alarm,
                ActivationType = ToastActivationType.Background,
                Launch = scheduleId.ToString(),
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title,
                                HintMaxLines = 1,
                            },
                            new AdaptiveText()
                            {
                                Text = body
                            }
                        }
                    }
                },

                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("Snooze", "Snooze")
                        {
                            ActivationType = ToastActivationType.Background
                        },

                        new ToastButton("Dismiss", "Dismiss")
                        {
                            ActivationType = ToastActivationType.Background
                        }
                    }
                }
            };

            // We can easily enable Universal Dismiss by generating a RemoteId for the alarm that will be
            // the same on both devices. We'll just use the alarm delivery time. If an alarm on one device
            // has the same delivery time as an alarm on another device, it'll be dismissed when one of the
            // alarms is dismissed.
            string remoteId = (time.Ticks / 10000000 / 60).ToString();

            var notification = new ScheduledToastNotification(content.GetXml(), time)
            {
                Group = scheduleId.ToString(),
                RemoteId = remoteId
            };

            notifier.AddToSchedule(notification);
        }


        public void Remove(long scheduleId)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            foreach (var notification in notifier.GetScheduledToastNotifications())
            {
                if (scheduleId.ToString() == notification.Group)
                {
                    notifier.RemoveFromSchedule(notification);
                }
            }
        }

        public bool IsScheduled(long scheduleId)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var notifications = notifier.GetScheduledToastNotifications();
            return notifications.Any(x => x.Group == scheduleId.ToString());
        }
    }

}
