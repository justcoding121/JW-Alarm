using Bible.Alarm.Services.Contracts;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Bible.Alarm.UI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var navigationPage = new NavigationPage();
            IocSetup.Container.Register(x => navigationPage.Navigation, isSingleton: true);

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
