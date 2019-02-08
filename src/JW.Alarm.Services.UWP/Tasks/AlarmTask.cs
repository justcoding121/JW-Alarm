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
        private IScheduleRepository scheduleDbContext;
        private IPlaybackService playbackService;

        public AlarmTask(IAlarmService alarmService,
            IScheduleRepository scheduleDbContext,
            IPlaybackService playbackService)
        {
            this.alarmService = alarmService;
            this.scheduleDbContext = scheduleDbContext;
            this.playbackService = playbackService;
        }

        public async void Handle(IBackgroundTaskInstance backgroundTask)
        {
            var deferral = backgroundTask.GetDeferral();

            var details = backgroundTask.TriggerDetails as ToastNotificationHistoryChangedTriggerDetail;

            if (details.ChangeType == ToastHistoryChangedType.Added)
            {
                var toast = ToastNotificationManager.History.GetHistory()
                    .Select(x => new
                    {
                        x.Group
                    })
                .FirstOrDefault();

                await playbackService.Play(long.Parse(toast.Group));

            }

            deferral.Complete();

        }
    }
}
