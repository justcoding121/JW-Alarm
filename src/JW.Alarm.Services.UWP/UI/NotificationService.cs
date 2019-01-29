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
        INotificationRepository notificationTableContext;

        public UwpNotificationService(IMediaCacheService mediaCacheService,
            INotificationRepository playDetailDbContext)
        {
            this.mediaCacheService = mediaCacheService;
            this.notificationTableContext = playDetailDbContext;
        }

        public async Task Add(string scheduleId, NotificationDetail notificationDetail, string title, string body, string audioUrl)
        {

            await notificationTableContext.Add(notificationDetail);

            var notifier = ToastNotificationManager.CreateToastNotifier();

            var url = new Uri(Path.Combine(ApplicationData.Current.TemporaryFolder.Path,
                    mediaCacheService.GetCacheKey(audioUrl)));

            var content = new ToastContent()
            {
                Audio = new ToastAudio() { Src = new Uri("ms-appx:///Assets/Media/1.5-second-silence.mp3") },
                Scenario = ToastScenario.Alarm,
                ActivationType = ToastActivationType.Background,
                Launch = scheduleId,
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
                        new ToastButton("Snooze", scheduleId)
                        {
                            ActivationType = ToastActivationType.Background
                        },

                        new ToastButton("Dismiss", scheduleId)
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
            string remoteId = (notificationDetail.NotificationTime.Ticks / 10000000 / 60).ToString(); 

            var notification = new ScheduledToastNotification(content.GetXml(), notificationDetail.NotificationTime)
            {
                Tag = notificationDetail.Id.ToString(),
                Group = scheduleId,
                RemoteId = remoteId,
            };

            notifier.AddToSchedule(notification);
        }


        public void AddSilent(string groupId, DateTimeOffset notificationTime)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            var content = new ToastContent()
            {
                Audio = new ToastAudio() { Src = new Uri("ms-appx:///Assets/Media/1.5-second-silence.mp3") },
                Scenario = ToastScenario.Default,
                ActivationType = ToastActivationType.Background,
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Playing next chapter..",
                                HintMaxLines = 1
                            }
                        }
                    }
                },

                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButton("Snooze", "snooze")
                        {
                            ActivationType = ToastActivationType.Background
                        },

                        new ToastButton("Dismiss", "dismiss")
                        {
                            ActivationType = ToastActivationType.Background
                        }
                    }
                }
            };

            string remoteId = (notificationTime.Ticks / 10000000 / 60).ToString(); // Minutes

            var notification = new ScheduledToastNotification(content.GetXml(), notificationTime)
            {
                Group = groupId,
                RemoteId = remoteId
            };

            notifier.AddToSchedule(notification);
        }
        public async Task Remove(long scheduleId)
        {
            var notifications = (await notificationTableContext.Notifications)
                                .Where(x => x.ScheduleId == scheduleId)
                                .Select(x => x).ToList();

            var notifier = ToastNotificationManager.CreateToastNotifier();

            foreach (var notification in notifier.GetScheduledToastNotifications())
            {
                if (notifications.Any(x => x.Id.ToString() == notification.Tag))
                {
                    notifier.RemoveFromSchedule(notification);
                }
            }

            foreach (var notification in notifications)
            {
                await notificationTableContext.Remove(notification.Id);
            }
        }

        public bool IsScheduled(long scheduleId)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var notifications = notifier.GetScheduledToastNotifications();
            return notifications.Any(x => x.Group == scheduleId.ToString());
        }

        public string GetBibleNotificationDetail(long scheduleId, BibleReadingSchedule bibleReadingSchedule)
        {
            return JsonConvert.SerializeObject(new NotificationDetail()
            {
                ScheduleId = scheduleId,
                BookNumber = bibleReadingSchedule.BookNumber,
                ChapterNumber = bibleReadingSchedule.ChapterNumber
            });
        }

        public string GetMusicNotificationDetail(long scheduleId, AlarmMusic alarmMusicSchedule)
        {
            return JsonConvert.SerializeObject(new NotificationDetail()
            {
                ScheduleId = scheduleId,
                TrackNumber = alarmMusicSchedule.TrackNumber
            });
        }

        public async Task<NotificationDetail> ParseNotificationDetail(string key)
        {
            return await notificationTableContext.Read(long.Parse(key));
        }

    }

}
