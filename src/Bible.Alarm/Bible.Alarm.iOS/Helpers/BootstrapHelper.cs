using Bible.Alarm.Common.Helpers;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services.Tasks;
using NLog;
using System;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.iOS.Helpers
{
    public class BootstrapHelper
    {
        public static void Initialize(IContainer container, Logger logger)
        {
            Task.Run(async () =>
            {
                try
                {
                    SQLitePCL.Batteries_V2.Init();
                    await CommonBootstrapHelper.VerifyServices(container);

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
