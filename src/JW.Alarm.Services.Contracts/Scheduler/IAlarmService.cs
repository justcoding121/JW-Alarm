using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IAlarmService
    {
        Task Create(AlarmSchedule schedule);
        Task ScheduleNextTrack(AlarmSchedule schedule, NotificationDetail detail);
        Task Update(AlarmSchedule schedule);
        Task Delete(long scheduleId);
      
    }
}
