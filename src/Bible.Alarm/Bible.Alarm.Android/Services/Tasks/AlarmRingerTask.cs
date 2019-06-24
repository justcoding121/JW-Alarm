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
using JW.Alarm.Common.Mvvm;
using JW.Alarm.Services.Contracts;
using MediaManager;
using MediaManager.Playback;
using Microsoft.Extensions.Logging;
using NLog;

namespace Bible.Alarm.Droid.Services.Tasks
{
    [Service(Enabled = true)]
    [IntentFilter(new[] { "com.bible.alarm.RING" })]
    public class AlarmRingerService : Service
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private IPlaybackService playbackService;

        public AlarmRingerService() : base()
        {
            LogSetup.Initialize();

            if (IocSetup.Container == null)
            {
                IocSetup.Initialize();
                IocSetup.Container.Resolve<IMediaManager>().SetContext(this);
            }

            this.playbackService = IocSetup.Container.Resolve<IPlaybackService>();

            playbackService.StateChanged += stateChanged;
        }

        private void stateChanged(object sender, MediaPlayerState e)
        {
            if (e == MediaPlayerState.Stopped
            || e == MediaPlayerState.Failed)
            {
                playbackService.StateChanged -= stateChanged;
                StopSelf();
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            logger.Info($"Alarm rang at {DateTime.Now}");

            try
            {
                IocSetup.Container.Resolve<IMediaManager>().SetContext(this);

                var scheduleId = intent.GetStringExtra("ScheduleId");

                Task.Run(async () =>
                {
                    try
                    {
                        var id = long.Parse(scheduleId);
                        if (IocSetup.Container.RegisteredTypes.Any(x => x == typeof(Xamarin.Forms.INavigation)))
                        {
                            await Messenger<object>.Publish(Messages.ShowSnoozeDismissModal, IocSetup.Container.Resolve<AlarmViewModal>());
                        }

                        await playbackService.Play(id);

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

            return StartCommandResult.Sticky;
        }

        public override void OnCreate()
        {

        }

        public override void OnDestroy()
        {

        }

    }
}