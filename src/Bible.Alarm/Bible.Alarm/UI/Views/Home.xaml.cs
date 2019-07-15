using JW.Alarm.ViewModels;
using Xamarin.Forms;

namespace Bible.Alarm.UI
{
    public partial class Home : ContentPage
    {
        public HomeViewModel ViewModel => BindingContext as HomeViewModel;

        public Home()
        {
            InitializeComponent();
            
        }
    }
}
