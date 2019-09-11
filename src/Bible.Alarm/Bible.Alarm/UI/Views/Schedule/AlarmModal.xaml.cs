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
	public partial class AlarmModal : ContentPage
	{
        public static bool IsShown { get; set; }
		public AlarmModal ()
		{
			InitializeComponent ();
		}
	}
}