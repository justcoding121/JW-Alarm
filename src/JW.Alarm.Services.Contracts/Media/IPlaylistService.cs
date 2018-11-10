using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IPlaylistService
    {
        Task<PlayItem> NextTrack(int scheduleId);
        Task<PlayItem> NextTrack(PlayDetail currentTrack);
        Task SetFinishedTrack(PlayDetail trackDetail);
        Task<List<string>> Playlist(int scheduleId, TimeSpan duration);
    }
}
