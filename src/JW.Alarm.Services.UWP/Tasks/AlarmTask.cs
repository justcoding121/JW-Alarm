﻿using JW.Alarm.Services.Contracts;
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
        private IMediaPlayService mediaPlayService;

        public AlarmTask(IMediaPlayService mediaPlayService)
        {
            this.mediaPlayService = mediaPlayService;
        }

        public async void Handle(IBackgroundTaskInstance backgroundTask)
        {
            var deferral = backgroundTask.GetDeferral();

            var details = backgroundTask.TriggerDetails as ToastNotificationHistoryChangedTriggerDetail;

            if (details.ChangeType == ToastHistoryChangedType.Added)
            {
                var history = ToastNotificationManager.History.GetHistory();

                foreach (var toast in history)
                {
                    if(int.TryParse(toast.Tag, out var id))
                    {
                        try
                        {
                            await mediaPlayService.Play(id);
                        }
                        catch { }
                      
                        break;
                    }
                }
           }

            deferral.Complete();
        }
    }
}
