using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.ViewModels.Redux;
using Bible.Alarm.ViewModels.Redux.Actions;
using Mvvmicro;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bible.Alarm.ViewModels
{
    public class AlarmViewModal : ViewModel, IDisposableModal
    {
        private IPlaybackService playbackService;

        public Command SnoozeCommand { get; private set; }
        public Command DismissCommand { get; private set; }
        public ICommand CancelCommand { get; set; }

        public AlarmViewModal()
        {
            this.playbackService = IocSetup.Container.Resolve<IPlaybackService>();

            SnoozeCommand = new Command(async () =>
            {
                await playbackService.Snooze();

                var navigationService = IocSetup.Container.Resolve<INavigationService>();
                await navigationService?.CloseModal();
            });

            DismissCommand = new Command(async () =>
            {
                await playbackService.Dismiss();
                var navigationService = IocSetup.Container.Resolve<INavigationService>();
                await navigationService?.CloseModal();
            });

            CancelCommand = new Command(async () =>
            {
                var navigationService = IocSetup.Container.Resolve<INavigationService>();
                await navigationService?.GoBack();
            });
        }

        public void Dispose()
        {
            playbackService.Dispose();
        }
    }
}
