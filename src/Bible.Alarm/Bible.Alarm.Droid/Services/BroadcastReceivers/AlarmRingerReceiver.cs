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
        private Logger logger;

        private IPlaybackService playbackService;
        private Context context;
        private Intent intent;
        private IMediaManager mediaManager;

        private static SemaphoreSlim @lock = new SemaphoreSlim(1);
        public AlarmRingerReceiver()
        {
            LogSetup.Initialize(VersionFinder.Default,
             new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" });

            logger = LogManager.GetCurrentClassLogger();
        }

        public async override void OnReceive(Context context, Intent intent)
        {
            var pendingIntent = GoAsync();

            try
            {
                await @lock.WaitAsync();

                var container = BootstrapHelper.InitializeService(context);

                this.context = context;
                this.intent = intent;


                this.mediaManager = container.Resolve<IMediaManager>();
                this.playbackService = container.Resolve<IPlaybackService>();

                if (mediaManager.IsPrepared())
                {
                    context.StopService(intent);
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
                        await Messenger<object>.Publish(MvvmMessages.ShowAlarmModal);
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
                playbackService.Stopped -= stateChanged;
                mediaManager?.Dispose();
                context.StopService(intent);
            }
            finally
            {
                @lock.Release();
                pendingIntent.Finish();
            }

        }

        private void stateChanged(object sender, MediaPlayerState e)
        {
            try
            {
                if (e == MediaPlayerState.Stopped)
                {
                    playbackService.Stopped -= stateChanged;
                    mediaManager?.Dispose();
                    context.StopService(intent);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error happened when stopping the alarm after media failure.");
            }
        }
    }
}