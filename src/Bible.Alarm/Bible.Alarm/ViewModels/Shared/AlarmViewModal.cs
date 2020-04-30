using Bible.Alarm.Common.Extensions;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services.Contracts;
using MediaManager;
using Mvvmicro;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bible.Alarm.ViewModels
{
    public class AlarmViewModal : ViewModel, IDisposableModal
    {
        private readonly IContainer container;
        private readonly IMediaManager mediaManager;
        private readonly IPlaybackService playbackService;

        private bool isDisposed = false;

        public Command SnoozeCommand { get; private set; }
        public Command DismissCommand { get; private set; }
        public ICommand CancelCommand { get; set; }

        public Command PlayCommand { get; set; }
        public Command PauseCommand { get; set; }
        public Command PreviousCommand { get; set; }
        public Command NextCommand { get; set; }
        public Command ForwardCommand { get; set; }
        public Command BackwardCommand { get; set; }

        public AlarmViewModal(IContainer container)
        {
            this.container = container;

            this.playbackService = this.container.Resolve<IPlaybackService>();
            this.mediaManager = this.container.Resolve<IMediaManager>();

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

            PlayCommand = new Command(() =>
            {
                mediaManager.Play();
                refresh();
            });

            PauseCommand = new Command(() =>
            {
                mediaManager.Pause();
                refresh();
            });

            PreviousCommand = new Command(async () =>
            {
                await mediaManager.PlayPrevious();
                refresh();
            });

            NextCommand = new Command(async () =>
            {
                await mediaManager.PlayNext();
                refresh();
            });

            ForwardCommand = new Command(async () =>
            {
                await mediaManager.StepForward();
                refresh();
            });

            BackwardCommand = new Command(async () =>
            {
                await mediaManager.StepBackward();
                refresh();
            });

            Task.Run(async () =>
            {
                while (!isDisposed)
                {
                    refresh();
                    await Task.Delay(1000);
                    if (!mediaManager.IsPreparedEx())
                    {
                        Dispose();
                    }
                }
            });
        }

        private void refresh()
        {
            try
            {
                var mediaItem = mediaManager.Queue.Current;

                Title = mediaItem.DisplayTitle;
                SubTitle = mediaItem.DisplaySubtitle;
                Description = mediaItem.DisplayDescription;

                if (mediaManager.IsPlaying())
                {
                    PlayVisible = false;
                    PauseVisible = true;
                }
                else
                {
                    PlayVisible = true;
                    PauseVisible = false;
                }

                CurrentTime = $"{mediaManager.Position.Minutes:00}:{mediaManager.Position.Seconds:00}";
                EndTime = $"{mediaManager.Duration.Minutes:00}:{mediaManager.Duration.Seconds:00}";
                Progress = mediaManager.Position.TotalMilliseconds / mediaManager.Duration.TotalMilliseconds;
            }
            catch { }
        }

        private string title;
        public string Title
        {
            get => title;
            set => this.Set(ref title, value);
        }

        private string subTitle;
        public string SubTitle
        {
            get => subTitle;
            set => this.Set(ref subTitle, value);
        }

        private string description;
        public string Description
        {
            get => description;
            set => this.Set(ref description, value);
        }

        private bool playVisible;
        public bool PlayVisible
        {
            get => playVisible;
            set => this.Set(ref playVisible, value);
        }
        private bool pauseVisible;
        public bool PauseVisible
        {
            get => pauseVisible;
            set => this.Set(ref pauseVisible, value);
        }
        private string currentTime;
        public string CurrentTime
        {
            get => currentTime;
            set => this.Set(ref currentTime, value);
        }
        private string endTime;
        public string EndTime
        {
            get => endTime;
            set => this.Set(ref endTime, value);
        }
        private double progress;
        public double Progress
        {
            get => progress;
            set => this.Set(ref progress, value);
        }



        public void Dispose()
        {
            isDisposed = true;
            playbackService.Dispose();
        }
    }
}
