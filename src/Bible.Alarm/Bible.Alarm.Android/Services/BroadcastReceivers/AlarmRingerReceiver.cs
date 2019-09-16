using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.ViewModels;
using Com.Google.Android.Exoplayer2.UI;
using JW.Alarm.Common.Mvvm;
using JW.Alarm.Services.Contracts;
using MediaManager;
using MediaManager.Playback;
using MediaManager.Player;
using Microsoft.Extensions.Logging;
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
            LogSetup.Initialize();
        }

        private void stateChanged(object sender, MediaPlayerState e)
        {
            if (e == MediaPlayerState.Stopped
            || e == MediaPlayerState.Failed)
            {
                try
                {
                    playbackService.StateChanged -= stateChanged;
                    context.StopService(intent);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "An error happened when stopping the alarm after media failure.");
                }
            }
        }

        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                this.context = context;
                this.intent = intent;

                if (IocSetup.Container == null)
                {
                    IocSetup.Initialize(context, true);
                    IocSetup.Container.Resolve<IMediaManager>().Init(Application.Context);
                }

                this.playbackService = IocSetup.Container.Resolve<IPlaybackService>();

                playbackService.StateChanged += stateChanged;
                logger.Info($"Alarm rang at {DateTime.Now}");

                IocSetup.Container.Resolve<IMediaManager>().Init(Application.Context);

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