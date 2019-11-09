using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using MediaManager;
using NLog;
using Java.Interop;
using System.Diagnostics;

namespace Bible.Alarm.Droid
{
    [Activity(Label = "Bible Alarm", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = false,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private Logger logger => LogHelper.GetLogger(global::Xamarin.Forms.Forms.IsInitialized);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                TabLayoutResource = Resource.Layout.Tabbar;
                ToolbarResource = Resource.Layout.Toolbar;

                base.OnCreate(savedInstanceState);

                global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
                LoadApplication(new App());

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
            return IocSetup.Container.Resolve<IMediaManager>().IsPrepared().ToString();
        }
    }
}