using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Bible.Alarm.Common.Extensions;
using Bible.Alarm.Droid.Services.Platform;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Droid.Tasks;
using Bible.Alarm.Services.Infrastructure;
using Java.Interop;
using MediaManager;
using NLog;
using Plugin.CurrentActivity;
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

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                container = BootstrapHelper.InitializeUI(logger, this, Application);

                TabLayoutResource = Resource.Layout.Tabbar;
                ToolbarResource = Resource.Layout.Toolbar;

                base.OnCreate(bundle);

                Forms.Init(this, bundle);
                LoadApplication(new App(container));

                Task.Run(() =>
                {
                    //legacy
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

            try
            {
                CrossCurrentActivity.Current.Init(this, bundle);
                CrossCurrentActivity.Current.ActivityStateChanged += currentActivityStateChanged;
                callSchedulerTask();
            }
            catch (Exception e)
            {
                logger.Error(e, "CrossCurrentActivity init error.");
                throw;
            }
        }

        private void callSchedulerTask()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (lastResumeTime.HasValue
                        && DateTime.Now.Subtract(lastResumeTime.Value).TotalSeconds >= 3)
                        {
                            var i = new Intent(container.AndroidContext(), typeof(AlarmSetupService));
                            i.PutExtra("Action", "SetupBackgroundTasks");
                            StartService(i);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "An error happened when calling setup background task upon app launch.");
                        break;
                    }

                    await Task.Delay(1000);
                }
            });
        }

        private DateTime? lastResumeTime;
        private void currentActivityStateChanged(object sender, ActivityEventArgs e)
        {
            if (e.Event == ActivityEvent.Resumed)
            {
                lastResumeTime = DateTime.Now;
            }
            else
            {
                lastResumeTime = null;
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            BootstrapHelper.Remove(this);
        }
    }
}