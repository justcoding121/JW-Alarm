using Bible.Alarm.Services.Contracts;
using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using JW.Alarm.Services;
using JW.Alarm.Services.Contracts;
using JW.Alarm.ViewModels.Redux;
using Mvvmicro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace JW.Alarm.ViewModels
{
    public class MusicSelectionViewModel : ViewModel
    {
        private AlarmMusic current;
        private readonly IThreadService threadService;
        private readonly MediaService mediaService;
        private readonly INavigationService navigationService;

        public MusicSelectionViewModel()
        {
            this.threadService = IocSetup.Container.Resolve<IThreadService>();
            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.navigationService = IocSetup.Container.Resolve<INavigationService>();

            //set schedules from initial state.
            //this should fire only once (look at the where condition).
            ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
               .DistinctUntilChanged(state => state.CurrentMusic)
               .Subscribe(x =>
               {
                   current = x.CurrentMusic;
                   Task.Run(() => initializeAsync());
               });

            SongBookSelectionCommand = new Command<MusicTypeListItemViewModel>(async x =>
            {
                var viewModel = IocSetup.Container.Resolve<SongBookSelectionViewModel>();
                //ReduxContainer.Store.Dispatch(new ViewScheduleAction()
                //{
                //    ScheduleViewModel = viewModel,
                //    SelectedScheduleListItem = x
                //});
                await navigationService.Navigate(viewModel);
            });

            BackCommand = new Command(async () =>
            {
                await navigationService.GoBack();
            });

        }

        public ICommand BackCommand { get; set; }
        public ICommand SongBookSelectionCommand { get; set; }

        private async Task initializeAsync()
        {
            var selected = MusicTypes.First(x => x.MusicType == current.MusicType);
            await threadService.RunOnUIThread(() => SelectedMusicType = selected);
        }

        public ObservableCollection<MusicTypeListItemViewModel> MusicTypes { get; set; }
            = new ObservableCollection<MusicTypeListItemViewModel>(new List<MusicTypeListItemViewModel> {
                new MusicTypeListItemViewModel()
                {
                    MusicType = MusicType.Melodies,
                    Name = "Melodies"
                },
                new MusicTypeListItemViewModel()
                {
                    MusicType = MusicType.Vocals,
                    Name = "Vocals"
                }
            });

        public object GetBookSelectionViewModel(MusicTypeListItemViewModel musicTypeListItemViewModel)
        {
            if (musicTypeListItemViewModel.MusicType == MusicType.Vocals)
            {
                return new SongBookSelectionViewModel(current, new AlarmMusic()
                {
                    MusicType = MusicType.Vocals,
                    LanguageCode = current.MusicType == MusicType.Vocals ? current.LanguageCode : null
                });
            }

            return new TrackSelectionViewModel(current, new AlarmMusic()
            {
                Fixed = current.Fixed,
                MusicType = MusicType.Melodies
            });
        }

        private MusicTypeListItemViewModel selectedMusicType;
        public MusicTypeListItemViewModel SelectedMusicType
        {
            get => selectedMusicType;
            set => this.Set(ref selectedMusicType, value);
        }
    }

    public class MusicTypeListItemViewModel : IComparable
    {
        public MusicType MusicType { get; set; }
        public string Name { get; set; }

        public int CompareTo(object obj)
        {
            return Name.CompareTo((obj as MusicTypeListItemViewModel).Name);
        }
    }
}
