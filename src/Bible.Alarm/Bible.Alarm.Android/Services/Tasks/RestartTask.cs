using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Bible.Alarm.Services.Infrastructure;
using JW.Alarm.Services.Droid.Helpers;
using JW.Alarm.Services.Droid.Tasks;
using Microsoft.Extensions.Logging;
using NLog;

namespace Bible.Alarm.Droid.Services.Tasks
{

    [BroadcastReceiver(Enabled = true, DirectBootAware = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted,
        "android.intent.action.QUICKBOOT_POWERON", "com.htc.intent.action.QUICKBOOT_POWERON", "com.Bible.Alarm.Restart"})]
    public class RestartTask : BroadcastReceiver
    {
        private static Logger logger => LogManager.GetCurrentClassLogger();

        public RestartTask()
            : base()
        {
            LogSetup.Initialize();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (IocSetup.Container == null)
            {
                IocSetup.Initialize(context, true);
            }

            logger.Info($"Restart task fired.");

            try
            {
                var schedulerTask = IocSetup.Container.Resolve<SchedulerTask>();
                schedulerTask.Handle().Wait();
                BootstrapHelper.VerifyBackgroundTasks();

                context.StopService(intent);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to process restart task.");
            }
        }
    }
}