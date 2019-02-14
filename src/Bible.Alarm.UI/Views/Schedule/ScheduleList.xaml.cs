using JW.Alarm.Models;
using JW.Alarm.Services;
using JW.Alarm.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bible.Alarm.UI.Views.Schedule
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScheduleList : ContentView
    {
        public ScheduleList()
        {
            InitializeComponent();
            BindingContext = IocSetup.Container.Resolve<ScheduleListViewModel>();
        }

        private void AddScheduleButton_Clicked(object sender, EventArgs e)
        {

        }

        private void SchedulesListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

        }
    }
}