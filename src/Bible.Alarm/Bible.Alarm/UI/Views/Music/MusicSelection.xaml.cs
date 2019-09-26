using Bible.Alarm.UI.ViewHelpers;
using Bible.Alarm.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bible.Alarm.UI.Views.Music
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MusicSelection : ContentPage
    {
        public MusicSelectionViewModel ViewModel => BindingContext as MusicSelectionViewModel;

        public MusicSelection()
        {
            InitializeComponent();

            BackButton.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() => AnimateUtils.FlickUponTouched(BackButton, 1500,
                ColorUtils.ToHexString(Color.LightGray), ColorUtils.ToHexString(Color.WhiteSmoke), 1))
            });
        }

        protected override bool OnBackButtonPressed()
        {
            ViewModel.BackCommand.Execute(null);
            return true;
        }
    }
}