using Android;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Bible.Alarm.Services;
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
    [BroadcastReceiver]
    public class AlarmTask : BroadcastReceiver
    {
        private IPlaybackService playbackService;
        private AlarmReceiver alarmReceiver;

        public AlarmTask()
            : base()
        {
            if (IocSetup.Container == null)
            {
                Bible.Alarm.Droid.IocSetup.Initialize();
            }

            this.playbackService = IocSetup.Container.Resolve<IPlaybackService>();
            this.alarmReceiver = IocSetup.Container.Resolve<AlarmReceiver>();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            var scheduleId = intent.GetStringExtra("ScheduleId");

            PendingResult result = GoAsync();

            Task.Run(async () =>
            {
                var id = long.Parse(scheduleId);
                alarmReceiver.Raise(id);
                await playbackService.Play(id);

                result.SetResult(Result.Ok, null, null);
                result.Finish();
            });
        }

    }
}
