using Bible.Alarm.Services.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bible.Alarm.Models;
using Bible.Alarm.Common.DataStructures;
using Microsoft.EntityFrameworkCore;

namespace Bible.Alarm.Services
{
    public class ScheduleDbContext : DbContext
    {
        public ScheduleDbContext() { }

        public ScheduleDbContext(DbContextOptions<ScheduleDbContext> options)
            : base(options)
        {
        }

        public DbSet<AlarmSchedule> AlarmSchedules { get; set; }
        public DbSet<AlarmMusic> AlarmMusic { get; set; }
        public DbSet<BibleReadingSchedule> BibleReadingSchedules { get; set; }
        public DbSet<GeneralSettings> GeneralSettings { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
#if DEBUG
            //only for seed migration
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=bibleAlarm.db");
            }
#endif
        }
    }
}
