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
using JW.Alarm.Services.Droid.Tasks;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace Bible.Alarm.Droid.Services.Tasks
{
    [BroadcastReceiver(Enabled = true, DirectBootAware = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted,
        "android.intent.action.QUICKBOOT_POWERON", "com.htc.intent.action.QUICKBOOT_POWERON"})]
    public class RestartTask : BroadcastReceiver
    {
        public RestartTask()
            : base()
        {
            if (!AppCenter.Configured)
            {
                AppCenter.Start("0cd5c3e8-dcfa-48dd-9d4b-0433a8572fb9", typeof(Analytics), typeof(Crashes));
            }

            if (IocSetup.Container == null)
            {
                IocSetup.Initialize();
            }
        }

        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                Analytics.TrackEvent($"Restart task called at {DateTime.Now}");
                var schedulerTask = IocSetup.Container.Resolve<SchedulerTask>();
                schedulerTask.Handle().Wait();

                context.StopService(intent);
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
            }
        }
    }
}