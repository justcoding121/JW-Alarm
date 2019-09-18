using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using System.Threading.Tasks;
using JW.Alarm.Services.Droid.Helpers;
using MediaManager;
using Android.Content;
using JW.Alarm.Common.Mvvm;
using Bible.Alarm.Services.Infrastructure;
using NLog;
using JW.Alarm.Services.Droid.Tasks;

namespace Bible.Alarm.Droid
{
    [Activity(Label = "Bible Alarm", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = false,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        public MainActivity() : base()
        {
            LogSetup.Initialize();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                TabLayoutResource = Resource.Layout.Tabbar;
                ToolbarResource = Resource.Layout.Toolbar;

                base.OnCreate(savedInstanceState);

                if (IocSetup.Container == null)
                {
                    IocSetup.Initialize(this, false);
                    IocSetup.Container.Resolve<IMediaManager>().Init(this);
                }

                global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
                LoadApplication(new App());
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Android Application Crashed.");
                throw;
            }

            Task.Run(async () =>
            {
                try
                {
                    await BootstrapHelper.VerifyMediaLookUpService();
                    await BootstrapHelper.InitializeDatabase();
                    await Messenger<bool>.Publish(Messages.Initialized, true);

                    var intent = new Intent(this, typeof(AlarmSetupService));
                    intent.PutExtra("Action", "SetupBackgroundTasks");
                    StartService(intent);
                }
                catch (Exception e)
                {
                    logger.Fatal(e, "Android Initialization Crashed.");
                }
            });
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

    }
}