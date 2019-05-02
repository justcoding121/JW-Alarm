using Bible.Alarm.Services.Contracts;
using Bible.Alarm.UI;
using JW.Alarm.Services.Contracts;
using Mvvmicro;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Bible.Alarm.ViewModels
{
    public class AlarmViewModal : ViewModel
    {
        private IPlaybackService playbackService;
        private INavigationService navigationService;

        public Command SnoozeCommand { get; private set; }
        public Command DismissCommand { get; private set; }

        public AlarmViewModal()
        {
            this.playbackService = IocSetup.Container.Resolve<IPlaybackService>();
            this.navigationService = IocSetup.Container.Resolve<INavigationService>();

            SnoozeCommand = new Command(() =>
            {
                playbackService.Snooze();
                navigationService.CloseModal();
            });

            DismissCommand = new Command(() =>
            {
                playbackService.Dismiss();
                navigationService.CloseModal();
            });
        }
    }
}
