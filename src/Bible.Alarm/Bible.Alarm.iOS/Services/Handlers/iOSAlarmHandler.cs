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
        }

        public async Task HandleNotification(long notificationId)
        {
            var notification = await dbContext.AlarmNotifications
                                .Include(x => x.AlarmSchedule)
                                .FirstOrDefaultAsync(x => x.Id == notificationId);

            var utcNow = DateTime.UtcNow;

            if (notification != null
                   && !notification.CancellationRequested)
            {
                await Handle(notification.AlarmScheduleId);
            }
            else
            {
                Dispose();
            }
        }

        public async Task Handle(long scheduleId)
        {
            var isBusy = false;

            try
            {
                await @lock.WaitAsync();

                if (mediaManager.IsPreparedEx())
                {
                    isBusy = true;
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
                        mediaManager.Init();

                    }, taskScheduler);      
                }

                playbackService.Stopped += stateChanged;

                await Task.Run(async () =>
                {
                    try
                    {
                        await playbackService.Play(scheduleId);
                        Messenger<object>.Publish(MvvmMessages.ShowAlarmModal);
                       
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
                Dispose();
            }
            finally
            {
                @lock.Release();
            }

            if (isBusy)
            {
                Dispose();
            }
        }

        private void stateChanged(object sender, MediaPlayerState e)
        {
            try
            {
                if (e == MediaPlayerState.Stopped)
                {
                    playbackService.Stopped -= stateChanged;
                    Dispose();
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
            Task.Delay(0).ContinueWith((x) =>
                   {
                       UIApplication.SharedApplication.EndReceivingRemoteControlEvents();
                       this.mediaManager.Dispose();
                   }, taskScheduler);
      
        }

    }
}