using JW.Alarm.Services.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using JW.Alarm.Models;
using JW.Alarm.Common.DataStructures;

namespace JW.Alarm.Services
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly ITableStorage database;

        private Dictionary<long, AlarmSchedule> schedules;

        public ScheduleRepository(ITableStorage database)
        {
            this.database = database;
        }

        public Task<IEnumerable<AlarmSchedule>> AlarmSchedules => getAlarmSchedules();

        public async Task Add(AlarmSchedule alarmSchedule)
        {
            await database.Insert(alarmSchedule);
            if (schedules != null)
            {
                schedules.Add(alarmSchedule.Id, alarmSchedule);
            }
        }

        public async Task<AlarmSchedule> Read(long alarmScheduleId)
        {
            if (schedules != null)
            {
                return schedules[alarmScheduleId];
            }

            return await database.Read<AlarmSchedule>(alarmScheduleId);
        }

        public async Task Remove(long alarmScheduleId)
        {
            var schedule = await Read(alarmScheduleId);

            await database.Delete<AlarmSchedule>(schedule.Id);

            if (schedules != null)
            {
                schedules.Remove(alarmScheduleId);
            }
        }

        public async Task Update(AlarmSchedule alarmSchedule)
        {
            await database.Update(alarmSchedule);

            if (schedules != null)
            {
                schedules[alarmSchedule.Id] = alarmSchedule;
            }
        }

        private async Task<IEnumerable<AlarmSchedule>> getAlarmSchedules()
        {
            if (schedules == null)
            {
                schedules = (await database.ReadAll<AlarmSchedule>()).ToDictionary(x => x.Id, x => x);
            }

            return schedules.Select(x=>x.Value);
        }

    }
}
