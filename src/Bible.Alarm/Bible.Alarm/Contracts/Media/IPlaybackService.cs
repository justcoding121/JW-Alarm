using MediaManager.Player;
using System;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Contracts
{
    public interface IPlaybackService : IDisposable
    {
        long CurrentlyPlayingScheduleId { get; }

        Task Play(long scheduleId, bool isImmediate);
        Task Dismiss();
        Task Snooze();
        event EventHandler<MediaPlayerState> Stopped;
    }
}
