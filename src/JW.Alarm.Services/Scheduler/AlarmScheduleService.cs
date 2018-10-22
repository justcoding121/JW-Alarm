using JW.Alarm.Services.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using JW.Alarm.Models;
using JW.Alarm.Common.DataStructures;

namespace JW.Alarm.Services
{
    public abstract class AlarmScheduleService : IAlarmScheduleService
    {
        private readonly IDatabase database;
        private ObservableDictionary<int, AlarmSchedule> schedules;

        public AlarmScheduleService(IDatabase database)
        {
            this.database = database;
        }

        public Task<ObservableDictionary<int, AlarmSchedule>> AlarmSchedules => getAlarmSchedules();

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

        private async Task<ObservableDictionary<int, AlarmSchedule>> getAlarmSchedules()
        {
            if (schedules == null)
            {
                schedules = (await database.ReadAll<AlarmSchedule>()).ToObservableDictionary(x => x.Id, x => x);
            }

            return schedules;
        }

    }
}
