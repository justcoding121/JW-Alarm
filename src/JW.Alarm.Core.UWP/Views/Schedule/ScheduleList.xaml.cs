using JW.Alarm.Core.Uwp;
using JW.Alarm.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace JW.Alarm.Core.UWP.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScheduleList : Page
    {
        public ScheduleListViewModel ViewModel => DataContext as ScheduleListViewModel;

        public ScheduleList()
        {
            this.InitializeComponent();
            DataContext = Uwp.IocSetup.Container.Resolve<ScheduleListViewModel>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
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
