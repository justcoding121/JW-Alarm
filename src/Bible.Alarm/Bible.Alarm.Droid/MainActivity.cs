using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using System.Threading.Tasks;
using Bible.Alarm.Services.Droid.Helpers;
using MediaManager;
using Android.Content;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services.Infrastructure;
using NLog;
using Bible.Alarm.Services.Droid.Tasks;
using Java.Interop;

namespace Bible.Alarm.Droid
{
    [Activity(Label = "Bible Alarm", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = false,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        public MainActivity() : base()
        {
            LogSetup.Initialize("Android");
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                TabLayoutResource = Resource.Layout.Tabbar;
                ToolbarResource = Resource.Layout.Toolbar;

                base.OnCreate(savedInstanceState);

                IocSetup.Initialize(this, false);

                global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
                LoadApplication(new App());
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Android application crashed.");
                throw;
            }


            Task.Run(async () =>
            {
                try
                {
                    await BootstrapHelper.VerifyMediaLookUpService();
                    await BootstrapHelper.InitializeDatabase();
                    await Messenger<bool>.Publish(Messages.Initialized, true);

                    var intent = new Intent(IocSetup.Context, typeof(AlarmSetupService));
                    intent.PutExtra("Action", "SetupBackgroundTasks");
                    StartService(intent);
                }
                catch (Exception e)
                {
                    logger.Fatal(e, "Android initialization crashed.");
                }
            });
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