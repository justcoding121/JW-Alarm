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
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Droid.Tasks;
using Microsoft.Extensions.Logging;
using NLog;

namespace Bible.Alarm.Droid.Services.Tasks
{

    [BroadcastReceiver(Enabled = true, DirectBootAware = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted,
        "android.intent.action.QUICKBOOT_POWERON", "com.htc.intent.action.QUICKBOOT_POWERON", "com.Bible.Alarm.Restart"})]
    public class RestartReceiver : BroadcastReceiver
    {
        private static Logger logger => LogManager.GetCurrentClassLogger();

        public RestartReceiver()
            : base()
        {
            LogSetup.Initialize("Android");
        }

        public override void OnReceive(Context context, Intent intent)
        {
            try
            {

                IocSetup.Initialize(Application.Context, true);

                BootstrapHelper.VerifyBackgroundTasks(IocSetup.Context);

                var schedulerTask = IocSetup.Container.Resolve<SchedulerTask>();
                schedulerTask.Handle().Wait();

                context.StopService(intent);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to process restart task.");
            }
        }
    }
}