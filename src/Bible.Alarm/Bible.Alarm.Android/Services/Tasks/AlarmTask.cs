using Android;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Widget;
using Bible.Alarm.Services;
using Bible.Alarm.ViewModels;
using JW.Alarm.Common.Mvvm;
using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
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
    public class AlarmRingerService : Service
    {
        public static bool IsRunning => notificationReceiver != null;
        private static BroadcastReceiver notificationReceiver;

        public AlarmRingerService() : base()
        {
            if (IocSetup.Container == null)
            {
                Bible.Alarm.Droid.IocSetup.Initialize();
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            RegisterBroadcastReceiver();

            var schedulerTask = Bible.Alarm.Droid.IocSetup.Container.Resolve<SchedulerTask>();
            schedulerTask.Handle();
        }

        public override void OnDestroy()
        {
            Application.Context.UnregisterReceiver(notificationReceiver);
            notificationReceiver = null;
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            base.OnTaskRemoved(rootIntent);

            Intent broadcastIntent = new Intent();
            broadcastIntent.SetAction("com.bible.alarm.RESTART");
            broadcastIntent.SetClass(this, typeof(RestartTask));
            this.SendBroadcast(broadcastIntent);
        }

        private void RegisterBroadcastReceiver()
        {
            notificationReceiver = new AlarmTask();

            IntentFilter filter = new IntentFilter("com.bible.alarm.NOTIFICATION");
            Application.Context.RegisterReceiver(notificationReceiver, filter);
        }

        [IntentFilter(new[] { "com.bible.alarm.NOTIFICATION" })]
        public class AlarmTask : BroadcastReceiver
        {
            private IPlaybackService playbackService;

            public AlarmTask()
                : base()
            {
                if (IocSetup.Container == null)
                {
                    Bible.Alarm.Droid.IocSetup.Initialize();
                }

                this.playbackService = IocSetup.Container.Resolve<IPlaybackService>();
            }

            public override void OnReceive(Context context, Intent intent)
            {
                Analytics.TrackEvent($"I am called at {DateTime.Now}.");

                var scheduleId = intent.GetStringExtra("ScheduleId");

                PendingResult result = GoAsync();

                Task.Run(async () =>
                {
                    var id = long.Parse(scheduleId);
                    await Messenger<object>.Publish(Messages.SnoozeDismiss, IocSetup.Container.Resolve<AlarmViewModal>());
                    await playbackService.Play(id);

                    result.SetResult(Result.Ok, null, null);
                    result.Finish();
                });
            }

        }
    }

    [BroadcastReceiver(Enabled = true, Exported = true, DirectBootAware = true)]
    [IntentFilter(new[] { "com.bible.alarm.RESTART", Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted,
        "android.intent.action.QUICKBOOT_POWERON", "com.htc.intent.action.QUICKBOOT_POWERON"})]
    public class RestartTask : BroadcastReceiver
    {

        public RestartTask()
            : base()
        {

        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                context.StartForegroundService(new Intent(context, typeof(AlarmRingerService)));
            }
            else
            {
                context.StartService(new Intent(context, typeof(AlarmRingerService)));

            }
        }

    }
}
