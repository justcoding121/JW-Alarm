using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IMediaCacheService
    {
        Task<bool> Exists(string url);
        string GetCacheKey(string url);
        string GetCacheUrl(string url);

        Task SetupAlarmCache(int alarmScheduleId);
    }
}
