using Bible.Alarm.UI.ViewHelpers;
using Bible.Alarm.ViewModels;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bible.Alarm.UI.Views.Bible
{
	public partial class ChapterSelection : ContentPage
	{
        public ChapterSelectionViewModel ViewModel => BindingContext as ChapterSelectionViewModel;

        public ChapterSelection ()
		{
			InitializeComponent ();

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
                chapterListView.ScrollTo(ViewModel.SelectedChapter, ScrollToPosition.Center, true);
                this.Appearing -= onAppearing;

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        protected override bool OnBackButtonPressed()
        {
            ViewModel.BackCommand.Execute(null);
            return true;
        }
    }
}