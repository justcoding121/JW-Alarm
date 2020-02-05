using Android.App;
using Android.Content;
using Android.OS;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Droid.Services.Platform;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.Services.Infrastructure;
using MediaManager;
using MediaManager.Player;
using NLog;
using System;
using System.Threading.Tasks;

namespace Bible.Alarm.Droid.Services.Tasks
{
    [BroadcastReceiver(Enabled = true)]
    public class AlarmRingerReceiver : BroadcastReceiver, IDisposable
    {
        private IContainer container;
        private Logger logger;

        private IPlaybackService playbackService;
        private Context context;
        private Intent intent;
        private IMediaManager mediaManager;

        public AlarmRingerReceiver()
        {
            LogSetup.Initialize(VersionFinder.Default,
             new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" });

            logger = LogManager.GetCurrentClassLogger();
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

        public async override void OnReceive(Context context, Intent intent)
        {
            var pendingIntent = GoAsync();

            try
            {
                this.context = context;
                this.intent = intent;

                var result = IocSetup.Initialize(context, true);
                this.container = result.Item1;

                if (container.Resolve<IMediaManager>().IsPrepared())
                {
                    context.StopService(intent);
                    return;
                }

                this.playbackService = container.Resolve<IPlaybackService>();

                playbackService.Stopped += stateChanged;

                mediaManager = container.Resolve<IMediaManager>();
                if (!mediaManager.IsPrepared())
                {
                    mediaManager.Init(Application.Context);
                }

                var scheduleId = intent.GetStringExtra("ScheduleId");

                await Task.Run(async () =>
                {
                    try
                    {
                        var id = long.Parse(scheduleId);

                        await playbackService.Play(id);

                        await Messenger<object>.Publish(Messages.ShowAlarmModal);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "An error happened when ringing the alarm.");
                    }
                });
            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened when creating the task to ring the alarm.");
            }
            finally
            {
                pendingIntent.Finish();
            }

        }

    }
}