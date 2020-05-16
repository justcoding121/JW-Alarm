using Android.App;
using Android.Content.PM;
using Android.OS;
using Bible.Alarm.Common.Extensions;
using Bible.Alarm.Droid.Services.Platform;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Infrastructure;
using Java.Interop;
using MediaManager;
using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bible.Alarm.Droid
{
    [Activity(Label = "Bible Alarm", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = false,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private IContainer container;

        private Logger logger => LogManager.GetCurrentClassLogger();

        public MainActivity()
        {
            LogSetup.Initialize(VersionFinder.Default,
                new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" }, Device.Android);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                container = BootstrapHelper.InitializeUI(this, logger, Application);

                TabLayoutResource = Resource.Layout.Tabbar;
                ToolbarResource = Resource.Layout.Toolbar;

                base.OnCreate(savedInstanceState);

                global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
                LoadApplication(new App(container));

                //legacy
                Task.Run(() =>
                {
                    try
                    {
                        var cachePath = Path.Combine(Path.GetTempPath(), "MediaCache");

                        // If exist, delete the cache directory
                        // and everything in it recursivly
                        if (Directory.Exists(cachePath))
                        {
                            Directory.Delete(cachePath, true);
                        }

                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Deleting cache directory failed in android.");
                    }
                });

            }
            catch (Exception e)
            {
                logger.Fatal(e, "Android application crashed.");
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
            return container.Resolve<IMediaManager>().IsPreparedEx().ToString();
        }
    }
}