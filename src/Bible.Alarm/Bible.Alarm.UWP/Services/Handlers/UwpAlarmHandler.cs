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

namespace Bible.Alarm.Uwp.Services.Handlers
{
    public class UwpAlarmHandler : IDisposable
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        private IPlaybackService playbackService;
        private IMediaManager mediaManager;
        private ScheduleDbContext dbContext;

        private static SemaphoreSlim @lock = new SemaphoreSlim(1);

        public UwpAlarmHandler(IPlaybackService playbackService,
                                IMediaManager mediaManager,
                                ScheduleDbContext dbContext)
        {
            this.playbackService = playbackService;
            this.mediaManager = mediaManager;
            this.dbContext = dbContext;
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

                if (mediaManager.IsPrepared())
                {
                    isBusy = true;
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
                mediaManager?.Dispose();
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