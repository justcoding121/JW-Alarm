using System.Collections.Generic;
using System.Threading.Tasks;
using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;

namespace JW.Alarm.Services.Contracts
{
    public interface IScheduleRepository
    {
        Task<IEnumerable<AlarmSchedule>> AlarmSchedules { get; }

        Task<AlarmSchedule> Read(long alarmScheduleId);
        Task Add(AlarmSchedule alarmSchedule);
        Task Update(AlarmSchedule alarmSchedule);
        Task Remove(long alarmScheduleId);
    }
}
