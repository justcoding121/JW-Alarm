using System;
using System.Threading.Tasks;
using Foundation;
using Bible.Alarm.Common.Mvvm;
using UIKit;
using NLog;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.iOS.Services.Platform;
using Bible.Alarm.Services.iOS.Helpers;

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
            container = IocSetup.GetContainer("SplashActivity");

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
