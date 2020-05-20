using Android.App;
using Android.Content;
using Android.OS;
using Bible.Alarm.Common.Extensions;
using Bible.Alarm.Common.Mvvm;
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
        private IPlaybackService playbackService;
        private Context context;
        private Intent intent;
        private IMediaManager mediaManager;

        private static SemaphoreSlim @lock = new SemaphoreSlim(1);
        public AlarmRingerReceiver()
        {
            LogSetup.Initialize(VersionFinder.Default,
             new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" }, Xamarin.Forms.Device.Android);
        }

        public async override void OnReceive(Context context, Intent intent)
        {
            var pendingIntent = GoAsync();

            await @lock.WaitAsync();

            try
            {
                container = BootstrapHelper.InitializeService(context);

                this.context = context;
                this.intent = intent;

                this.mediaManager = container.Resolve<IMediaManager>();
                this.playbackService = container.Resolve<IPlaybackService>();
                playbackService.Stopped += stateChanged;

                if (mediaManager.IsPreparedEx())
                {
                    context.StopService(intent);
                    Dispose();
                    return;
                }

                var scheduleId = intent.GetStringExtra("ScheduleId");
                var isImmediate = !string.IsNullOrEmpty(intent.GetStringExtra("IsImmediate"));

                await Task.Run(async () =>
                {
                    try
                    {
                        var id = long.Parse(scheduleId);
                        await playbackService.Play(id, isImmediate);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "An error happened when ringing the alarm.");
                        throw;
                    }
                });
            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened when creating the task to ring the alarm.");

                context.StopService(intent);
                Dispose();
                throw;
            }
            finally
            {
                @lock.Release();
                pendingIntent.Finish();
            }
        }

        private void stateChanged(object sender, bool disposeMediaManager)
        {
            try
            {
                dispose(disposeMediaManager);

            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error happened when stopping the alarm after media failure.");
            }
        }

        public new void Dispose()
        {
            dispose(false);
            base.Dispose();
        }

        private bool disposed = false;
        private void dispose(bool disposeMediaManager)
        {
            if (!disposed)
            {
                disposed = true;

                if (playbackService != null)
                {
                    playbackService.Stopped -= stateChanged;
                    playbackService.Dispose();
                }    

                if (disposeMediaManager)
                {
                    mediaManager?.Queue?.Clear();
                }
       
            }
        }
    }
}