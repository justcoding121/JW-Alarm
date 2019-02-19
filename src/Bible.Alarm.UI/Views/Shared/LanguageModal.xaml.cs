using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bible.Alarm.UI.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LanguageModal : ContentPage
	{
		public LanguageModal ()
		{
			InitializeComponent ();
          
        }

        private async Task closePopup()
        {
            await Navigation.PopModalAsync();
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