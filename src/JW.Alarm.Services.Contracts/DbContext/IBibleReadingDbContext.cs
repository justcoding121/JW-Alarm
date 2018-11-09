using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Contracts
{
    public interface IBibleReadingDbContext
    {
        int RandomScheduleId { get; }

        Task<ObservableDictionary<int, BibleReadingSchedule>> BibleReadingSchedules { get; }

        Task<BibleReadingSchedule> Read(int bibleReadingScheduleId);
        Task Create(BibleReadingSchedule bibleReadingSchedule);
        Task Update(BibleReadingSchedule bibleReadingSchedule);
        Task Delete(int bibleReadingScheduleId);
    }
}
