using Microsoft.EntityFrameworkCore;
using NLog;
using System;
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
        public static async Task InitializeDatabase(IContainer container)
        {
            using (var db = container.Resolve<ScheduleDbContext>())
            {
                await db.Database.MigrateAsync();
            }
        }

        public static Task Initialize(IContainer container, Logger logger)
        {
            return Task.Run(async () =>
            {
                try
                {
                    SQLitePCL.Batteries_V2.Init();
                    var task1 = BootstrapHelper.VerifyMediaLookUpService(container);
                    var task2 = BootstrapHelper.InitializeDatabase(container);

                    await Task.WhenAll(task1, task2);
                }
                catch (Exception e)
                {
                    logger.Fatal(e, "iOS initialization crashed.");
                }
            });
        }
    }
}
