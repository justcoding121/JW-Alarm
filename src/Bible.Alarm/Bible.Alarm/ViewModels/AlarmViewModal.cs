using Bible.Alarm.Services.Contracts;
using Mvvmicro;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bible.Alarm.ViewModels
{
    public class AlarmViewModal : ViewModel, IDisposable
    {
        private IPlaybackService playbackService;
        private INavigationService navigationService;

        public Command SnoozeCommand { get; private set; }
        public Command DismissCommand { get; private set; }
        public ICommand CancelCommand { get; set; }

        public AlarmViewModal()
        {
            this.playbackService = IocSetup.Container.Resolve<IPlaybackService>();
            this.navigationService = IocSetup.Container.Resolve<INavigationService>();

            SnoozeCommand = new Command(async () =>
            {
                await playbackService.Snooze();
                await navigationService.CloseModal();
            });

            DismissCommand = new Command(async () =>
            {
                await playbackService.Dismiss();
                await navigationService.CloseModal();
            });

            CancelCommand = new Command(async () =>
            {
                await navigationService.GoBack();
            });
        }

        public void Dispose()
        {
            
        }
    }
}
