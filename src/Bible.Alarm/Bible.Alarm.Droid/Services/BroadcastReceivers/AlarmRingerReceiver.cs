using Android.App;
using Android.Content;
using Android.OS;
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
             new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" });
        }

        public async override void OnReceive(Context context, Intent intent)
        {
            var pendingIntent = GoAsync();

            var isBusy = false;

            try
            {
                await @lock.WaitAsync();

                container = BootstrapHelper.InitializeService(context);

                this.context = context;
                this.intent = intent;

                this.mediaManager = container.Resolve<IMediaManager>();
                this.playbackService = container.Resolve<IPlaybackService>();

                if (mediaManager.IsPrepared())
                {
                    context.StopService(intent);
                    isBusy = true;
                    return;
                }
                else
                {
                    mediaManager.Init(Application.Context);
                }

                playbackService.Stopped += stateChanged;

                var scheduleId = intent.GetStringExtra("ScheduleId");

                await Task.Run(async () =>
                {
                    try
                    {
                        var id = long.Parse(scheduleId);

                        await playbackService.Play(id);
                        Messenger<object>.Publish(MvvmMessages.ShowAlarmModal);
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
            }
            finally
            {
                @lock.Release();
                pendingIntent.Finish();

                if (isBusy)
                {
                    context.StopService(intent);
                    Dispose();
                }
            }

        }

        private void stateChanged(object sender, MediaPlayerState e)
        {
            try
            {
                if (e == MediaPlayerState.Stopped)
                {
                    context.StopService(intent);
                    Dispose();         
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error happened when stopping the alarm after media failure.");
            }
        }

        public new void Dispose()
        {
            if (playbackService != null)
            {
                playbackService.Stopped -= stateChanged;
            }

            playbackService?.Dispose();
            mediaManager?.Dispose();

            container = null;

            base.Dispose();
        }
    }
}