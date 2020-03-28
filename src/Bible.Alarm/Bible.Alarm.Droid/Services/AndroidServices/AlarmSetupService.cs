using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Bible.Alarm.Droid;
using Bible.Alarm.Droid.Services.Platform;
using Bible.Alarm.Droid.Services.Tasks;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Infrastructure;
using NLog;
using System;
using System.Threading.Tasks;
using static Android.App.AlarmManager;

namespace Bible.Alarm.Services.Droid.Tasks
{
    [Service(Enabled = true)]
    public class AlarmSetupService : Service, IDisposable
    {
        private IContainer container;
        private Logger logger;

        public static bool IsRunning = false;

        public AlarmSetupService()
        {
            LogSetup.Initialize(VersionFinder.Default,
                new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" });

            logger = LogManager.GetCurrentClassLogger();
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
                this.container = BootstrapHelper.InitializeService(this);

                var extra = intent.GetStringExtra("Action");

                switch (extra)
                {
                    case "Add":
                        {
                            var time = DateTimeOffset.Parse(intent.GetStringExtra("Time"));
                            var title = intent.GetStringExtra("Title");
                            var body = intent.GetStringExtra("Body");
                            ScheduleNotification(container.AndroidContext(), long.Parse(intent.GetStringExtra("ScheduleId")), time, title, body);
                            break;
                        }
                    case "SetupBackgroundTasks":
                        BootstrapHelper.VerifyBackgroundTasks(container.AndroidContext());
                        Task.Run(async () =>
                        {
                            try
                            {
                                using (var schedulerTask = container.Resolve<SchedulerTask>())
                                {
                                    await schedulerTask.Handle();
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Error(e, "An error happened in handling scheduler task.");
                            }
                        });
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
            using (var alarmIntent = new Intent(context, typeof(AlarmRingerReceiver)))
            {
                alarmIntent.PutExtra("ScheduleId", scheduleId.ToString());

                using (var pIntent = PendingIntent.GetBroadcast(
                         context,
                         (int)scheduleId,
                         alarmIntent,
                         PendingIntentFlags.UpdateCurrent))
                using (var alarmService = (AlarmManager)context.GetSystemService(Context.AlarmService))
                {

                    // Figure out the alaram in milliseconds.
                    var milliSecondsRemaining = Java.Lang.JavaSystem.CurrentTimeMillis()
                        + (long)time.Subtract(DateTimeOffset.Now).TotalSeconds * 1000;

                    if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                    {
                        alarmService.SetExact(AlarmType.RtcWakeup, milliSecondsRemaining, pIntent);
                    }
                    else
                    {
                        using (var mainLauncherIntent = new Intent(context, typeof(SplashActivity)))
                        {
                            mainLauncherIntent.SetFlags(ActivityFlags.ReorderToFront);

                            var mainLauncherPendingIntent = PendingIntent.GetActivity(
                               context,
                               0,
                               mainLauncherIntent,
                               PendingIntentFlags.UpdateCurrent);

                            alarmService.SetAlarmClock(new AlarmClockInfo(milliSecondsRemaining, mainLauncherPendingIntent), pIntent);
                        }
                    }
                }
            }
        }

        public static void ShowNotification(Context context, long scheduleId)
        {
            using (var alarmIntent = new Intent(context, typeof(AlarmRingerReceiver)))
            {
                alarmIntent.PutExtra("ScheduleId", scheduleId.ToString());
                context.SendBroadcast(alarmIntent);
            }
        }

    }
}
