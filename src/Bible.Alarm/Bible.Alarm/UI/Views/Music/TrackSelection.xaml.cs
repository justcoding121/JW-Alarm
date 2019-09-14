using Bible.Alarm.UI.ViewHelpers;
using JW.Alarm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bible.Alarm.UI.Views.Music
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TrackSelection : ContentPage
    {
        public TrackSelectionViewModel ViewModel => BindingContext as TrackSelectionViewModel;

        public TrackSelection()
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