using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Bible.Alarm.Services.Infrastructure;
using MediaManager;
using NLog;
using System;
using Bible.Alarm.Droid.Services.Tasks;
using Bible.Alarm.Services.Droid.Helpers;

namespace Bible.Alarm.Services.Droid.Tasks
{
    [Service(Enabled = true)]
    public class AlarmSetupService : Service
    {
        private static Logger logger => LogManager.GetCurrentClassLogger();

        public static bool IsRunning = false;

        public AlarmSetupService() : base()
        {
            LogSetup.Initialize();
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
                if (IocSetup.Container == null)
                {
                    Bible.Alarm.Droid.IocSetup.Initialize(this, true);
                    IocSetup.Container.Resolve<IMediaManager>().Init(this);
                }

                var extra = intent.GetStringExtra("Action");

                switch (extra)
                {
                    case "Add":
                        {
                            var time = DateTimeOffset.Parse(intent.GetStringExtra("Time"));
                            var title = intent.GetStringExtra("Title");
                            var body = intent.GetStringExtra("Body");
                            AddNotification(this, long.Parse(intent.GetStringExtra("ScheduleId")), time, title, body);
                            break;
                        }
                    case "SetupBackgroundTasks":
                        BootstrapHelper.VerifyBackgroundTasks(this);
                        break;
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

        public static void AddNotification(Context context, long scheduleId, DateTimeOffset time,
            string title, string body)
        {
            var alarmIntent = new Intent(context, typeof(AlarmRingerReceiver));
            alarmIntent.PutExtra("ScheduleId", scheduleId.ToString());

            var pIntent = PendingIntent.GetBroadcast(
                    context,
                    (int)scheduleId,
                    alarmIntent,
                    PendingIntentFlags.UpdateCurrent);

            var alarmService = (AlarmManager)context.GetSystemService(Context.AlarmService);

            // Figure out the alaram in milliseconds.
            var milliSecondsRemaining = Java.Lang.JavaSystem.CurrentTimeMillis()
                + (long)time.Subtract(DateTimeOffset.Now).TotalSeconds * 1000;

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
    }
}
