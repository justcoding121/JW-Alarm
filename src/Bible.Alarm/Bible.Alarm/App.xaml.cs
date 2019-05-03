using Bible.Alarm.Services;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.UI;
using Bible.Alarm.UI.Views;
using Bible.Alarm.ViewModels;
using JW.Alarm.Services.Contracts;
using MediaManager;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Bible.Alarm
{
    public partial class App : Application
    {
        private readonly Home homePage;
        private readonly IMediaManager mediaManager;

        public App()
        {
            InitializeComponent();

            homePage = new Home();

            mediaManager = IocSetup.Container.Resolve<IMediaManager>();

            var navigationPage = new NavigationPage(homePage);
            IocSetup.Container.Register(x => navigationPage.Navigation, isSingleton: true);
            homePage.BindingContext = IocSetup.Container.Resolve<JW.Alarm.ViewModels.HomeViewModel>();
            MainPage = navigationPage;

            var receiver = IocSetup.Container.Resolve<AlarmReceiver>();
            var syncContext = TaskScheduler.FromCurrentSynchronizationContext();
            receiver.Received += (o, e) =>
            {
                Task.Factory.StartNew(async () =>
                 {
                     var navigator = IocSetup.Container.Resolve<INavigationService>();
                     var vm = IocSetup.Container.Resolve<AlarmViewModal>();
                     await navigator.ShowModal("AlarmModal", vm);

                 },
                 CancellationToken.None,
                 TaskCreationOptions.None,
                 syncContext
                 );
            };
        }

        protected async override void OnStart()
        {
            var navigator = IocSetup.Container.Resolve<INavigationService>();
            if (mediaManager.IsPlaying())
            {
                var vm = IocSetup.Container.Resolve<AlarmViewModal>();
                await navigator.ShowModal("AlarmModal", vm);
                return;
            }

            // Handle when your app starts  
            await navigator.NavigateToHome();

        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected async override void OnResume()
        {
            // Handle when your app resumes
            if (mediaManager.IsPlaying())
            {
                var navigator = IocSetup.Container.Resolve<INavigationService>();
                var vm = IocSetup.Container.Resolve<AlarmViewModal>();
                await navigator.ShowModal("AlarmModal", vm);
            }
        }

    }
}
