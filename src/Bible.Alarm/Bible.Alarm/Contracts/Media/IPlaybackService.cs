using MediaManager.Player;
using System;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Contracts
{
    public interface IPlaybackService 
    {
        long CurrentlyPlayingScheduleId { get; }
        bool IsPlaying { get; }
        bool IsPrepared { get; }
        Task PrepareAndPlay(long scheduleId, bool isImmediate);

        Task Prepare(long scheduleId);
        Task Play();

        Task Dismiss();
        Task PrepareLastPlayed();
    }
}
