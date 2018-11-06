using JW.Alarm.Services.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using JW.Alarm.Models;
using JW.Alarm.Common.DataStructures;

namespace JW.Alarm.Services
{
    public class BibleReadingScheduleService : IBibleReadingScheduleService
    {
        private readonly IDatabase database;
        private ObservableDictionary<int, BibleReadingSchedule> schedules;

        public BibleReadingScheduleService(IDatabase database)
        {
            this.database = database;
        }

        public Task<ObservableDictionary<int, BibleReadingSchedule>> BibleReadingSchedules => getBibleReadingSchedules();

        public int RandomScheduleId => (BibleReadingSchedules.Result).First().Key;

        public virtual async Task Create(BibleReadingSchedule bibleReadingSchedule)
        {
            await database.Insert(bibleReadingSchedule);

            if (schedules != null)
            {
                schedules.Add(bibleReadingSchedule.Id, bibleReadingSchedule);
            }

        }

        public virtual async Task Delete(int bibleReadingScheduleId)
        {
            await database.Delete<BibleReadingSchedule>(bibleReadingScheduleId);

            if (schedules != null)
            {
                schedules.Remove(bibleReadingScheduleId);
            }
        }

        public async Task<BibleReadingSchedule> Read(int bibleReadingScheduleId)
        {
            if (schedules != null)
            {
                return schedules[bibleReadingScheduleId];
            }

            return await database.Read<BibleReadingSchedule>(bibleReadingScheduleId);
        }

        public virtual async Task Update(BibleReadingSchedule bibleReadingSchedule)
        {
            await database.Update(bibleReadingSchedule);

            if (schedules != null)
            {
                schedules[bibleReadingSchedule.Id] = bibleReadingSchedule;
            }
        }

        private async Task<ObservableDictionary<int, BibleReadingSchedule>> getBibleReadingSchedules()
        {
            if (schedules == null)
            {
                schedules = (await database.ReadAll<BibleReadingSchedule>()).ToObservableDictionary(x => x.Id, x => x);
            }

            return schedules;
        }

    }
}
