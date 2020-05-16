using Bible.Alarm.Common.Mvvm;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Bible.Alarm.Services.Uwp.Helpers
{
    public class BootstrapHelper
    {
        public static bool IsBackgroundTaskEnabled = true;
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

        public static async Task SetupBackgroundTask(IContainer container)
        {
            await BackgroundExecutionManager.RequestAccessAsync();
            var allowed = BackgroundExecutionManager.GetAccessStatus();
            switch (allowed)
            {
                case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:
                case BackgroundAccessStatus.AlwaysAllowed:
                    break;
                case BackgroundAccessStatus.Unspecified:
                case BackgroundAccessStatus.DeniedBySystemPolicy:
                case BackgroundAccessStatus.DeniedByUser:
                    IsBackgroundTaskEnabled = false;
                    break;
            }

            var registered = false;

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == "SchedulerTask")
                {
                    registered = true;
                }
            }

            if (!registered)
            {
                var builder = new BackgroundTaskBuilder
                {
                    Name = "SchedulerTask"
                };

                builder.SetTrigger(new TimeTrigger(15, true));
                builder.Register();
            }
        }

        public static Task Initialize(IContainer container, Logger logger)
        {
            Task.Run(() => SetupBackgroundTask(container));

            return Task.Run(async () =>
            {
                try
                {
                    var task1 = BootstrapHelper.VerifyMediaLookUpService(container);
                    var task2 = BootstrapHelper.InitializeDatabase(container);
                    
                    await Task.WhenAll(task1, task2);

                    Messenger<bool>.Publish(MvvmMessages.Initialized, true);
                }
                catch (Exception e)
                {
                    logger.Fatal(e, "Uwp initialization crashed.");
                }
            });
        }
    }
}
