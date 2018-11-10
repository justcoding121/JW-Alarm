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
        IPlayDetailDbContext playDetailDbContext;

        public UwpNotificationService(IMediaCacheService mediaCacheService,
            IPlayDetailDbContext playDetailDbContext)
        {
            this.mediaCacheService = mediaCacheService;
            this.playDetailDbContext = playDetailDbContext;
        }

        public async Task Add(int scheduleId, PlayDetail detail, DateTimeOffset notificationTime,
                                    string title, string body, string audioUrl)
        {
            await playDetailDbContext.Create(detail);

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
                Group = scheduleId.ToString(),
                RemoteId = remoteId
            };

            notifier.AddToSchedule(notification);
        }

        public async Task Remove(int scheduleId)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();

            var notifications = notifier.GetScheduledToastNotifications()
                                    .Where(x => x.Group == scheduleId.ToString())
                                    .ToList();


            var details = (await playDetailDbContext.PlayDetails)
                .Where(x => x.Value.ScheduleId == scheduleId)
                .Select(x => x.Value)
                .ToList();

            foreach (var detail in details)
            {
                await playDetailDbContext.Delete(detail.Id);
            }

            foreach (var notification in notifications)
            {
                notifier.RemoveFromSchedule(notification);
            }
        }

        public bool IsScheduled(int scheduleId)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var notifications = notifier.GetScheduledToastNotifications();
            return notifications.Any(x => x.Group == scheduleId.ToString());
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

        public async Task<PlayDetail> ParseNotificationDetail(string key)
        {
            return await playDetailDbContext.Read(int.Parse(key));
        }
    }

}
