using System;
using Android.App;
using Android.Content;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Droid.Tasks;
using NLog;
using System.Threading.Tasks;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.Droid.Services.Platform;
using Android.OS;

namespace Bible.Alarm.Droid.Services.Tasks
{

    [BroadcastReceiver(Enabled = true, DirectBootAware = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted,
        "android.intent.action.QUICKBOOT_POWERON", "com.htc.intent.action.QUICKBOOT_POWERON",
        "com.Bible.Alarm.Restart"})]
    public class RestartReceiver : BroadcastReceiver
    {
        private IContainer container;
        private Logger logger;

        public RestartReceiver()
        {
            LogSetup.Initialize(VersionFinder.Default,
                new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" });
            logger = LogManager.GetCurrentClassLogger();
        }
        public override async void OnReceive(Context context, Intent intent)
        {
            var pendingIntent = GoAsync();

            try
            {
                var result = IocSetup.Initialize(context, true);
                this.container = result.Item1;

                BootstrapHelper.VerifyBackgroundTasks(container.AndroidContext());

                var task1 = BootstrapHelper.VerifyMediaLookUpService(container);
                var task2 = BootstrapHelper.InitializeDatabase(container);

                await Task.WhenAll(task1, task2);

                using (var schedulerTask = container.Resolve<SchedulerTask>())
                {
                    await schedulerTask.Handle();
                }

                context.StopService(intent);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to process restart task. Intent action {intent.Action}");
            }
            finally
            {
                pendingIntent.Finish();
            }
        }
    }
}