using Bible.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Contracts
{
    public interface IAlarmService : IDisposable
    {
        Task Create(AlarmSchedule schedule, bool downloadAlarmMedia = false);
        void Update(AlarmSchedule schedule);
        void Delete(long scheduleId);

        Task Snooze(long scheduleId);
    }
}
