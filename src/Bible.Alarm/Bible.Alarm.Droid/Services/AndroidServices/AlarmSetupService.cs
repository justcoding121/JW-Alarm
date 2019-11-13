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
using static Android.App.AlarmManager;
using Bible.Alarm.Droid;

namespace Bible.Alarm.Services.Droid.Tasks
{
    [Service(Enabled = true)]
    public class AlarmSetupService : Service
    {
        private static Logger logger => LogHelper.GetLogger(global::Xamarin.Forms.Forms.IsInitialized);

        public static bool IsRunning = false;

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
                Bible.Alarm.Droid.IocSetup.Initialize(this, true);

                var extra = intent.GetStringExtra("Action");

                switch (extra)
                {
                    case "Add":
                        {
                            var time = DateTimeOffset.Parse(intent.GetStringExtra("Time"));
                            var title = intent.GetStringExtra("Title");
                            var body = intent.GetStringExtra("Body");
                            ScheduleNotification(IocSetup.Context, long.Parse(intent.GetStringExtra("ScheduleId")), time, title, body);
                            break;
                        }
                    case "SetupBackgroundTasks":
                        BootstrapHelper.VerifyBackgroundTasks(IocSetup.Context);
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

        public static void ScheduleNotification(Context context, long scheduleId, DateTimeOffset time,
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

            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
            {
                alarmService.SetExact(AlarmType.RtcWakeup, milliSecondsRemaining, pIntent);
            }
            else
            {
                var mainLauncherIntent = new Intent(IocSetup.Context, typeof(SplashActivity));
                mainLauncherIntent.SetFlags(ActivityFlags.ReorderToFront);

                var mainLauncherPendingIntent = PendingIntent.GetActivity(
                   IocSetup.Context,
                   0,
                   mainLauncherIntent,
                   PendingIntentFlags.UpdateCurrent);


                alarmService.SetAlarmClock(new AlarmClockInfo(milliSecondsRemaining, mainLauncherPendingIntent), pIntent);
            }

        }

        public static void ShowNotification(Context context, long scheduleId)
        {
            var alarmIntent = new Intent(context, typeof(AlarmRingerReceiver));
            alarmIntent.PutExtra("ScheduleId", scheduleId.ToString());

            context.SendBroadcast(alarmIntent);
        }
    }
}
