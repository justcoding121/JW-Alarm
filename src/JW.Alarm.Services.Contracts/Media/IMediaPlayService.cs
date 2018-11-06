using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IMediaPlayService
    {
        Task<List<PlayItem>> ItemsToPlay(int scheduleId, TimeSpan duration);
        Task<PlayItem> NextUrlToPlay(int scheduleId, PlayType playType);
        Task SetNextItemToPlay(int scheduleId, PlayType currentPublication);
    }
}
