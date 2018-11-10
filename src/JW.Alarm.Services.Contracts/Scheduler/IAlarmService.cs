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
        Task Create(AlarmSchedule schedule, PlayDetail playDetail);
        Task Delete(int scheduleId);
        Task Update(AlarmSchedule schedule);
    }
}
