using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using System;
using System.Linq;
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

        public void Add(int scheduleId, string detail, DateTimeOffset notificationTime, 
                                    string title, string body, string audioUrl)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            var content = new ToastContent()
            {
                Audio = new ToastAudio() { Src = new Uri(mediaCacheService.GetCacheUrl(audioUrl)) },
                Scenario = ToastScenario.Reminder,
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title,
                                HintMaxLines = 1
                            },
                            new AdaptiveText()
                            {
                                Text = body
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
            string remoteId = (notificationTime.Ticks / 10000000 / 60).ToString(); // Minutes

            var notification = new ScheduledToastNotification(content.GetXml(), notificationTime)
            {
                Tag = detail,
                Group = scheduleId.ToString(),
                RemoteId = remoteId
            };

            notifier.AddToSchedule(notification);
        }

        public bool Remove(int scheduleId)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            var notifications = notifier.GetScheduledToastNotifications()
                                    .Where(x => x.Group == scheduleId.ToString())
                                    .ToList();

            foreach (var notification in notifications)
            {
                notifier.RemoveFromSchedule(notification);
            }

            return true;
        }

        public bool IsScheduled(int scheduleId)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var notifications = notifier.GetScheduledToastNotifications();
            return notifications.Any(x => x.Group == scheduleId.ToString());
        }

        public void Clear()
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            var notifications = notifier.GetScheduledToastNotifications()
                                        .ToList();

            foreach (var notification in notifications)
            {
                notifier.RemoveFromSchedule(notification);
            }
        }

        public string GetBibleNotificationDetail(int scheduleId, BibleReadingSchedule bibleReadingSchedule)
        {
            return JsonConvert.SerializeObject(new PlayDetail()
            {
                ScheduleId = scheduleId,
                BookNumber = bibleReadingSchedule.BookNumber,
                Chapter = bibleReadingSchedule.ChapterNumber
            });
        }

        public string GetMusicNotificationDetail(int scheduleId, AlarmMusic alarmMusicSchedule)
        {
            return JsonConvert.SerializeObject(new PlayDetail()
            {
                ScheduleId = scheduleId,
                TrackNumber = alarmMusicSchedule.TrackNumber
            });
        }


        public PlayDetail ParseNotificationDetail(string detail)
        {
            return JsonConvert.DeserializeObject<PlayDetail>(detail);
        }
    }

}
