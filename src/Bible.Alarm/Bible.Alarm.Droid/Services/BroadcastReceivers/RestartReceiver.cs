using System;
using Android.App;
using Android.Content;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Droid.Tasks;
using NLog;
using System.Threading.Tasks;

namespace Bible.Alarm.Droid.Services.Tasks
{

    [BroadcastReceiver(Enabled = true, DirectBootAware = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted,
        "android.intent.action.QUICKBOOT_POWERON", "com.htc.intent.action.QUICKBOOT_POWERON", "com.Bible.Alarm.Restart"})]
    public class RestartReceiver : BroadcastReceiver
    {
        private static Logger logger => LogHelper.GetLogger(global::Xamarin.Forms.Forms.IsInitialized);

        public override async void OnReceive(Context context, Intent intent)
        {
            var pendingIntent = GoAsync();

            try
            {

                IocSetup.Initialize(context, true);

                var task1 = BootstrapHelper.VerifyMediaLookUpService();
                var task2 = BootstrapHelper.InitializeDatabase();

                await Task.WhenAll(task1, task2);

                using (var schedulerTask = IocSetup.Container.Resolve<SchedulerTask>())
                {
                    await schedulerTask.Handle();
                }
                  
                context.StopService(intent);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to process restart task.");
            }
            finally
            {
                pendingIntent.Finish();
            }
        }
    }
}