using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.ViewModels;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services.Contracts;
using MediaManager;
using MediaManager.Player;
using NLog;

namespace Bible.Alarm.Droid.Services.Tasks
{
    [BroadcastReceiver(Enabled = true)]
    public class AlarmRingerReceiver : BroadcastReceiver
    {
        private static Logger logger => LogManager.GetCurrentClassLogger();

        private IPlaybackService playbackService;
        private Context context;
        private Intent intent;
        public AlarmRingerReceiver() : base()
        {
            LogSetup.Initialize("Android");
        }

        private void stateChanged(object sender, MediaPlayerState e)
        {
            try
            {
                if (e == MediaPlayerState.Stopped
                    || e == MediaPlayerState.Failed)
                {
                    logger.Info($"Alarm stopped with the status {e.ToString()}.");
                    playbackService.StateChanged -= stateChanged;
                    context.StopService(intent);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error happened when stopping the alarm after media failure.");
            }
        }

        public override void OnReceive(Context context, Intent intent)
        {
            logger.Info($"Alarm Rang.");

            try
            {
                this.context = context;
                this.intent = intent;

                IocSetup.Initialize(context, true);

                this.playbackService = IocSetup.Container.Resolve<IPlaybackService>();

                playbackService.StateChanged += stateChanged;

                var mediaManager = IocSetup.Container.Resolve<IMediaManager>();

                if (!mediaManager.IsPrepared())
                {
                    mediaManager.Init(Application.Context);
                }
                else
                {
                    logger.Info("Media Manager was already initialized.");
                }

                var scheduleId = intent.GetStringExtra("ScheduleId");

                Task.Run(async () =>
                {
                    try
                    {
                        var id = long.Parse(scheduleId);

                        await playbackService.Play(id);

                        if (IocSetup.Container.RegisteredTypes.Any(x => x == typeof(Xamarin.Forms.INavigation)))
                        {
                            await Messenger<object>.Publish(Messages.ShowSnoozeDismissModal, IocSetup.Container.Resolve<AlarmViewModal>());
                        }
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

        }

    }
}