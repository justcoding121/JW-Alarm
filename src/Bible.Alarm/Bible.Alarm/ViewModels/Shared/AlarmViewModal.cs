using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services.Contracts;
using Mvvmicro;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bible.Alarm.ViewModels
{
    public class AlarmViewModal : ViewModel, IDisposableModal
    {
        private IContainer container;

        private IPlaybackService playbackService;

        public Command SnoozeCommand { get; private set; }
        public Command DismissCommand { get; private set; }
        public ICommand CancelCommand { get; set; }

        public AlarmViewModal(IContainer container)
        {
            this.container = container;

            this.playbackService = this.container.Resolve<IPlaybackService>();

            SnoozeCommand = new Command(async () =>
            {
                await playbackService.Snooze();

                var navigationService = this.container.Resolve<INavigationService>();
                await navigationService?.CloseModal();
            });

            DismissCommand = new Command(async () =>
            {
                await playbackService.Dismiss();
                var navigationService = this.container.Resolve<INavigationService>();
                await navigationService?.CloseModal();
            });

            CancelCommand = new Command(async () =>
            {
                var navigationService = this.container.Resolve<INavigationService>();
                await navigationService?.GoBack();
            });
        }

        public void Dispose()
        {
            playbackService.Dispose();
        }
    }
}
