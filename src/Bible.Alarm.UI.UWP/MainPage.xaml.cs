using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Bible.Alarm.UI.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            //customizeTitleBar();
            //setColorDefaults();

            LoadApplication(new Bible.Alarm.UI.App());
        }

        private void customizeTitleBar()
        {
            // customize title area
            //CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            //var grid = new Grid()
            //{
            //    Height = 35
            //};

            //RowDefinition gridRow0 = new RowDefinition();
            //gridRow0.Height = new GridLength(45);

            //grid.RowDefinitions.Add(gridRow0);

            //var textBlock = new TextBlock();
            //textBlock.Text = "Bible Alarm";
            //textBlock.Foreground = new SolidColorBrush(Colors.White);
            //textBlock.VerticalAlignment = VerticalAlignment.Center;
            //textBlock.Margin = new Thickness(12, 0, 0, 0);

            //grid.Children.Add(textBlock);
            //Grid.SetRow(textBlock, 0);

            //Window.Current.SetTitleBar(grid);

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
    }
}
