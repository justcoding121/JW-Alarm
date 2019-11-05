using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Droid.Tasks;
using MediaManager;
using NLog;

namespace Bible.Alarm.Droid
{
    [Activity(Label = "Bible Alarm", Theme = "@style/MyTheme.Splash", Icon = "@mipmap/ic_launcher",
        MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        private static Logger logger => LogHelper.GetLogger(global::Xamarin.Forms.Forms.IsInitialized);

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)((int)Window.DecorView.SystemUiVisibility ^ (int)SystemUiFlags.LayoutStable ^ (int)SystemUiFlags.LayoutFullscreen);
            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            SetContentView(Resource.Layout.SplashScreen);

            if (IocSetup.Initialize(Application.Context, false))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await BootstrapHelper.VerifyMediaLookUpService();
                        await BootstrapHelper.InitializeDatabase();
                        await Messenger<bool>.Publish(Messages.Initialized, true);

                        await Task.Delay(1000);

                        var i = new Intent(IocSetup.Context, typeof(AlarmSetupService));
                        i.PutExtra("Action", "SetupBackgroundTasks");
                        StartService(i);

                    }
                    catch (Exception e)
                    {
                        logger.Fatal(e, "Android initialization crashed.");
                    }
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