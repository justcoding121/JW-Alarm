﻿using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.Services.Tasks;
using Bible.Alarm.Services.Uwp.Helpers;
using Bible.Alarm.Uwp;
using Bible.Alarm.Uwp.Services.Platform;
using Bible.Alarm.UWP.Services.Handlers;
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
        private static readonly Lazy<Logger> lazyLogger = new Lazy<Logger>(() => LogManager.GetCurrentClassLogger());
        private static Logger logger => lazyLogger.Value;


        private IContainer container;

        public App()
        {
            bool isLoggingEnabled = true;

#if DEBUG
            //We have an issue with NLOG throwing fatal error on DEBUG
            isLoggingEnabled = false;
#endif
            LogSetup.Initialize(UwpVersionFinder.Default,
                new string[] { }, Xamarin.Forms.Device.UWP,
                isLoggingEnabled);

            AppDomain.CurrentDomain.UnhandledException += unhandledExceptionHandler;
            TaskScheduler.UnobservedTaskException += unobserverdTaskException;

            initContainer();

            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        private void unobserverdTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            logger.Error(e.Exception,"Unobserved task exception.");
        }

        private void unhandledExceptionHandler(object sender, System.UnhandledExceptionEventArgs e)
        {
            logger.Error("Unhandled exception.", e.SerializeObject());
        }

        private void initContainer()
        {
            try
            {
                var result = IocSetup.Initialize("SplashActivity", false);
                container = result.Item1;
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
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(400, 500));

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

        protected override void OnActivated(IActivatedEventArgs e)
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(400, 500));

            ApplicationView.PreferredLaunchViewSize = new Size(400, 600);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (!(Window.Current.Content is Frame rootFrame))
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
                rootFrame.Navigate(typeof(MainPage));
            }

            // Ensure the current window is active
            Window.Current.Activate();

            // Handle toast activation
            if (e is ToastNotificationActivatedEventArgs)
            {
                var toastActivationArgs = e as ToastNotificationActivatedEventArgs;
                var scheduleId = int.Parse(toastActivationArgs.Argument);

                handleAlarm(scheduleId);
            }
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

                case "MediaIndexUpdateTask":
                    {
                        using var mediaIndexService = container.Resolve<Alarm.Services.MediaIndexService>();
                        await mediaIndexService.UpdateIndexIfAvailable();
                    }
                    break;
            }

            deferral.Complete();
        }

        private void handleAlarm(int scheduleId)
        {
            var uwpAlarmHandler = container.Resolve<UwpAlarmHandler>();
            _ = uwpAlarmHandler.Handle(scheduleId, true);
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

        private bool disposed = false;
        private void dispose()
        {
            if (disposed)
            {
                return;
            }

            AppDomain.CurrentDomain.UnhandledException -= unhandledExceptionHandler;
            TaskScheduler.UnobservedTaskException -= unobserverdTaskException;

            disposed = true;
            GC.SuppressFinalize(this);
        }

        ~App()
        {
            dispose();
        }
    }
}
