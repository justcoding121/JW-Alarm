using JW.Alarm.Services.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using JW.Alarm.Models;
using JW.Alarm.Common.DataStructures;
using Microsoft.EntityFrameworkCore;

namespace JW.Alarm.Services
{
    public class MediaDbContext : DbContext
    {
        public MediaDbContext() { }

        public MediaDbContext(DbContextOptions<ScheduleDbContext> options)
            : base(options)
        {
        }

        public DbSet<Language> Languages { get; set; }
        public DbSet<BibleTranslation> BibleTranslations { get; set; }
        public DbSet<SongBook> SongBooks { get; set; }

#if DEBUG
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=mediaIndex.db");
        }
#endif
    }
}
