using Bible.Alarm.Services.Contracts;
using Bible.Alarm.UI;
using Bible.Alarm.ViewModels;
using Bible.Alarm.Common.Mvvm;
using MediaManager;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bible.Alarm
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if (UI.IocSetup.Container.RegisteredTypes.Any(x => x == typeof(NavigationPage)))
            {
                MainPage = UI.IocSetup.Container.Resolve<NavigationPage>();
            }
            else
            {
                var homePage = new Home();
                var navigationPage = new NavigationPage(homePage);

                UI.IocSetup.Container.Register(x => navigationPage.Navigation, isSingleton: true);
                UI.IocSetup.Container.Register(
                    x => navigationPage, isSingleton: true);


                MainPage = navigationPage;

                MainPage.SetValue(NavigationPage.BarBackgroundColorProperty, Color.SlateBlue);
                MainPage.SetValue(NavigationPage.BarTextColorProperty, Color.White);

                Task.Delay(0).ContinueWith((y) =>
                {
                    homePage.BindingContext = UI.IocSetup.Container.Resolve<Bible.Alarm.ViewModels.HomeViewModel>();

                }, TaskScheduler.FromCurrentSynchronizationContext());
            }

        }

        protected override void OnStart()
        {
            Task.Run(async () =>
            {
                var navigator = UI.IocSetup.Container.Resolve<INavigationService>();
                // Handle when your app starts  
                await navigator.NavigateToHome();

                var mediaManager = UI.IocSetup.Container.Resolve<IMediaManager>();
                if (mediaManager.IsPrepared())
                {
                    await Messenger<object>.Publish(Messages.ShowSnoozeDismissModal, UI.IocSetup.Container.Resolve<AlarmViewModal>());
                }
            });
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            Task.Run(async () =>
            {
                var mediaManager = UI.IocSetup.Container.Resolve<IMediaManager>();
                // Handle when your app resumes
                if (mediaManager.IsPrepared())
                {
                    await Messenger<object>.Publish(Messages.ShowSnoozeDismissModal, UI.IocSetup.Container.Resolve<AlarmViewModal>());
                }
            });
        }

    }
}
