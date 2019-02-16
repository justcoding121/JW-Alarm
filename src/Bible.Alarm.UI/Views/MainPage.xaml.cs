
using Bible.Alarm.UI.Views;
using JW.Alarm.Common.Mvvm;
using JW.Alarm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bible.Alarm.UI
{
    public partial class MainPage : ContentPage
    {
        public ScheduleListViewModel ViewModel => BindingContext as ScheduleListViewModel;

        public MainPage()
        {
            InitializeComponent();

            Messenger<bool>.Subscribe(Messages.Progress, (bool inProgress) =>
            {
                if (inProgress)
                {
                    this.ProgressRing_Loading.IsRunning = true;
                    this.ProgressRing_Loading.IsVisible = true;
                }
                else
                {
                    this.ProgressRing_Loading.IsRunning = false;
                    this.ProgressRing_Loading.IsVisible = false;
                }

                return Task.FromResult(false);
            });

            BindingContext = IocSetup.Container.Resolve<ScheduleListViewModel>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private async void AddScheduleButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Schedule()
            {
                BindingContext = new ScheduleViewModel()
            });
        }

        private async void SchedulesListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            await Navigation.PushAsync(new Schedule()
            {
                BindingContext = new ScheduleViewModel((e.SelectedItem as ScheduleListItem).Schedule)
            });
        }
    }
}
