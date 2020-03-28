using Android.App;
using Android.Content;
using Android.OS;
using Bible.Alarm.Droid.Services.Platform;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Droid.Tasks;
using Bible.Alarm.Services.Infrastructure;
using NLog;
using System;
using System.Threading.Tasks;

namespace Bible.Alarm.Droid.Services.Tasks
{

    [BroadcastReceiver(Enabled = true, DirectBootAware = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted,
        "android.intent.action.QUICKBOOT_POWERON", "com.htc.intent.action.QUICKBOOT_POWERON",
        "com.Bible.Alarm.Restart"})]
    public class RestartReceiver : BroadcastReceiver
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        private IContainer container;

        public RestartReceiver()
        {
            LogSetup.Initialize(VersionFinder.Default,
                new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" });
        }
        public override async void OnReceive(Context context, Intent intent)
        {
            var pendingIntent = GoAsync();

            try
            {
                this.container = BootstrapHelper.InitializeService(context);

                BootstrapHelper.VerifyBackgroundTasks(container.AndroidContext());

                await BootstrapHelper.VerifyServices(container);

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