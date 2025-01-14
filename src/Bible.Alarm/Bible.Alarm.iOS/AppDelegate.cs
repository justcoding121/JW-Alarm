﻿using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.iOS.Extensions;
using Bible.Alarm.iOS.Services.Handlers;
using Bible.Alarm.iOS.Services.Platform;
using Bible.Alarm.Services;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.Services.iOS.Helpers;
using Bible.Alarm.Services.Tasks;
using Foundation;
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
        private static readonly Lazy<Logger> lazyLogger = new Lazy<Logger>(() => LogManager.GetCurrentClassLogger());
        private static Logger logger => lazyLogger.Value;

        private readonly IContainer container;

        public AppDelegate()
        {
            LogSetup.Initialize(VersionFinder.Default, new string[] { }, Xamarin.Forms.Device.iOS);

            AppDomain.CurrentDomain.UnhandledException += unhandledExceptionHandler;
            TaskScheduler.UnobservedTaskException += unobserverdTaskException;

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

        private void unobserverdTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            logger.Error(e.Exception, "Unobserved task exception.");
        }

        private void unhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
              logger.Error("Unhandled exception.", e.SerializeObject());
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
                //once every hour
                UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(60 * 60);

                global::Xamarin.Forms.Forms.Init();
                LoadApplication(new App(container));
            }
            catch (Exception e)
            {
                logger.Fatal(e, "iOS application crashed.");
                throw;
            }

            // check for a notification
            if (launchOptions != null)
            {
                try
                {
                    // check for a local notification
                    if (launchOptions.ContainsKey(UIApplication.LaunchOptionsLocalNotificationKey))
                    {
                        var localNotification = launchOptions[UIApplication.LaunchOptionsLocalNotificationKey] as UILocalNotification;
                        if (localNotification != null)
                        {
                            handleNotification(localNotification.UserInfo);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error handling iOS notification on launch.");
                }
            }

            // Request notification permissions from the user
            UNUserNotificationCenter.Current.RequestAuthorization(
                UNAuthorizationOptions.Alert
                | UNAuthorizationOptions.Sound
                | UNAuthorizationOptions.Badge, (approved, err) =>
            {
                if (!approved)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            using var dbContext = container.Resolve<ScheduleDbContext>();

                            if (!dbContext.GeneralSettings.Any(x => x.Key == "iOSNotificationDisabledMsgShown"))
                            {
                                dbContext.GeneralSettings.Add(new Alarm.Models.GeneralSettings()
                                {
                                    Key = "iOSNotificationDisabledMsgShown",
                                    Value = "true"
                                });
                                dbContext.SaveChanges();

                                var popupService = container.Resolve<IToastService>();
                                popupService.ShowMessage("You've disabled notifications. " +
                                    "We won't be able to alert you on scheduled time. " +
                                    "You can however open the app anytime and resume listening.", 8);
                            }
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Error when prompting iOS notification permission on launch.");
                        }
                    });

                }
            });


            return base.FinishedLaunching(app, launchOptions);
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            try
            {
                var delivered = UNUserNotificationCenter.Current.GetDeliveredNotificationsAsync().Result;

                if (delivered != null)
                {
                    var notification = delivered.FirstOrDefault();

                    if (notification != null)
                    {
                        handleNotification(notification.Request.Content.UserInfo);
                    }

                    UNUserNotificationCenter.Current.RemoveAllDeliveredNotifications();
                }

                UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;

            }
            catch (Exception e)
            {
                logger.Error(e, "Error when showing notification on iOS activation.");
            }

            base.OnActivated(uiApplication);
        }

        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            try
            {
                handleNotification(notification.UserInfo);
            }
            catch (Exception e)
            {
                logger.Error(e, "Error when handling iOS notification.");
            }
        }

        private void handleNotification(NSDictionary nsUserInfo)
        {
            var userInfo = nsUserInfo.ToDictionary();
            var scheduleId = userInfo["ScheduleId"];
            // show an alert
            var iosAlarmHandler = container.Resolve<iOSAlarmHandler>();
            _ = iosAlarmHandler.Handle(long.Parse(scheduleId), true);

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

                using var mediaIndexService = container.Resolve<MediaIndexService>();
                downloaded = downloaded || await mediaIndexService.UpdateIndexIfAvailable();
            }
            catch (Exception e)
            {
                logger.Error(e, "An error occurred in doing perform fetch task.");
            }

            // Inform system of fetch results
            completionHandler(downloaded ? UIBackgroundFetchResult.NewData : UIBackgroundFetchResult.NoData);
        }

        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            AppDomain.CurrentDomain.UnhandledException -= unhandledExceptionHandler;
            TaskScheduler.UnobservedTaskException -= unobserverdTaskException;

            disposed = true;

            base.Dispose(disposing);
        }
    }
}
