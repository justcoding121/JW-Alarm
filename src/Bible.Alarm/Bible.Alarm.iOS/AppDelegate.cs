﻿using Bible.Alarm.Common.Helpers;
using Bible.Alarm.iOS.Models;
using Foundation;
using Newtonsoft.Json;
using NLog;
using System;
using System.Net.Http;
using System.Text;
using UIKit;

namespace Bible.Alarm.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private IContainer container;
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

            container = IocSetup.GetContainer("SplashActivity");

            try
            {
                global::Xamarin.Forms.Forms.Init();
                LoadApplication(new App(container));

                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                {
                    var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
                                       UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                                       new NSSet());

                    UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
                    UIApplication.SharedApplication.RegisterForRemoteNotifications();
                }
                else
                {
                    UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
                    UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
                }
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
            var deviceId = NSUserDefaults.StandardUserDefaults.StringForKey("DeviceId");

            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = Guid.NewGuid().ToString();
                NSUserDefaults.StandardUserDefaults.SetString(deviceId, "DeviceId");
            }

            // Get current device token
            var DeviceToken = deviceToken.Description;
            if (!string.IsNullOrWhiteSpace(DeviceToken))
            {
                DeviceToken = DeviceToken.Trim('<').Trim('>');
            }

            // Get previous device token
            var oldDeviceToken = NSUserDefaults.StandardUserDefaults.StringForKey("PushDeviceToken");

            // Save new device token
            NSUserDefaults.StandardUserDefaults.SetString(DeviceToken, "PushDeviceToken");

            // Has the token changed?
            if (string.IsNullOrEmpty(oldDeviceToken) || !oldDeviceToken.Equals(DeviceToken))
            {
                var request = new RegisterDeviceRequest()
                {
                    DeviceId = deviceId,
                    DeviceToken = DeviceToken
                };

                try
                {
#if DEBUG
                    var url = "https://192.168.1.64:5011/api/v1/RegisterDevice";
#else
                    var url = "https://production-push.jthomas.info/api/v1/RegisterDevice";
#endif

                    var result = await RetryHelper.Retry(async () =>
                    {
                        using var client = new HttpClient();
                        var payload = JsonConvert.SerializeObject(request);
                        HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
                        return await client.PostAsync(url, content);

                    }, 3);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error happenned when updating ios device token.");
                }
            }

        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            logger.Error($"Error registering push notifications. Description: {error.LocalizedDescription}");
        }
    }
}
