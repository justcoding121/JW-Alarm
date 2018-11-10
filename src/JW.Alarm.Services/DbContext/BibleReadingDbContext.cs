using JW.Alarm.Services.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using JW.Alarm.Models;
using JW.Alarm.Common.DataStructures;

namespace JW.Alarm.Services
{
    public class BibleReadingDbContext : IBibleReadingDbContext
    {
        private readonly IDatabase database;
        private ObservableDictionary<long, BibleReadingSchedule> schedules;

        public BibleReadingDbContext(IDatabase database)
        {
            this.database = database;
        }

        public Task<ObservableDictionary<long, BibleReadingSchedule>> BibleReadingSchedules => getBibleReadingSchedules();

        public virtual async Task Add(BibleReadingSchedule bibleReadingSchedule)
        {
            await database.Insert(bibleReadingSchedule);

            if (schedules != null)
            {
                schedules.Add(bibleReadingSchedule.Id, bibleReadingSchedule);
            }

        }

        public virtual async Task Remove(long bibleReadingScheduleId)
        {
            await database.Delete<BibleReadingSchedule>(bibleReadingScheduleId);

            if (schedules != null)
            {
                schedules.Remove(bibleReadingScheduleId);
            }
        }

        public async Task<BibleReadingSchedule> Read(long bibleReadingScheduleId)
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

        private async Task<ObservableDictionary<long, BibleReadingSchedule>> getBibleReadingSchedules()
        {
            if (schedules == null)
            {
                schedules = (await database.ReadAll<BibleReadingSchedule>()).ToObservableDictionary(x => x.Id, x => x);
            }

            return schedules;
        }

    }
}
