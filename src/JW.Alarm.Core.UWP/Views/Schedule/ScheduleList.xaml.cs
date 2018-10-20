using Autofac;
using JW.Alarm.Core.Uwp;
using JW.Alarm.Services.Contracts;
using JW.Alarm.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JW.Alarm.Core.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScheduleList : Page
    {
        public MainViewModel ViewModel => DataContext as MainViewModel;

        public ScheduleList()
        {
            this.InitializeComponent();
            DataContext = Uwp.IocSetup.Container.Resolve<MainViewModel>();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.GetScheduleListAsync();
        }

        private void Toggle_IsEnabled_Toggled(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;

            if (toggleSwitch != null)
            {
                var listViewItem = toggleSwitch.FindVisualAncestor<ListViewItem>();

                if (listViewItem != null)
                {
                    var schedule = listViewItem.Content as ScheduleViewModel;

                    if (schedule != null)
                    {
                        if (toggleSwitch.IsOn == true)
                        {
                            schedule.EnableCommand.Execute(true);
                        }
                        else
                        {
                            schedule.EnableCommand.Execute(false);
                        }
                    }
                }
            }

        }

        private void addScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ScheduleView), new ScheduleViewModel());
        }

        private void SchedulesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
            {
                Frame.Navigate(typeof(ScheduleView), (sender as ListView).SelectedItem as ScheduleViewModel);
            }     
        }
    }
}
