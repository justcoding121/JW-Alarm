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
        private readonly IBibleReadingScheduleService bibleReadingScheduleService;

        private ObservableDictionary<int, AlarmSchedule> schedules;

        public AlarmScheduleService(IDatabase database,
            IBibleReadingScheduleService bibleReadingScheduleService)
        {
            this.database = database;
            this.bibleReadingScheduleService = bibleReadingScheduleService;
        }

        public Task<ObservableDictionary<int, AlarmSchedule>> AlarmSchedules => getAlarmSchedules();

        public virtual async Task Create(AlarmSchedule alarmSchedule)
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

                await bibleReadingScheduleService.Create(newSchedule);

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

        public async Task<AlarmSchedule> Read(int alarmScheduleId)
        {
            if (schedules != null)
            {
                return schedules.GetValue(alarmScheduleId);
            }

            return await database.Read<AlarmSchedule>(alarmScheduleId);
        }

        public virtual async Task Delete(int alarmScheduleId)
        {
            var schedule = await Read(alarmScheduleId);

            await database.Delete<AlarmSchedule>(schedule.Id);
            await bibleReadingScheduleService.Delete(schedule.BibleReadingScheduleId);

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
                schedules.SetValue(alarmSchedule.Id,  alarmSchedule);
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
