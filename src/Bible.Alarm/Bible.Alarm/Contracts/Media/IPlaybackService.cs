using MediaManager.Playback;
using MediaManager.Player;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Contracts
{
    public interface IPlaybackService
    {
        Task Play(long scheduleId);
        void Dismiss();
        Task Snooze();
        event EventHandler<MediaPlayerState> StateChanged;
    }
}
