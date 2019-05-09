using System;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using JW.Alarm.Services.Droid.Helpers;
using MediaManager;
using Android.Content;
using JW.Alarm.Common.Mvvm;
using Bible.Alarm.ViewModels;
using JW.Alarm.Services.Droid.Tasks;

namespace Bible.Alarm.Droid
{
    [Activity(Label = "Bible Alarm", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private bool initialized = false;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            AppCenter.Start("0cd5c3e8-dcfa-48dd-9d4b-0433a8572fb9",
                   typeof(Analytics));
            AppCenter.Start("0cd5c3e8-dcfa-48dd-9d4b-0433a8572fb9", typeof(Analytics));

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            if (IocSetup.Container == null)
            {
                IocSetup.Initialize();
                IocSetup.Container.Resolve<IMediaManager>().SetContext(this);
            }

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            Task.Run(async () =>
            {
                //BootstrapHelper.VerifyBackgroundTasks();
                await BootstrapHelper.VerifyMediaLookUpService();
                await BootstrapHelper.InitializeDatabase();
                await Messenger<bool>.Publish(Messages.Initialized, true);
                initialized = true;
            });
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (initialized && !AlarmSetupTask.IsRunning)
            {
                Intent service = new Intent(this, typeof(AlarmSetupTask));
                StartService(service);
            }
        }
    }
}