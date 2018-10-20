using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IMediaPlayService
    {
        Task Play(int scheduleId);
        Task Stop(AlarmSchedule schedule);
        Task SetNextItemToPlay(int scheduleId);
        Task UpdatePlayedSeconds(AlarmSchedule schedule, int second);
        Task<PlayItem> NextUrlToPlay(int scheduleId);
    }
}
