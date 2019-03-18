using Bible.Alarm.Services.Contracts;
using Bible.Alarm.UI;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Bible.Alarm
{
    public partial class App : Application
    {
        private readonly Home homePage;
        public App()
        {
            InitializeComponent();

            homePage = new Home();
            var navigationPage = new NavigationPage(homePage);
            IocSetup.Container.Register(x => navigationPage.Navigation, isSingleton: true);
            homePage.BindingContext = IocSetup.Container.Resolve<JW.Alarm.ViewModels.HomeViewModel>();
            MainPage = navigationPage;
        }

        protected async override void OnStart()
        {
            // Handle when your app starts  
            var navigater = IocSetup.Container.Resolve<INavigationService>();
            await navigater.NavigateToHome();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
