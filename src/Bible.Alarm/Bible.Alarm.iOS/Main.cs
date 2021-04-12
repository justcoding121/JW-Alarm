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
        private static readonly Lazy<Logger> lazyLogger = new Lazy<Logger>(() => LogManager.GetCurrentClassLogger());
        private static Logger logger => lazyLogger.Value;

        static Application()
        {
            LogSetup.Initialize(VersionFinder.Default, new string[] { }, Xamarin.Forms.Device.iOS);

            AppDomain.CurrentDomain.UnhandledException += unhandledExceptionHandler;
            TaskScheduler.UnobservedTaskException += unobserverdTaskException;
        }

        private static void unobserverdTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            logger.Error("Unobserved task exception.", e.Exception);
        }

        private static void unhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Error("Unhandled exception.", e);
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

        private static bool disposed = false;
        private static void dispose()
        {
            if (disposed)
            {
                return;
            }

            AppDomain.CurrentDomain.UnhandledException -= unhandledExceptionHandler;
            TaskScheduler.UnobservedTaskException -= unobserverdTaskException;

            disposed = true;
        }

        ~Application()
        {
            dispose();
        }
    }
}
