using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace JW.Alarm.Services.UWP
{
    public class UwpNotificationService : INotificationService
    {
        IMediaCacheService mediaCacheService;
        INotificationDetailDbContext playDetailDbContext;
        IBibleReadingDbContext bibleReadingDbContext;

        public UwpNotificationService(IMediaCacheService mediaCacheService,
            INotificationDetailDbContext playDetailDbContext,
            IBibleReadingDbContext bibleReadingDbContext)
        {
            this.mediaCacheService = mediaCacheService;
            this.playDetailDbContext = playDetailDbContext;
            this.bibleReadingDbContext = bibleReadingDbContext;
        }

        public async Task Add(string groupId, NotificationDetail detail, DateTimeOffset notificationTime,
                                    string title, string body, string audioUrl)
        {

            await playDetailDbContext.Add(detail);

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
                Tag = detail.Id.ToString(),
                Group = groupId,
                RemoteId = remoteId
            };

            notifier.AddToSchedule(notification);
        }

        public async Task Remove(long scheduleId)
        {
            var notifications = (await playDetailDbContext.PlayDetails)
                                .Where(x => x.Value.ScheduleId == scheduleId)
                                .Select(x => x.Value).ToList();


            foreach (var notification in notifications)
            {
                await playDetailDbContext.Remove(notification.Id);
            }

            var notifier = ToastNotificationManager.CreateToastNotifier();

            ToastNotificationManager.History.Clear();
            foreach (var notification in notifier.GetScheduledToastNotifications())
            {
                //if(notifications.Any(x=> x.Id.ToString() == notification.Tag))
                //{
                notifier.RemoveFromSchedule(notification);
                //}     
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
            return await playDetailDbContext.Read(long.Parse(key));
        }

        public void AddSilent(string groupId, DateTimeOffset notificationTime)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            var content = new ToastContent()
            {
                Audio = new ToastAudio() { Src = new Uri("ms-appx:///Assets/Media/1.5-second-silence.mp3") },
                Scenario = ToastScenario.Reminder,
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

                Actions = new ToastActionsSnoozeAndDismiss()
            };

            string remoteId = (notificationTime.Ticks / 10000000 / 60).ToString(); // Minutes

            var notification = new ScheduledToastNotification(content.GetXml(), notificationTime)
            {
                Group = groupId,
                RemoteId = remoteId
            };

            notifier.AddToSchedule(notification);
        }
    }

}
