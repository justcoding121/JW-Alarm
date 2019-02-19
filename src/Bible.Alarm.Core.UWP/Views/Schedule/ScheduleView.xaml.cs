using JW.Alarm.Core.UWP.Views.Bible;
using JW.Alarm.Core.UWP.Views.Music;
using JW.Alarm.Models;
using JW.Alarm.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JW.Alarm.Core.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScheduleView : Page
    {
        public ScheduleViewModel ViewModel => DataContext as ScheduleViewModel;

        public ScheduleView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DataContext = e.Parameter as ScheduleViewModel;
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            if (await ViewModel.saveAsync())
            {
                Frame.GoBack();
            }
        }

        private async void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.deleteAsync();
            Frame.GoBack();
        }

        private void Button_Day_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            switch (button.Name)
            {
                case "Button_Sunday":
                    ViewModel.toggle(DaysOfWeek.Sunday);
                    break;
                case "Button_Monday":
                    ViewModel.toggle(DaysOfWeek.Monday);
                    break;
                case "Button_Tuesday":
                    ViewModel.toggle(DaysOfWeek.Tuesday);
                    break;
                case "Button_Wednesday":
                    ViewModel.toggle(DaysOfWeek.Wednesday);
                    break;
                case "Button_Thursday":
                    ViewModel.toggle(DaysOfWeek.Thursday);
                    break;
                case "Button_Friday":
                    ViewModel.toggle(DaysOfWeek.Friday);
                    break;
                case "Button_Saturday":
                    ViewModel.toggle(DaysOfWeek.Saturday);
                    break;
            }


        }

        private void Music_Btn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MusicSelection), ViewModel.getMusicSelectionViewModel());
        }

        private void Bible_Btn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BibleSelection), ViewModel.getBibleSelectionViewModel());
        }
    }
}
