using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IPlaylistService
    {
        Task<PlayItem> NextTrack(long scheduleId);
        Task<PlayItem> NextTrack(NotificationDetail currentTrack);
        Task SetFinishedTrack(NotificationDetail trackDetail);
        Task<List<string>> Playlist(long scheduleId, TimeSpan duration);
    }
}
