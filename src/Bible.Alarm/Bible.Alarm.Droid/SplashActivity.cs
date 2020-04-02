using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Bible.Alarm.Droid.Services.Platform;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Infrastructure;
using NLog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bible.Alarm.Droid
{
    [Activity(Label = "Bible Alarm", Theme = "@style/MyTheme.Splash", Icon = "@mipmap/ic_launcher",
        MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        public SplashActivity()
        {
            LogSetup.Initialize(VersionFinder.Default,
                new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" });
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            try
            {
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)((int)Window.DecorView.SystemUiVisibility ^ (int)SystemUiFlags.LayoutStable ^ (int)SystemUiFlags.LayoutFullscreen);
                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

                SetContentView(Resource.Layout.SplashScreen);

                BootstrapHelper.InitializeUI(this, logger, Application);
            }
            catch (Exception e)
            {
                logger.Fatal(e, "An error happened inside OnCreate.");
                throw;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            try
            {
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened inside OnRequestpermissionResult.");
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
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
            try
            {
                var intent = new Intent(Application.Context, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.ReorderToFront);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened in doWork() task under SplashActivity.");
            }
        }
    }
}