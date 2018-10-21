using JW.Alarm.Services.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using JW.Alarm.Models;

namespace JW.Alarm.Services
{
    public abstract class AlarmScheduleService : IAlarmScheduleService
    {
        private readonly IDatabase database;
        private Dictionary<int, AlarmSchedule> schedules;

        public AlarmScheduleService(IDatabase database)
        {
            this.database = database;
        }

        public Task<Dictionary<int, AlarmSchedule>> AlarmSchedules => getAlarmSchedules();

        public virtual async Task Create(AlarmSchedule alarmSchedule)
        {
            await database.Insert(alarmSchedule);

            if (schedules != null)
            {
                schedules.Add(alarmSchedule.Id, alarmSchedule);
            }
        }

        public async Task<AlarmSchedule> Read(int alarmScheduleId)
        {
            if (schedules != null)
            {
                return schedules[alarmScheduleId];
            }

            return await database.Read<AlarmSchedule>(alarmScheduleId);
        }

        public virtual async Task Delete(int alarmScheduleId)
        {
            await database.Delete<AlarmSchedule>(alarmScheduleId);

            if (schedules != null)
            {
                schedules.Remove(alarmScheduleId);
            }
        }

        public virtual async Task Update(AlarmSchedule alarmSchedule)
        {
            await database.Update(alarmSchedule);

            if (schedules != null)
            {
                schedules[alarmSchedule.Id] = alarmSchedule;
            }
        }

        private async Task<Dictionary<int, AlarmSchedule>> getAlarmSchedules()
        {
            if (schedules == null)
            {
                schedules = (await database.ReadAll<AlarmSchedule>()).ToDictionary(x => x.Id, x => x);
            }

            return schedules;
        }

    }
}
