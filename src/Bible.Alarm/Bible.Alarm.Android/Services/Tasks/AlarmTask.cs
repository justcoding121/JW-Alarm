using Android.App;
using Android.Content;
using Android.OS;
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

        public AlarmTask()
            : base()
        {
            this.playbackService = IocSetup.Container.Resolve<IPlaybackService>();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            var scheduleId = intent.GetStringExtra("ScheduleId");

            PendingResult result = GoAsync();

            Task.Run(async () =>
            {
                await (playbackService as PlaybackService).Play(long.Parse(scheduleId), context);

                await Task.Delay(1000 * 8);
                result.SetResult(Result.Ok, null, new Bundle());          
                result.Finish();
            });
        }
    }
}
