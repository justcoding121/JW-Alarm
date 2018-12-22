using JW.Alarm.Common.Mvvm;
using JW.Alarm.Core.UWP.Views;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace JW.Alarm.Core.Uwp
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            customizeTitleBar();
            setColorDefaults();

            Messenger<bool>.Subscribe(Messages.Progress, (bool inProgress) =>
            {
                if (inProgress)
                {
                    this.ProgressRing_Loading.IsActive = true;
                    this.ProgressRing_Loading.Visibility = Visibility.Visible;
                }
                else
                {
                    this.ProgressRing_Loading.IsActive = false;
                    this.ProgressRing_Loading.Visibility = Visibility.Collapsed;
                }

                return Task.FromResult(false);
            });
        }

        private void customizeTitleBar()
        {
            // customize title area
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(customTitleBar);

            // customize buttons' colors
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Colors.SlateBlue;
            titleBar.ButtonBackgroundColor = Colors.SlateBlue;
            titleBar.InactiveBackgroundColor = Colors.SlateBlue;
            titleBar.ButtonInactiveBackgroundColor = Colors.SlateBlue;

            titleBar.ForegroundColor = Colors.White;
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonInactiveForegroundColor = Colors.White;   
        }

        private void setColorDefaults()
        {
            Application.Current.Resources["ToggleSwitchFillOnPointerOver"] = new SolidColorBrush(Colors.SlateBlue);
            Application.Current.Resources["ToggleSwitchFillOn"] = new SolidColorBrush(Colors.SlateBlue);

            Application.Current.Resources["ButtonBackground"] = new SolidColorBrush(Colors.WhiteSmoke);
            Application.Current.Resources["ButtonBackgroundPointerOver"] = new SolidColorBrush(Colors.WhiteSmoke);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavFrame.Navigate(typeof(ScheduleList));
        }

        //private SolidColorBrush getSolidColorBrush(string hex)
        //{
        //    hex = hex.Replace("#", string.Empty);
        //    byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
        //    byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
        //    byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
        //    byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
        //    SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
        //    return myBrush;
        //}
    }
}
