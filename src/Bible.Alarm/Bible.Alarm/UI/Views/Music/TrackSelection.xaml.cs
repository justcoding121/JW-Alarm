using Bible.Alarm.UI.ViewHelpers;
using Bible.Alarm.ViewModels;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bible.Alarm.UI.Views.Music
{

    public partial class TrackSelection : ContentPage
    {
        private readonly IContainer container;
        public TrackSelectionViewModel ViewModel => BindingContext as TrackSelectionViewModel;

        public TrackSelection(IContainer container)
        {
            this.container = container;
            InitializeComponent();

            BackButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => AnimateUtils.FlickUponTouched(BackButton, 1500,
                ColorUtils.ToHexString(Color.LightGray), ColorUtils.ToHexString(Color.WhiteSmoke), 1))
            });

            this.Appearing += onAppearing;
        }

        private void onAppearing(object sender, EventArgs e)
        {
            Task.Delay(100).ContinueWith(x =>
            {
                trackListView.ScrollTo(ViewModel.SelectedTrack, ScrollToPosition.Center, true);
                this.Appearing -= onAppearing;

            }, container.Resolve<TaskScheduler>());
        }

        protected override bool OnBackButtonPressed()
        {
            ViewModel.BackCommand.Execute(null);
            return true;
        }

    }
}