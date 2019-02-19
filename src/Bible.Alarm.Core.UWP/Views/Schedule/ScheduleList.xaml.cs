using JW.Alarm.Core.Uwp;
using JW.Alarm.Models;
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
        public HomeViewModel ViewModel => DataContext as HomeViewModel;

        public ScheduleList()
        {
            this.InitializeComponent();
            DataContext = Uwp.IocSetup.Container.Resolve<HomeViewModel>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void addScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ScheduleView), new ScheduleViewModel());
        }

        private void SchedulesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                Frame.Navigate(typeof(ScheduleView),
                    new ScheduleViewModel((sender as ListView).SelectedItem as ScheduleListItem));
            }
        }
    }
}
