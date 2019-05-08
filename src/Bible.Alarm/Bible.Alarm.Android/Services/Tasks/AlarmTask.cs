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

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            RegisterBroadcastReceiver();
        }

        public override void OnDestroy()
        {
            Application.Context.UnregisterReceiver(notificationReceiver);
            notificationReceiver = null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
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


}
