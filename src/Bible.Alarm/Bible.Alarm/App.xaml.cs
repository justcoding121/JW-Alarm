using Bible.Alarm.Services;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.UI;
using Bible.Alarm.UI.Views;
using Bible.Alarm.ViewModels;
using JW.Alarm.Common.Mvvm;
using JW.Alarm.Services.Contracts;
using MediaManager;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Bible.Alarm
{
    public partial class App : Application
    {
        private readonly IMediaManager mediaManager;

        public App()
        {
            InitializeComponent();

            mediaManager = IocSetup.Container.Resolve<IMediaManager>();

            if (IocSetup.Container.RegisteredTypes.Any(x => x == typeof(NavigationPage)))
            {
                MainPage = IocSetup.Container.Resolve<NavigationPage>();
            }
            else
            {
                var homePage = new Home();
                var navigationPage = new NavigationPage(homePage);

                IocSetup.Container.Register(x => navigationPage.Navigation, isSingleton: true);
                IocSetup.Container.Register(x => navigationPage, isSingleton: true);

                homePage.BindingContext = IocSetup.Container.Resolve<JW.Alarm.ViewModels.HomeViewModel>();
                MainPage = navigationPage;
            }
        }

        protected async override void OnStart()
        {
            var navigator = IocSetup.Container.Resolve<INavigationService>();
            // Handle when your app starts  
            await navigator.NavigateToHome();

            if (mediaManager.IsPlaying())
            {
                await Messenger<object>.Publish(Messages.ShowSnoozeDismissModal, IocSetup.Container.Resolve<AlarmViewModal>());
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
            if (mediaManager.IsPlaying())
            {
                Task.Run(() => Messenger<object>.Publish(Messages.ShowSnoozeDismissModal, IocSetup.Container.Resolve<AlarmViewModal>()));
            }
        }

    }
}
