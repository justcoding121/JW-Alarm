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

namespace Bible.Alarm.iOS
{
    public class Application
    {
        private static Logger logger => LogManager.GetCurrentClassLogger();

        static Application()
        {
            LogSetup.Initialize(VersionFinder.Default, new string[] { });
        }

        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            try
            {
                var result = IocSetup.Initialize("SplashActivity", false);
                var container = result.Item1;
                var containerCreated = result.Item2;
                if (containerCreated)
                {
                    BootstrapHelper.Initialize(container, logger);

                    // if you want to use a different Application Delegate class from "AppDelegate"
                    // you can specify it here.
                    UIApplication.Main(args, null, "AppDelegate");

                }
                else
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            Messenger<bool>.Publish(MvvmMessages.Initialized, true);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "An error happened in Messenger Publish call.");
                        }
                    });
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Main initialization failed.");
                throw;
            }
        }
    }
}
