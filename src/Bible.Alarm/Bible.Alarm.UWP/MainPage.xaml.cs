using Bible.Alarm.Services.Uwp;

namespace Bible.Alarm.UWP
{
    public sealed partial class MainPage
    {
        private static IContainer container => IocSetup.Container;
        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new Alarm.App(container));
        }
    }
}
