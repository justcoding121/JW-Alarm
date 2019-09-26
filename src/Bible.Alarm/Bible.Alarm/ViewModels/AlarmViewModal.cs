﻿using Bible.Alarm.Services.Contracts;
using Mvvmicro;
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

            SnoozeCommand = new Command(async () =>
            {
                await playbackService.Snooze();
                await navigationService.CloseModal();
                await navigationService.NavigateToHome();
            });

            DismissCommand = new Command(async () =>
            {
                playbackService.Dismiss();
                await navigationService.CloseModal();
                await navigationService.NavigateToHome();
            });
        }
    }
}
