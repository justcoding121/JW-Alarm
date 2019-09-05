using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.ViewModels;
using JW.Alarm.Common.Mvvm;
using JW.Alarm.Services.Contracts;
using JW.Alarm.Services.Droid.Tasks;
using MediaManager;
using MediaManager.Playback;
using MediaManager.Player;
using Microsoft.Extensions.Logging;
using NLog;

namespace Bible.Alarm.Droid.Services.Tasks
{
    [BroadcastReceiver(Enabled = true)]
    public class SchedulerReceiver : BroadcastReceiver
    {
        private static Logger logger => LogManager.GetCurrentClassLogger();

        public SchedulerReceiver() : base()
        {
            LogSetup.Initialize();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                if (IocSetup.Container == null)
                {
                    IocSetup.Initialize(context, true);
                }

                var schedulerTask = IocSetup.Container.Resolve<SchedulerTask>();
                schedulerTask.Handle().Wait();
            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened when creating the repeating scheduler task.");
            }

        }

    }
}