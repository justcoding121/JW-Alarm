using MediaManager.Playback;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IPlaybackService
    {
        Task Play(long scheduleId);
        void Dismiss();
        Task Snooze();
        event EventHandler<MediaPlayerState> StateChanged;
    }
}
