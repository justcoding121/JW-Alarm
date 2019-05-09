using Android;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Bible.Alarm.Services;
using Bible.Alarm.ViewModels;
using JW.Alarm.Common.Mvvm;
using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using MediaManager;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Droid.Tasks
{
    [Service]
    public class AlarmSetupTask : Service
    {
        public static bool IsRunning = false;

        public AlarmSetupTask() : base()
        {
            if (!AppCenter.Configured)
            {
                AppCenter.Start("0cd5c3e8-dcfa-48dd-9d4b-0433a8572fb9",
                  typeof(Analytics));
            }

            if (IocSetup.Container == null)
            {
                Analytics.TrackEvent($"Container null for AlarmService at {DateTime.Now}.");
                Bible.Alarm.Droid.IocSetup.Initialize();
                IocSetup.Container.Resolve<IMediaManager>().SetContext(this);
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            IsRunning = true;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            var extra = intent.GetStringExtra("Action");

            switch (extra)
            {
                case "Add":
                    {
                        var time = DateTimeOffset.Parse(intent.GetStringExtra("Time"));
                        var title = intent.GetStringExtra("Title");
                        var body = intent.GetStringExtra("Body");

                        addNotification(long.Parse(intent.GetStringExtra("ScheduleId")), time, title, body);
                        break;
                    }

                default:
                    var schedulerTask = Bible.Alarm.Droid.IocSetup.Container.Resolve<SchedulerTask>();
                    schedulerTask.Handle().Wait();
                    break;
            }


            StopSelf();

            return base.OnStartCommand(intent, flags, startId);
        }


        public override void OnDestroy()
        {
            IsRunning = false;
        }

        public void addNotification(long scheduleId, DateTimeOffset time,
            string title, string body)
        {
            var context = this;
            var alarmIntent = new Intent();
            alarmIntent.SetAction(buildActionName());
            alarmIntent.PutExtra("ScheduleId", scheduleId.ToString());

            var pIntent = PendingIntent.GetService(
                    context,
                    (int)scheduleId,
                    alarmIntent,
                    PendingIntentFlags.UpdateCurrent);

            var alarmService = (AlarmManager)context.GetSystemService(Context.AlarmService);

            // Figure out the alaram in milliseconds.
            var milliSecondsRemaining = Java.Lang.JavaSystem.CurrentTimeMillis() + (long)time.Subtract(DateTimeOffset.Now).TotalSeconds * 1000;

            if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
            {
                alarmService.Set(AlarmType.RtcWakeup, milliSecondsRemaining, pIntent);
            }
            else if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                alarmService.SetExact(AlarmType.RtcWakeup, milliSecondsRemaining, pIntent);
            }
            else
            {
                alarmService.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, milliSecondsRemaining, pIntent);
            }
        }

        public bool IsScheduled(long scheduleId)
        {
            var pIntent = findIntent(scheduleId);
            return pIntent != null;
        }

        private PendingIntent findIntent(long scheduleId)
        {
            var context = this;

            var alarmIntent = new Intent();
            alarmIntent.PutExtra("ScheduleId", scheduleId.ToString());
            alarmIntent.SetAction(buildActionName());

            var pIntent = PendingIntent.GetService(
                  context,
                  (int)scheduleId,
                  alarmIntent,
                  PendingIntentFlags.NoCreate);

            return pIntent;
        }

        internal static string buildActionName()
        {
            return "com.bible.alarm.RING";
        }
    }
}
