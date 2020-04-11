﻿using Bible.Alarm.Common.Helpers;
using Bible.Alarm.iOS.Helpers;
using Bible.Alarm.iOS.Models;
using Bible.Alarm.iOS.Services.Handlers;
using Bible.Alarm.iOS.Services.Platform;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.Services.iOS.Helpers;
using Foundation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
                    var result = IocSetup.Initialize("SplashActivity", true);
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
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
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
                global::Xamarin.Forms.Forms.Init();
                LoadApplication(new App(container));


                UIApplication.SharedApplication.RegisterForRemoteNotifications();
                UNUserNotificationCenter.Current.Delegate = this;

            }
            catch (Exception e)
            {
                logger.Fatal(e, "iOS application crashed.");
                throw;
            }

            return base.FinishedLaunching(app, options);
        }

        public async override void RegisteredForRemoteNotifications(
            UIApplication application, NSData deviceToken)
        {
            try
            {
                var deviceId = NSUserDefaults.StandardUserDefaults.StringForKey("DeviceId");

                if (string.IsNullOrEmpty(deviceId))
                {
                    deviceId = Guid.NewGuid().ToString();
                    NSUserDefaults.StandardUserDefaults.SetString(deviceId, "DeviceId");
                }

                // Get current device token
                var currentDeviceToken = deviceToken.Description;
                if (!string.IsNullOrWhiteSpace(currentDeviceToken))
                {
                    currentDeviceToken = currentDeviceToken.Trim('<').Trim('>');
                }

#if DEBUG
                NSUserDefaults.StandardUserDefaults.SetString(string.Empty, "PushDeviceToken");
#endif
                // Get previous device token
                var oldDeviceToken = NSUserDefaults.StandardUserDefaults.StringForKey("PushDeviceToken");

                // Has the token changed?
                if (string.IsNullOrEmpty(oldDeviceToken)
                    || !oldDeviceToken.Equals(currentDeviceToken))
                {
                    var result = await PnsService.RegisterDevice(deviceId, currentDeviceToken);

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        // Save new device token
                        NSUserDefaults.StandardUserDefaults.SetString(currentDeviceToken, "PushDeviceToken");
                    }

                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Error happenned inside RegisteredForRemoteNotifications.");
            }
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            logger.Error($"Error registering push notifications. Description: {error.LocalizedDescription}");
        }

        public async override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            base.ReceivedRemoteNotification(application, userInfo);

            await handleNotification(userInfo);
        }

        [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            var schuler = TaskScheduler.FromCurrentSynchronizationContext();
            handleNotification(userInfo).ContinueWith((x) =>
            {
              completionHandler(UIBackgroundFetchResult.NewData);
            }, schuler);
        }

        private async Task handleNotification(NSDictionary userInfo)
        {
            try
            {
                //cancel local notifications here
                var notificationId = getNotificationId(userInfo);

                if (notificationId >= 0)
                {
                    var handler = container.Resolve<iOSAlarmHandler>();
                    await handler.HandleNotification(notificationId);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to handle remote notification while on foreground.");
            }
        }

        private long getNotificationId(NSDictionary userInfo)
        {
            if (null != userInfo && userInfo.ContainsKey(new NSString("notificationId")))
            {
                var s = userInfo.ValueForKey(new NSString("notificationId"));
                var notificationId = (s as NSNumber).ToString();

                if (!string.IsNullOrEmpty(notificationId))
                {
                    return long.Parse(notificationId);
                }

            }

            return -1;
        }
    }
}
