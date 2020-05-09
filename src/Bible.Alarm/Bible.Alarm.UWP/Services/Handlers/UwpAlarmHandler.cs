using Bible.Alarm.Common.Extensions;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services;
using Bible.Alarm.Services.Contracts;
using MediaManager;
using MediaManager.Platforms.Uap.Player;
using MediaManager.Player;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media;

namespace Bible.Alarm.UWP.Services.Handlers
{
    public class UwpAlarmHandler : IDisposable
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        private IPlaybackService playbackService;
        private IMediaManager mediaManager;
        private ScheduleDbContext dbContext;
        private SystemMediaTransportControls systemMediaTransportControls;

        private static SemaphoreSlim @lock = new SemaphoreSlim(1);
        public UwpAlarmHandler(IPlaybackService playbackService,
                                IMediaManager mediaManager,
                                ScheduleDbContext dbContext)
        {
            this.playbackService = playbackService;
            this.mediaManager = mediaManager;
            this.dbContext = dbContext;

            var windowsMediaPlayer = mediaManager.MediaPlayer as WindowsMediaPlayer;
            var mediaPlayer = windowsMediaPlayer.Player;

            mediaPlayer.SystemMediaTransportControls.IsEnabled = false;  
        }

        public async Task Handle(long scheduleId, bool isImmediate)
        {
            try
            {
                await @lock.WaitAsync();

                if (mediaManager.IsPreparedEx())
                {
                    playbackService.Dispose();
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
                playbackService.Stopped -= stateChanged;
                Dispose();
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
            playbackService?.Dispose();
            mediaManager?.Dispose();
        }

    }
}