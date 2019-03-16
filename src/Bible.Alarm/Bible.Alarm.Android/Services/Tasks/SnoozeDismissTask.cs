using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace JW.Alarm.Services.Droid.Tasks
{
    public class SnoozeDismissTask
    {
        private IPlaybackService playbackService;

        public SnoozeDismissTask(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;
        }

        public async void Handle(IBackgroundTaskInstance backgroundTask)
        {
            var deferral = backgroundTask.GetDeferral();

            var details = backgroundTask.TriggerDetails as ToastNotificationActionTriggerDetail;

            switch(details.Argument)
            {
                case "Snooze":
                    await playbackService.Snooze();
                    break;
                case "Dismiss":
                    playbackService.Dismiss();
                    break;
            }

            ToastNotificationManager.History.Clear();

            deferral.Complete();
        }
    }
}
