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

namespace Bible.Alarm.Droid.Services.Tasks
{
    [BroadcastReceiver(Enabled = true, Exported = true, DirectBootAware = true)]
    [IntentFilter(new[] { "com.bible.alarm.RESTART", Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted,
        "android.intent.action.QUICKBOOT_POWERON", "com.htc.intent.action.QUICKBOOT_POWERON"})]
    public class RestartTask : BroadcastReceiver
    {
        public RestartTask()
            : base()
        {
            if (!AppCenter.Configured)
            {
                AppCenter.Start("0cd5c3e8-dcfa-48dd-9d4b-0433a8572fb9",
                  typeof(Analytics));
            }

            if (IocSetup.Container == null)
            {
                Analytics.TrackEvent($"Container null for RestartTask at {DateTime.Now}.");
                Bible.Alarm.Droid.IocSetup.Initialize();

            }
        }

        public override void OnReceive(Context context, Intent intent)
        {
            Analytics.TrackEvent($"Service start requested at {DateTime.Now}.");

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                context.StartForegroundService(new Intent(context, typeof(AlarmSetupTask)));
            }
            else
            {
                context.StartService(new Intent(context, typeof(AlarmSetupTask)));
            }

            context.StopService(intent);
        }
    }
}