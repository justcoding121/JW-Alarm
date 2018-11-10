using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Notifications;

namespace JW.Alarm.Services.Uwp.Tasks
{
    public class AlarmTask
    {
        private IAlarmService alarmService;
        private INotificationService notificationService;
        private IScheduleDbContext scheduleDbContext;
        private IPlaylistService playlistService;


        public AlarmTask(IAlarmService alarmService,
            INotificationService notificationService,
            IScheduleDbContext scheduleDbContext,
            IPlaylistService playlistService)
        {
            this.alarmService = alarmService;
            this.notificationService = notificationService;
            this.scheduleDbContext = scheduleDbContext;
            this.playlistService = playlistService;
        }

        public async void Handle(IBackgroundTaskInstance backgroundTask)
        {
            var deferral = backgroundTask.GetDeferral();

            var details = backgroundTask.TriggerDetails as ToastNotificationHistoryChangedTriggerDetail;

            if (details.ChangeType == ToastHistoryChangedType.Added)
            {
                var history = ToastNotificationManager.History.GetHistory();

                var playDetails = new Dictionary<ToastNotification, NotificationDetail>();
                foreach (var toast in history)
                {
                    var detail = await notificationService.ParseNotificationDetail(toast.Tag);
                    playDetails.Add(toast, detail);
                }

                var latest = playDetails
                    .Select(x => x.Value)
                    .OrderByDescending(x => x.NotificationTime)
                    .FirstOrDefault();

                if (latest != null)
                {
                    await playlistService.SetFinishedTrack(latest);

                    var schedule = await scheduleDbContext.Read(latest.ScheduleId);
                    await alarmService.Create(schedule, latest);

                    var notifier = ToastNotificationManager.CreateToastNotifier();
                    var notifications = notifier.GetScheduledToastNotifications();
                    var outDated = playDetails.Where(x => x.Value != latest);

                    foreach (var item in outDated)
                    {     
                        var toRemove = notifications.FirstOrDefault(x => x.Tag == item.Key.Tag);
                        if (toRemove != null)
                        {
                            notifier.RemoveFromSchedule(toRemove);
                        }

                    }
                }
            }

            deferral.Complete();
        }
    }
}
