using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using JW.Alarm.Services.Droid.Helpers;

namespace Bible.Alarm.Droid
{
    [Activity(Label = "Bible.Alarm", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

     
            IocSetup.Initialize();
            Task.Run(async () =>
            {
                //BootstrapHelper.VerifyBackgroundTasks();
                await BootstrapHelper.VerifyMediaLookUpService();
                BootstrapHelper.InitializeDatabase();
              
            });


            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
    }
}