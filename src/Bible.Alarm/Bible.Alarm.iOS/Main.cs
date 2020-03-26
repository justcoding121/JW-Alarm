using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.iOS.Services.Platform;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.Services.iOS.Helpers;
using Foundation;
using MediaManager;
using NLog;
using System;
using System.Threading.Tasks;
using UIKit;

[assembly: Preserve(typeof(System.Linq.Queryable), AllMembers = true)]
namespace Bible.Alarm.iOS
{
    public class Application
    {
        private static Logger logger;

        public Application()
        {
            LogSetup.Initialize(VersionFinder.Default, new string[] { });
            logger = LogManager.GetCurrentClassLogger();
        }

        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            var result = IocSetup.Initialize("SplashActivity", false);
            var container = result.Item1;
            var containerCreated = result.Item2;
            if (containerCreated)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        SQLitePCL.Batteries_V2.Init();
                        var task1 = BootstrapHelper.VerifyMediaLookUpService(container);
                        var task2 = BootstrapHelper.InitializeDatabase(container);

                        await Task.WhenAll(task1, task2);
                        await Messenger<bool>.Publish(MvvmMessages.Initialized, true);
                        await Task.Delay(1000);
                    }
                    catch (Exception e)
                    {
                        logger.Fatal(e, "iOS initialization crashed.");
                    }
                });

                // if you want to use a different Application Delegate class from "AppDelegate"
                // you can specify it here.
                UIApplication.Main(args, null, "AppDelegate");
            }
            else
            {
                Task.Run(async () =>
                {
                    await Messenger<bool>.Publish(MvvmMessages.Initialized, true);
                });
            }

        }
    }
}
