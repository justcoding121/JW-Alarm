using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services.Contracts;
using Foundation;
using MediaManager;
using MediaManager.Player;
using NLog;
using UIKit;

namespace Bible.Alarm.iOS.Services.Handlers
{
    public class iOSAlarmHandler : IDisposable
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        private IPlaybackService playbackService;
        private IMediaManager mediaManager;

        private static SemaphoreSlim @lock = new SemaphoreSlim(1);

        public iOSAlarmHandler(IPlaybackService playbackService, IMediaManager mediaManager)
        {
            this.playbackService = playbackService;
            this.mediaManager = mediaManager;
        }

        public async Task Handle(long scheduleId)
        {
            try
            {
                await @lock.WaitAsync();

                if (mediaManager.IsPrepared())
                {
                    await Task.CompletedTask;
                    return;
                }
                else
                {
                    mediaManager.Init();
                }

                playbackService.Stopped += stateChanged;

                await Task.Run(async () =>
                {
                    try
                    {
                        await playbackService.Play(scheduleId);
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
            }
            finally
            {
                @lock.Release();
            }
        }

        private void stateChanged(object sender, MediaPlayerState e)
        {
            try
            {
                if (e == MediaPlayerState.Stopped)
                {
                    playbackService.Stopped -= stateChanged;
                    playbackService.Dispose();
                    mediaManager.Dispose();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error happened when stopping the alarm after media failure.");
            }
        }

        public void Dispose()
        {
            this.playbackService.Dispose();
            this.mediaManager.Dispose();
        }
    }
}