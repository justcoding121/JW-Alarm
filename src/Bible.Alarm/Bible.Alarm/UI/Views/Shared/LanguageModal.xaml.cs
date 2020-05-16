
using Bible.Alarm.Contracts.UI;
using Mvvmicro;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bible.Alarm.UI.Views
{
    public partial class LanguageModal : ContentPage
    {
        public IListViewModel ViewModel => BindingContext as IListViewModel;

        public LanguageModal()
        {
            InitializeComponent();
            this.Appearing += onAppearing;
        }

        private void onAppearing(object sender, EventArgs e)
        {
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Task.Delay(100).ContinueWith(x =>
            {
                LanguageListView.ScrollTo(ViewModel.SelectedItem, ScrollToPosition.Center, true);
                this.Appearing -= onAppearing;

            }, scheduler);
        }
    }
}