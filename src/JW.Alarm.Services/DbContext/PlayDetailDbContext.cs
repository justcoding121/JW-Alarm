using JW.Alarm.Services.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using JW.Alarm.Models;
using JW.Alarm.Common.DataStructures;

namespace JW.Alarm.Services
{
    public class PlayDetailDbContext : IPlayDetailDbContext
    {
        private readonly IDatabase database;
        private ObservableDictionary<int, PlayDetail> schedules;

        public PlayDetailDbContext(IDatabase database)
        {
            this.database = database;
        }

        public Task<ObservableDictionary<int, PlayDetail>> PlayDetails => getPlayDetails();

        public int RandomScheduleId => (PlayDetails.Result).First().Key;

        public virtual async Task Create(PlayDetail bibleReadingSchedule)
        {
            await database.Insert(bibleReadingSchedule);

            if (schedules != null)
            {
                schedules.Add(bibleReadingSchedule.Id, bibleReadingSchedule);
            }

        }

        public virtual async Task Delete(int bibleReadingScheduleId)
        {
            await database.Delete<PlayDetail>(bibleReadingScheduleId);

            if (schedules != null)
            {
                schedules.Remove(bibleReadingScheduleId);
            }
        }

        public async Task<PlayDetail> Read(int bibleReadingScheduleId)
        {
            if (schedules != null)
            {
                return schedules[bibleReadingScheduleId];
            }

            return await database.Read<PlayDetail>(bibleReadingScheduleId);
        }

        public virtual async Task Update(PlayDetail bibleReadingSchedule)
        {
            await database.Update(bibleReadingSchedule);

            if (schedules != null)
            {
                schedules[bibleReadingSchedule.Id] = bibleReadingSchedule;
            }
        }

        private async Task<ObservableDictionary<int, PlayDetail>> getPlayDetails()
        {
            if (schedules == null)
            {
                schedules = (await database.ReadAll<PlayDetail>()).ToObservableDictionary(x => x.Id, x => x);
            }

            return schedules;
        }

    }
}
