using Bible.Alarm.Services.Contracts;
using Bible.Alarm.ViewModels.Redux.Actions;
using Bible.Alarm.ViewModels.Redux.Actions.Music;
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
    public class MusicSelectionViewModel : ViewModel, IDisposable
    {
        private AlarmMusic current;
        private readonly IThreadService threadService;
        private readonly MediaService mediaService;
        private readonly INavigationService navigationService;

        private List<IDisposable> disposables = new List<IDisposable>();

        public MusicSelectionViewModel()
        {
            this.threadService = IocSetup.Container.Resolve<IThreadService>();
            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.navigationService = IocSetup.Container.Resolve<INavigationService>();

            disposables.Add(mediaService);

            //set schedules from initial state.
            //this should fire only once 
            var subscription = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
                .Select(state => state.CurrentMusic)
                .Where(x => x != null)
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    current = x;
                    setSelectedMusicType();
                });

            disposables.Add(subscription);

            SongBookSelectionCommand = new Command<MusicTypeListItemViewModel>(async x =>
            {
                if (x.MusicType == MusicType.Vocals)
                {
                    ReduxContainer.Store.Dispatch(new SongBookSelectionAction()
                    {
                        TentativeMusic = new AlarmMusic()
                        {
                            MusicType = MusicType.Vocals,
                            LanguageCode = current.LanguageCode
                        }
                    });
                    var viewModel = IocSetup.Container.Resolve<SongBookSelectionViewModel>();
                    await navigationService.Navigate(viewModel);
                }
                else
                {
                    ReduxContainer.Store.Dispatch(new TrackSelectionAction()
                    {
                        TentativeMusic = new AlarmMusic()
                        {
                            Fixed = current.Fixed,
                            MusicType = MusicType.Melodies,
                            PublicationCode = "iam"
                        }
                    });
                    var viewModel = IocSetup.Container.Resolve<TrackSelectionViewModel>();
                    await navigationService.Navigate(viewModel);
                }


            });

            BackCommand = new Command(async () =>
            {
                await navigationService.GoBack();
                ReduxContainer.Store.Dispatch(new BackAction(this));
            });

            navigationService.NavigatedBack += onNavigated;
        }

        private void onNavigated(object viewModal)
        {
            if (viewModal.GetType() == this.GetType())
            {
                setSelectedMusicType();
            }
        }

        private void setSelectedMusicType()
        {
            var selected = MusicTypes.First(y => y.MusicType == current.MusicType);
            selectedMusicType = selected;
            Raise("SelectedMusicType");
        }

        public ICommand BackCommand { get; set; }
        public ICommand SongBookSelectionCommand { get; set; }

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

        private MusicTypeListItemViewModel selectedMusicType;
        public MusicTypeListItemViewModel SelectedMusicType
        {
            get => selectedMusicType;
            set
            {
                //this is a hack since selection is not working in one-way mode 
                //make two-way mode behave like one way mode
                Raise();
            }
        }

        public void Dispose()
        {
            navigationService.NavigatedBack -= onNavigated;
            disposables.ForEach(x => x.Dispose());
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
