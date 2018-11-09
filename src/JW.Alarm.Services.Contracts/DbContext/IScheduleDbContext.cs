using System.Collections.Generic;
using System.Threading.Tasks;
using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;

namespace JW.Alarm.Services.Contracts
{
    public interface IScheduleDbContext
    {
        Task<ObservableDictionary<int, AlarmSchedule>> AlarmSchedules { get; }

        Task<AlarmSchedule> Read(int alarmScheduleId);
        Task Create(AlarmSchedule alarmSchedule);
        Task Update(AlarmSchedule alarmSchedule);
        Task Delete(int alarmScheduleId);
    }
}
