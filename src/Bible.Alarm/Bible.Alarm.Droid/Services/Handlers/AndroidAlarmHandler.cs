using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Bible.Alarm.Common.Extensions;
using Bible.Alarm.Services.Contracts;
using MediaManager;
using NLog;

namespace Bible.Alarm.Droid.Services.Handlers
{
    public class AndroidAlarmHandler : IDisposable
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        private IMediaManager mediaManager;
        private IPlaybackService playbackService;
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
            else
            {
                mediaManager.Init(Application.Context);
            }

            await Task.Run(async () =>
            {
                try
                {
                    await playbackService.Play(scheduleId, isImmediate);
                }
                catch (Exception e)
                {
                    logger.Error(e, "An error happened when ringing the alarm.");
                    Dispose();
                }
            });
        }

        private void onStopped(object sender, bool disposeMediaManager)
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

        private bool disposed = false;
        private void dispose(bool disposeMediaManager)
        {
            if (!disposed)
            {
                disposed = true;

                if (playbackService != null)
                {
                    playbackService.Stopped -= onStopped;
                    playbackService.Dispose();
                }

                if (disposeMediaManager)
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

                    mediaManager?.Queue?.Clear();                
                }

                mediaManager?.Dispose();
                Disposed?.Invoke(this, true);
            }
        }

        public void Dispose()
        {
            dispose(false);
        }
    }
}