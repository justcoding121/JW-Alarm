using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services.Tasks;
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

        public static void Initialize(IContainer container, Logger logger)
        {
            Task.Run(async () =>
            {
                try
                {
                    SQLitePCL.Batteries_V2.Init();
                    var task1 = BootstrapHelper.VerifyMediaLookUpService(container);
                    var task2 = BootstrapHelper.InitializeDatabase(container);

                    await Task.WhenAll(task1, task2);

                    Messenger<bool>.Publish(MvvmMessages.Initialized, true);

                    await Task.Delay(1000);

                    try
                    {
                        using var schedulerTask = container.Resolve<SchedulerTask>();
                        var downloaded = await schedulerTask.Handle();
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "An error occurred in cleanup task.");
                    }

                }
                catch (Exception e)
                {
                    logger.Fatal(e, "iOS initialization crashed.");
                }
            });
        }
    }
}
