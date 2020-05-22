using Android.App;
using Android.Content;
using Android.OS;
using Bible.Alarm.Common.Extensions;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Droid.Services.Handlers;
using Bible.Alarm.Droid.Services.Platform;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Infrastructure;
using MediaManager;
using MediaManager.Player;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bible.Alarm.Droid.Services.Tasks
{
    [BroadcastReceiver(Enabled = true)]
    public class AlarmRingerReceiver : BroadcastReceiver, IDisposable
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        private IContainer container;
        private Context context;
        private Intent intent;
        private AndroidAlarmHandler alarmHandler;

        private static readonly SemaphoreSlim @lock = new SemaphoreSlim(1);
        public AlarmRingerReceiver()
        {
            LogSetup.Initialize(VersionFinder.Default,
             new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" }, Xamarin.Forms.Device.Android);
        }

        public event EventHandler<bool> Stopped;

        public async override void OnReceive(Context context, Intent intent)
        {
            var pendingIntent = GoAsync();

            await @lock.WaitAsync();

            try
            {
                container = BootstrapHelper.InitializeService(context);

                this.context = context;
                this.intent = intent;

                var scheduleId = intent.GetStringExtra("ScheduleId");
                var isImmediate = !string.IsNullOrEmpty(intent.GetStringExtra("IsImmediate"));

                alarmHandler = container.Resolve<AndroidAlarmHandler>();
                alarmHandler.Disposed += onDisposed;
                await alarmHandler.Handle(long.Parse(scheduleId), isImmediate);
            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened when creating the task to ring the alarm.");
                Dispose();
            }
            finally
            {
                @lock.Release();
                pendingIntent.Finish();
            }
        }

        private void onDisposed(object sender, bool e)
        {
            Dispose();
        }

        public new void Dispose()
        {
            dispose();
            base.Dispose();
        }

        private bool disposed = false;
        private void dispose()
        {
            if (!disposed)
            {
                disposed = true;

                if (alarmHandler != null)
                {
                    alarmHandler.Disposed -= onDisposed;
                }

                context?.StopService(intent);
            }
        }
    }
}