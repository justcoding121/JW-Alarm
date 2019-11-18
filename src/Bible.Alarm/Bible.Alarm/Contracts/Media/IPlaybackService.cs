using MediaManager.Playback;
using MediaManager.Player;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Contracts
{
    public interface IPlaybackService : IDisposable
    {
        long CurrentlyPlayingScheduleId { get; }

        Task Play(long scheduleId);
        Task Dismiss();
        Task Snooze();
        event EventHandler<MediaPlayerState> Stopped;
    }
}
