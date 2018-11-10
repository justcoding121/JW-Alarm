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
        Task<ObservableDictionary<long, BibleReadingSchedule>> BibleReadingSchedules { get; }

        Task<BibleReadingSchedule> Read(long bibleReadingScheduleId);
        Task Add(BibleReadingSchedule bibleReadingSchedule);
        Task Update(BibleReadingSchedule bibleReadingSchedule);
        Task Remove(long bibleReadingScheduleId);
    }
}
