using Bible.Alarm.Services.Uwp;
using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace Bible.Alarm.UWP
{
    public sealed partial class MainPage
    {
        private static IContainer container => IocSetup.Container;
        public MainPage()
        {
            this.InitializeComponent();

            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var localValue = localSettings.Values["firstLaunchComplete"] as string;

            if (string.IsNullOrEmpty(localValue))
            {
                ApplicationView.PreferredLaunchViewSize = new Size(400, 600);
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

                localSettings.Values["firstLaunchComplete"] = true.ToString();
            }

            // resetting the auto-resizing -> next launch the system will control the PreferredLaunchViewSize
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;


            LoadApplication(new Alarm.App(container));
        }
    }
}
