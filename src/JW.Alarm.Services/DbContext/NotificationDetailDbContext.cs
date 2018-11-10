using JW.Alarm.Services.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using JW.Alarm.Models;
using JW.Alarm.Common.DataStructures;

namespace JW.Alarm.Services
{
    public class NotificationDetailDbContext : INotificationDetailDbContext
    {
        private readonly IDatabase database;
        private ObservableDictionary<long, NotificationDetail> schedules;

        public NotificationDetailDbContext(IDatabase database)
        {
            this.database = database;
        }

        public Task<ObservableDictionary<long, NotificationDetail>> PlayDetails => getPlayDetails();

        public virtual async Task Add(NotificationDetail bibleReadingSchedule)
        {
            await database.Insert(bibleReadingSchedule);

            if (schedules != null)
            {
                schedules.Add(bibleReadingSchedule.Id, bibleReadingSchedule);
            }

        }

        public virtual async Task Remove(long bibleReadingScheduleId)
        {
            await database.Delete<NotificationDetail>(bibleReadingScheduleId);

            if (schedules != null)
            {
                schedules.Remove(bibleReadingScheduleId);
            }
        }

        public async Task<NotificationDetail> Read(long bibleReadingScheduleId)
        {
            if (schedules != null)
            {
                return schedules[bibleReadingScheduleId];
            }

            return await database.Read<NotificationDetail>(bibleReadingScheduleId);
        }

        public virtual async Task Update(NotificationDetail bibleReadingSchedule)
        {
            await database.Update(bibleReadingSchedule);

            if (schedules != null)
            {
                schedules[bibleReadingSchedule.Id] = bibleReadingSchedule;
            }
        }

        private async Task<ObservableDictionary<long, NotificationDetail>> getPlayDetails()
        {
            if (schedules == null)
            {
                schedules = (await database.ReadAll<NotificationDetail>()).ToObservableDictionary(x => x.Id, x => x);
            }

            return schedules;
        }

    }
}
