using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IMediaCacheService
    {
        Task<bool> Exists(string url);
        string GetCacheFileName(string url);
        string GetCacheFilePath(string url);

        Task SetupAlarmCache(long alarmScheduleId);
        Task CleanUp();
    }
}
