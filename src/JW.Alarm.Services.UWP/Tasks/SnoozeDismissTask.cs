using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace JW.Alarm.Services.Uwp.Tasks
{
    public class SnoozeDismissTask
    {
        private IAlarmService alarmService;
        private INotificationService notificationService;
        private IScheduleDbContext scheduleDbContext;
        private IPlaylistService playlistService;

        public SnoozeDismissTask(IAlarmService alarmService,
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

            var details = backgroundTask.TriggerDetails as ToastNotificationActionTriggerDetail;

            await notificationService.Remove(long.Parse(details.Argument));

            ToastNotificationManager.History.Clear();

            deferral.Complete();
        }
    }
}
