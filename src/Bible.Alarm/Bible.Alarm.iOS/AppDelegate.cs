using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services.iOS.Helpers;
using UIKit;
using NLog;
using Bible.Alarm.Services.Infrastructure;

namespace Bible.Alarm.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        public AppDelegate() : base()
        {
            LogSetup.Initialize("iOS");
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
            logger.Info("ios App launch started.");

            try
            {
                IocSetup.Initialize();

                global::Xamarin.Forms.Forms.Init();
                LoadApplication(new App());
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
                    await BootstrapHelper.InitializeDatabase();
                    await BootstrapHelper.VerifyMediaLookUpService();
                    await Messenger<bool>.Publish(Bible.Alarm.Common.Mvvm.Messages.Initialized, true);
                }
                catch (Exception e)
                {
                    logger.Fatal(e, "iOS initialization crashed.");
                }
            });

            return base.FinishedLaunching(app, options);
        }
    }
}
