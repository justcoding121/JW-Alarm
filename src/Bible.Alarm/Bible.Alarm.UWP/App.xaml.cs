using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.Services.Uwp.Helpers;
using Bible.Alarm.Services.Uwp.Tasks;
using Bible.Alarm.Uwp;
using Bible.Alarm.Uwp.Services.Platform;
using NLog;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Bible.Alarm.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private Logger logger => LogManager.GetCurrentClassLogger();
        private readonly IContainer container;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            LogSetup.Initialize(VersionFinder.Default, new string[] { });
            initContainer();

            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        private void initContainer()
        {
            try
            {
                var result = IocSetup.Initialize("SplashActivity", false);
                var container = result.Item1;
                var containerCreated = result.Item2;
                if (containerCreated)
                {
                    BootstrapHelper.Initialize(container, logger);
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex, "Main initialization failed.");
                throw;
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(200, 300));

            ApplicationView.PreferredLaunchViewSize = new Size(250, 400);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;

            Task.Run(() =>
            {
                try
                {
                    Messenger<bool>.Publish(MvvmMessages.Initialized, true);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "An error happened in Messenger Publish call.");
                }
            });


            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                Xamarin.Forms.Forms.Init(e);

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

        protected async override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            var deferral = args.TaskInstance.GetDeferral();

            switch (args.TaskInstance.Task.Name)
            {
                case "SchedulerTask":
                    await container.Resolve<SchedulerTask>().Handle();
                    break;
            }

            deferral.Complete();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
