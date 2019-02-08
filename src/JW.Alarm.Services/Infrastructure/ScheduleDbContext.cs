using JW.Alarm.Services.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using JW.Alarm.Models;
using JW.Alarm.Common.DataStructures;
using Microsoft.EntityFrameworkCore;

namespace JW.Alarm.Services
{
    public class ScheduleDbContext : DbContext
    {
        public ScheduleDbContext() { }

        public ScheduleDbContext(DbContextOptions<ScheduleDbContext> options)
            : base(options)
        {
        }

        public DbSet<AlarmSchedule> AlarmSchedules { get; set; }

#if DEBUG
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=bibleAlarm.db");
        }
#endif
    }
}
