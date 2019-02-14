
using Bible.Alarm.UI.Views.Schedule;
using JW.Alarm.Common.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bible.Alarm.UI
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
       
            Messenger<bool>.Subscribe(Messages.Progress, (bool inProgress) =>
            {
                if (inProgress)
                {
                    this.ProgressRing_Loading.IsRunning = true;
                    this.ProgressRing_Loading.IsVisible = true;
                }
                else
                {
                    this.ProgressRing_Loading.IsRunning = false;
                    this.ProgressRing_Loading.IsVisible = false;
                }

                return Task.FromResult(false);
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            NavFrame.Content = new ScheduleList();
        }
    }
}
