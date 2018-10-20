using System.Collections.Generic;
using System.Threading.Tasks;
using JW.Alarm.Models;

namespace JW.Alarm.Services.Contracts
{
    public interface IScheduleService
    {
        Task<Dictionary<int, AlarmSchedule>> Schedules { get; }
        
        Task Create(AlarmSchedule schedule);
        Task Update(AlarmSchedule schedule);
        Task Delete(int scheduleId);
    }
}
