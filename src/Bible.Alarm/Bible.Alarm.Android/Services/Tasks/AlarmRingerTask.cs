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
using Bible.Alarm.ViewModels;
using JW.Alarm.Common.Mvvm;
using JW.Alarm.Services.Contracts;
using MediaManager;
using MediaManager.Playback;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;

namespace Bible.Alarm.Droid.Services.Tasks
{
    [Service]
    [IntentFilter(new[] { "com.bible.alarm.RING" })]
    public class AlarmRingerService : Service
    {
        private IPlaybackService playbackService;

        public AlarmRingerService() : base()
        {
            if (!AppCenter.Configured)
            {
                AppCenter.Start("0cd5c3e8-dcfa-48dd-9d4b-0433a8572fb9",
                  typeof(Analytics));
            }

            if (IocSetup.Container == null)
            {
                Analytics.TrackEvent($"Container null for AlarmService at {DateTime.Now}.");
                Bible.Alarm.Droid.IocSetup.Initialize();
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
            IocSetup.Container.Resolve<IMediaManager>().SetContext(this);

            Analytics.TrackEvent($"Alarm rang at {DateTime.Now}.");

            var scheduleId = intent.GetStringExtra("ScheduleId");

            Analytics.TrackEvent($"Alarm play started at {DateTime.Now}.");

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

                    Analytics.TrackEvent($"Alarm played at {DateTime.Now}.");

                    if (IocSetup.Container.Resolve<IMediaManager>().IsPlaying())
                    {
                        Analytics.TrackEvent($"Media Manager playing at {DateTime.Now}.");
                    }
                    else
                    {
                        Analytics.TrackEvent($"Media Manager not playing at {DateTime.Now}.");
                    }
                }
                catch (Exception e)
                {
                    Analytics.TrackEvent($"Error at {DateTime.Now}. Stack: {e.ToString()}");
                }
            });

            return base.OnStartCommand(intent, flags, startId);
        }

        public override void OnCreate()
        {

        }

        public override void OnDestroy()
        {

        }

    }
}