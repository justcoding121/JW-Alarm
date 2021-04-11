using Bible.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Contracts
{
    public interface IPlaylistService : IDisposable
    {
        Task MarkTrackAsPlayed(NotificationDetail trackDetail);
        Task MarkTrackAsFinished(NotificationDetail trackDetail);
        Task<PlayItem> NextTrack(long scheduleId);
        Task<List<PlayItem>> NextTracks(long scheduleId);
        Task SaveLastPlayed(long currentScheduleId);

        Task<long> GetRelavantScheduleToPlay();
    }
}
