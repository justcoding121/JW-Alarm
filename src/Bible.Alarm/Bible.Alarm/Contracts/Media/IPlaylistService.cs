using Bible.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Contracts
{
    public interface IPlaylistService
    {
        Task MarkTrackAsPlayed(NotificationDetail trackDetail);
        Task MarkTrackAsFinished(NotificationDetail trackDetail);
        Task<List<PlayItem>> NextTracks(long scheduleId);
    }
}
