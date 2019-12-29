using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.iOS.Helpers
{
    public class BootstrapHelper
    {
        public async static Task VerifyMediaLookUpService(IContainer container)
        {
            using (var service = container.Resolve<MediaIndexService>())
            {
                await service.Verify();
            }

        }

        public static void VerifyBackgroundTasks()
        {

        }

        public static async Task InitializeDatabase(IContainer container)
        {
            using (var db = container.Resolve<ScheduleDbContext>())
            {
                await db.Database.MigrateAsync();
            }
        }


    }
}
