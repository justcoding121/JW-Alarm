using Bible.Alarm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bible.Alarm.UI.Views
{
	public partial class AlarmModal : ContentPage
	{
        public AlarmViewModal ViewModel => BindingContext as AlarmViewModal;
        public AlarmModal ()
		{
			InitializeComponent ();
		}
        protected override bool OnBackButtonPressed()
        {
            ViewModel.CancelCommand.Execute(null);
            return true;
        }

    }
}