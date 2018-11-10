using JW.Alarm.Services.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using JW.Alarm.Models;
using JW.Alarm.Common.DataStructures;

namespace JW.Alarm.Services
{
    public class ScheduleDbContext : IScheduleDbContext
    {
        private readonly IDatabase database;
        private readonly IBibleReadingDbContext bibleReadingScheduleService;

        private ObservableDictionary<long, AlarmSchedule> schedules;

        public ScheduleDbContext(IDatabase database,
            IBibleReadingDbContext bibleReadingScheduleService)
        {
            this.database = database;
            this.bibleReadingScheduleService = bibleReadingScheduleService;
        }

        public Task<ObservableDictionary<long, AlarmSchedule>> AlarmSchedules => getAlarmSchedules();

        public async Task Add(AlarmSchedule alarmSchedule)
        {
            if (alarmSchedule.BibleReadingScheduleId == 0)
            {
                var newSchedule = new BibleReadingSchedule()
                {
                    BookNumber = 23,
                    ChapterNumber = 1,
                    LanguageCode = "E",
                    PublicationCode = "NWT"
                };

                await bibleReadingScheduleService.Add(newSchedule);

                alarmSchedule.BibleReadingScheduleId = newSchedule.Id;
            }

            if (alarmSchedule.Music == null)
            {
                alarmSchedule.Music = new AlarmMusic()
                {
                    MusicType = MusicType.Melodies,
                    PublicationCode = "iam",
                    LanguageCode = "E",
                    TrackNumber = 89
                };
            }

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
            await bibleReadingScheduleService.Remove(schedule.BibleReadingScheduleId);

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

        private async Task<ObservableDictionary<long, AlarmSchedule>> getAlarmSchedules()
        {
            if (schedules == null)
            {
                schedules = (await database.ReadAll<AlarmSchedule>()).ToObservableDictionary(x => x.Id, x => x);
            }

            return schedules;
        }

    }
}
