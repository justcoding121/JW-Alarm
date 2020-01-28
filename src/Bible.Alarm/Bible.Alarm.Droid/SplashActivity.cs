using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Droid.Services.Platform;
using Bible.Alarm.Services;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Droid.Tasks;
using Bible.Alarm.Services.Infrastructure;
using MediaManager;
using NLog;

namespace Bible.Alarm.Droid
{
    [Activity(Label = "Bible Alarm", Theme = "@style/MyTheme.Splash", Icon = "@mipmap/ic_launcher",
        MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        private IContainer container;
        private Logger logger;

        public SplashActivity()
        {
            LogSetup.Initialize(VersionFinder.Default,
                new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" });

            logger = LogManager.GetCurrentClassLogger();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)((int)Window.DecorView.SystemUiVisibility ^ (int)SystemUiFlags.LayoutStable ^ (int)SystemUiFlags.LayoutFullscreen);
            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            SetContentView(Resource.Layout.SplashScreen);

            var result = IocSetup.Initialize("SplashActivity", Application.Context, false);
            this.container = result.Item1;
            var containerCreated = result.Item2;
            if (containerCreated)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var task1 = BootstrapHelper.VerifyMediaLookUpService(container);
                        var task2 = BootstrapHelper.InitializeDatabase(container);

                        await Task.WhenAll(task1, task2);

                        await Messenger<bool>.Publish(Messages.Initialized, true);

                        await Task.Delay(1000);
                        
                        var i = new Intent(container.AndroidContext(), typeof(AlarmSetupService));
                        i.PutExtra("Action", "SetupBackgroundTasks");
                        StartService(i);

                    }
                    catch (Exception e)
                    {
                        logger.Fatal(e, "Android initialization crashed.");
                    }
                });
            }
            else
            {
                Task.Run(async () =>
                {
                    await Messenger<bool>.Publish(Messages.Initialized, true);
                });
            }
        }

        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();

            Task.Run(() => doWork());
        }

        // background work that happens behind the splash screen
        void doWork()
        {
            var intent = new Intent(Application.Context, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ReorderToFront);
            StartActivity(intent);
        }
    }
}