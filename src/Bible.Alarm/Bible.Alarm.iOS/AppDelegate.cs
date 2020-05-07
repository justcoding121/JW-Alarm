using Bible.Alarm.iOS.Extensions;
using Bible.Alarm.iOS.Services.Handlers;
using Bible.Alarm.iOS.Services.Platform;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.Services.iOS.Helpers;
using Bible.Alarm.Services.Tasks;
using Foundation;
using MediaManager;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;
using UIKit;
using UserNotifications;

namespace Bible.Alarm.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate,
                                        IUNUserNotificationCenterDelegate
    {
        private Logger logger => LogManager.GetCurrentClassLogger();
        private readonly IContainer container;

        public AppDelegate()
        {
            LogSetup.Initialize(VersionFinder.Default, new string[] { });

            container = IocSetup.GetContainer("SplashActivity");

            try
            {
                if (container == null)
                {
                    var result = IocSetup.Initialize("SplashActivity", false);
                    container = result.Item1;
                    var containerCreated = result.Item2;
                    if (containerCreated)
                    {
                        BootstrapHelper.Initialize(container, logger);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Fatal(e, "AppDelegate initialization failed.");
                throw;
            }
        }

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary launchOptions)
        {
#if DEBUG
            System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
            {
                if (certificate.Issuer.Equals("CN=localhost"))
                    return true;
                return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
            };
#endif

            try
            {
                UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);
                global::Xamarin.Forms.Forms.Init();
                LoadApplication(new App(container));
            }
            catch (Exception e)
            {
                logger.Fatal(e, "iOS application crashed.");
                throw;
            }

            Task.Run(async () =>
            {
                try
                {
                    using var schedulerTask = container.Resolve<SchedulerTask>();
                    var downloaded = await schedulerTask.Handle();
                }
                catch (Exception e)
                {
                    logger.Error(e, "An error occurred in cleanup task.");
                }
            });

            // check for a notification

            if (launchOptions != null)
            {
                // check for a local notification
                if (launchOptions.ContainsKey(UIApplication.LaunchOptionsLocalNotificationKey))
                {
                    var localNotification = launchOptions[UIApplication.LaunchOptionsLocalNotificationKey] as UILocalNotification;
                    if (localNotification != null)
                    {
                        handleNotification(localNotification);
                    }
                }
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var notificationSettings = UIUserNotificationSettings.GetSettingsForTypes(
                    UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, null
                );

                app.RegisterUserNotificationSettings(notificationSettings);
            }

            return base.FinishedLaunching(app, launchOptions);
        }

        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            handleNotification(notification);
        }

        private void handleNotification(UILocalNotification notification)
        {
            var userInfo = notification.UserInfo.ToDictionary();
            var scheduleId = userInfo["ScheduleId"];
            // show an alert
            var iosAlarmHandler = container.Resolve<iOSAlarmHandler>();
            _ = iosAlarmHandler.Handle(long.Parse(scheduleId));

            // reset our badge
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
        }

        public async override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            bool downloaded = false;

            try
            {
                using var schedulerTask = container.Resolve<SchedulerTask>();
                downloaded = await schedulerTask.Handle();
            }
            catch (Exception e)
            {
                logger.Error(e, "An error occurred in cleanup task.");
            }

            // Inform system of fetch results
            completionHandler(downloaded ? UIBackgroundFetchResult.NewData : UIBackgroundFetchResult.NoData);
        }
    }
}
