using Bible.Alarm.Services;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.UI;
using Bible.Alarm.ViewModels;
using Bible.Alarm.Common.Mvvm;
using MediaManager;
using System.Linq;
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

            mediaManager = UI.IocSetup.Container.Resolve<IMediaManager>();

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

                homePage.BindingContext = UI.IocSetup.Container.Resolve<Bible.Alarm.ViewModels.HomeViewModel>();

                MainPage = navigationPage;          
            }

            MainPage.SetValue(NavigationPage.BarBackgroundColorProperty, Color.SlateBlue);
            MainPage.SetValue(NavigationPage.BarTextColorProperty, Color.White);
        }

        protected async override void OnStart()
        {
            var navigator = UI.IocSetup.Container.Resolve<INavigationService>();
            // Handle when your app starts  
            await navigator.NavigateToHome();

            if (mediaManager.IsPrepared())
            {
                await Messenger<object>.Publish(Messages.ShowSnoozeDismissModal, UI.IocSetup.Container.Resolve<AlarmViewModal>());
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
            if (mediaManager.IsPrepared())
            {
                Task.Run(() => Messenger<object>.Publish(Messages.ShowSnoozeDismissModal, UI.IocSetup.Container.Resolve<AlarmViewModal>()));
            }
        }

    }
}
