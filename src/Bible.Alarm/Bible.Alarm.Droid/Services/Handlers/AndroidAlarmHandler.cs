using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Bible.Alarm.Common.Extensions;
using Bible.Alarm.Contracts.Media;
using Bible.Alarm.Services.Contracts;
using Com.Google.Android.Exoplayer2.UI;
using MediaManager;
using NLog;

namespace Bible.Alarm.Droid.Services.Handlers
{
    public class AndroidAlarmHandler : IAndroidAlarmHandler, IDisposable
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        private IMediaManager mediaManager;
        private IPlaybackService playbackService;
        private bool mediaManagerInitialized = false;
        private PlayerNotificationManager playerNotificationManager;

        public AndroidAlarmHandler(IMediaManager mediaManager, IPlaybackService playbackService)
        {
            this.mediaManager = mediaManager;
            this.playbackService = playbackService;
            playbackService.Stopped += onStopped;
        }

        public event EventHandler<bool> Disposed;

        public async Task Handle(long scheduleId, bool isImmediate)
        {
            if (mediaManager.IsPreparedEx())
            {
                Dispose();
                return;
            }

            mediaManager.Init(Application.Context);

            await Task.Run(async () =>
            {
                try
                {
                    await playbackService.Play(scheduleId, isImmediate);

                    mediaManagerInitialized = true;

                    playerNotificationManager = (mediaManager.Notification as MediaManager.Platforms.Android.Notifications.NotificationManager).PlayerNotificationManager;
                    playerNotificationManager.NotificationCancelled += notificationCancelled;
                }
                catch (Exception e)
                {
                    logger.Error(e, "An error happened when ringing the alarm.");
                    Dispose();
                }
            });
        }

        private async void notificationCancelled(object sender, PlayerNotificationManager.NotificationCancelledEventArgs e)
        {
            if (e.DismissedByUser)
            {
                await mediaManager.StopEx();
            }
        }

        private void onStopped(object sender, bool resetMediaManager)
        {
            try
            {
                dispose(resetMediaManager);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error happened when stopping the alarm after media failure.");
            }
        }

        private bool disposed = false;
        private void dispose(bool resetMediaManager)
        {
            if (!disposed)
            {
                disposed = true;

                if (playbackService != null)
                {
                    playbackService.Stopped -= onStopped;
                    playbackService.Dispose();
                }

                if (playerNotificationManager != null)
                {
                    playerNotificationManager.NotificationCancelled -= notificationCancelled;
                }

                if (resetMediaManager)
                {
                    try
                    {
                        if (!mediaManager.IsStopped())
                        {
                            mediaManager.StopEx().Wait();
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "An error happened on calling StopEx.");
                    }

                }

                if (mediaManagerInitialized)
                {
                    mediaManager?.Dispose();
                }

                Disposed?.Invoke(this, true);
            }
        }

        public void Dispose()
        {
            dispose(false);
        }
    }
}