using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bible.Alarm.UI.Views.Shared
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LanguageSelector : ContentPage
	{
		public LanguageSelector ()
		{
			InitializeComponent ();
            this.LayoutChanged += LanguageSelection_LayoutUpdated;
        }

        private void LanguageSelection_LayoutUpdated(object sender, object e)
        {
            if (Width == 0 && Height == 0)
            {
                return;
            }

            
            //this.LanguageListView.MinHeight = Window.Current.Bounds.Height - 200;
            //this.LanguageListView.MaxHeight = Window.Current.Bounds.Height - 200;

            //this.LanguageListView.MinWidth = Window.Current.Bounds.Width - 50;
            //this.LanguageListView.MaxWidth = Window.Current.Bounds.Width - 50;

        }


        private async Task closePopup()
        {
            await Navigation.PopAsync();
        }

        private async void LanguageListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            await closePopup();
        }

        private async void CloseButton_Clicked(object sender, System.EventArgs e)
        {
            await closePopup();
        }
    }
}