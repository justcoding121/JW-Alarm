using Android.App;
using Android.Content;
using Android.OS;
using Bible.Alarm.Droid.Services.Platform;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Droid.Tasks;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.Services.Tasks;
using NLog;
using System;
using Xamarin.Forms;

namespace Bible.Alarm.Droid.Services.Tasks
{

    [BroadcastReceiver(Enabled = true, DirectBootAware = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted,
        "android.intent.action.QUICKBOOT_POWERON", "com.htc.intent.action.QUICKBOOT_POWERON",
        "com.Bible.Alarm.Restart"})]
    public class RestartReceiver : BroadcastReceiver, IDisposable
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        private IContainer container;

        public RestartReceiver()
        {
            LogSetup.Initialize(VersionFinder.Default,
                new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" }, Device.Android);
        }
        public override async void OnReceive(Context context, Intent intent)
        {
            var pendingIntent = GoAsync();

            try
            {
                this.container = BootstrapHelper.InitializeService(context);

                BootstrapHelper.VerifyBackgroundTasks(container.AndroidContext());

                try
                {
                    await BootstrapHelper.VerifyServices(container);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"Failed to process restart task: copy media index. Intent action {intent.Action}");
                }

                using var schedulerTask = container.Resolve<SchedulerTask>();
                await schedulerTask.Handle();

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

        public new void Dispose()
        {
            base.Dispose();
            container = null;
        }
    }
}