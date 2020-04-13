using Bible.Alarm.iOS.Services.Platform;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.Services.iOS.Helpers;
using Foundation;
using NLog;
using System;
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

            }
            catch (Exception e)
            {
                logger.Fatal(e, "iOS application crashed.");
                throw;
            }

            return base.FinishedLaunching(app, options);
        }

    }
}
