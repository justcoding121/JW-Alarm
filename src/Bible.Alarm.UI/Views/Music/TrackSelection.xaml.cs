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
        }

        protected override bool OnBackButtonPressed()
        {
            if (base.OnBackButtonPressed())
            {
                ViewModel.BackCommand.Execute(null);
                return true;
            }

            return false;
        }
    }
}