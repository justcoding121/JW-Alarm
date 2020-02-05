using Android.App;
using Android.Content.PM;
using Android.OS;
using Java.Interop;
using MediaManager;
using NLog;
using System;
using System.Diagnostics;

namespace Bible.Alarm.Droid
{
    [Activity(Label = "Bible Alarm", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = false,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private IContainer container;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            container = IocSetup.GetContainer("SplashActivity");

            try
            {
                TabLayoutResource = Resource.Layout.Tabbar;
                ToolbarResource = Resource.Layout.Toolbar;

                base.OnCreate(savedInstanceState);

                global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
                LoadApplication(new App(container));

            }
            catch (Exception e)
            {
                logger.Fatal(e, "Android application crashed.");
                throw;
            }

        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        //For Unit tests
        [Export]
        public string GetDeviceTime()
        {
            return DateTime.Now.ToString();
        }

        [Export]
        public string IsAlarmOn()
        {
            return container.Resolve<IMediaManager>().IsPrepared().ToString();
        }
    }
}