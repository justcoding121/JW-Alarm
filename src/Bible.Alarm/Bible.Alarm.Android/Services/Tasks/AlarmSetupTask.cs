using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Bible.Alarm.Services.Infrastructure;
using MediaManager;
using NLog;
using System;

namespace JW.Alarm.Services.Droid.Tasks
{
    [Service(Enabled = true)]
    [IntentFilter(new[] { "com.bible.alarm.SETUP" })]
    public class AlarmSetupTask : Service
    {
        private static Logger logger => LogManager.GetCurrentClassLogger();

        public static bool IsRunning = false;
      
        public AlarmSetupTask() : base()
        {
            LogSetup.Initialize();

            if (IocSetup.Container == null)
            {
                Bible.Alarm.Droid.IocSetup.Initialize();
                IocSetup.Container.Resolve<IMediaManager>().Init(this);
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            base.OnCreate();

            IsRunning = true;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        { 
            try
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
                        throw new NotImplementedException();
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened in alarm setup task.");
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
