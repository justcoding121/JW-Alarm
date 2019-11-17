using Bible.Alarm.Services.Contracts;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bible.Alarm.Models;
using Bible.Alarm.Common.DataStructures;
using Microsoft.EntityFrameworkCore;

namespace Bible.Alarm.Services
{
    public class MediaDbContext : DbContext
    {
        public MediaDbContext() { }

        public MediaDbContext(DbContextOptions<MediaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Language> Languages { get; set; }

        public DbSet<BibleTranslation> BibleTranslations { get; set; }

        public DbSet<MelodyMusic> MelodyMusic { get; set; }
        public DbSet<VocalMusic> VocalMusic { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            //only for seed migration
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=mediaIndex.db");
            }
#endif
        }

    }
}
