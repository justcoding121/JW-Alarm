﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Bible.Alarm.Droid.Services.Platform;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Infrastructure;
using NLog;
using Plugin.CurrentActivity;
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
                new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" }, Xamarin.Forms.Device.Android);
        }

        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    Window.SetDecorFitsSystemWindows(false);
                }
                else
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    try
                    {
                        Window.DecorView.SystemUiVisibility = (StatusBarVisibility)((int)Window.DecorView.SystemUiVisibility ^ (int)SystemUiFlags.LayoutStable ^ (int)SystemUiFlags.LayoutFullscreen);
                    }
                    catch { }
#pragma warning restore CS0618 // Type or member is obsolete
                }

                Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

                SetContentView(Resource.Layout.SplashScreen);
            }
            catch (Exception e)
            {
                logger.Fatal(e, "An error happened inside OnCreate.");
                await Task.Delay(1500);
                throw;
            }

        }


        public async override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            try
            {
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened inside OnRequestpermissionResult.");
                await Task.Delay(1500);
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();

            Task.Run(async () => await doWork());
        }

        // background work that happens behind the splash screen
        private async Task doWork()
        {
            try
            {
                var intent = new Intent(Application.Context, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.ReorderToFront);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                logger.Fatal(e, "An error happened in doWork() task under SplashActivity.");
                await Task.Delay(1500);
                throw;
            }
        }
    }
}