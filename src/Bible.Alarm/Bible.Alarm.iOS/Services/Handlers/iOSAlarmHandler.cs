using Bible.Alarm.Common.Extensions;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services;
using Bible.Alarm.Services.Contracts;
using MediaManager;
using MediaManager.Player;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using UIKit;

namespace Bible.Alarm.iOS.Services.Handlers
{
    public class iOSAlarmHandler : IDisposable
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        private IPlaybackService playbackService;
        private IMediaManager mediaManager;
        private ScheduleDbContext dbContext;
        private TaskScheduler taskScheduler;

        private static SemaphoreSlim @lock = new SemaphoreSlim(1);

        //Need this to fix issue in XamarinMediaManager (notification stays on screen)
        private static bool firstTime = true;

        public iOSAlarmHandler(IPlaybackService playbackService,
                                IMediaManager mediaManager,
                                ScheduleDbContext dbContext,
                                TaskScheduler taskScheduler)
        {
            this.playbackService = playbackService;
            this.mediaManager = mediaManager;
            this.dbContext = dbContext;
            this.taskScheduler = taskScheduler;

            playbackService.Stopped += stateChanged;

        }

        public async Task Handle(long scheduleId, bool isImmediate)
        {
            try
            {
                await @lock.WaitAsync();

                if (mediaManager.IsPreparedEx())
                {
                    Dispose();
                    return;
                }
                else
                {
                    await Task.Delay(0).ContinueWith((x) =>
                    {
                        if (!firstTime)
                        {
                            UIApplication.SharedApplication.BeginReceivingRemoteControlEvents();
                        }
                        firstTime = false;

                    }, taskScheduler);
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
                        throw;
                    }
                });

            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened when creating the task to ring the alarm.");
                Dispose();
            }
            finally
            {
                @lock.Release();
            }
        }

        private void stateChanged(object sender, bool disposeMediaManager)
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


        public void Dispose()
        {
            dispose(false);
        }

        private bool disposed = false;
        private void dispose(bool disposeMediaManager)
        {
            if (!disposed)
            {
                disposed = true;
                playbackService.Stopped -= stateChanged;
                playbackService.Dispose();
         
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

                    Task.Delay(0).ContinueWith((x) =>
                    {
                        UIApplication.SharedApplication.EndReceivingRemoteControlEvents();

                    }, taskScheduler);
                }
            }
        }
    }
}