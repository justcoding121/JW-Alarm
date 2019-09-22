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

        private readonly MediaService mediaService;
        private readonly INavigationService navigationService;

        private List<IDisposable> disposables = new List<IDisposable>();

        public MusicSelectionViewModel()
        {
            this.mediaService = IocSetup.Container.Resolve<MediaService>();
            this.navigationService = IocSetup.Container.Resolve<INavigationService>();

            disposables.Add(mediaService);

            //set schedules from initial state.
            //this should fire only once 
            var subscription1 = ReduxContainer.Store.ObserveOn(Scheduler.CurrentThread)
                .Select(state => state.CurrentMusic)
                .Where(x => x != null)
                .DistinctUntilChanged()
                .Subscribe(x =>
                {
                    current = x;
                    setSelectedMusicType();
                    IsBusy = false;
                });

            disposables.Add(subscription1);


            SongBookSelectionCommand = new Command<MusicTypeListItemViewModel>(async x =>
            {
                IsBusy = true;

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
                            Repeat = current.Repeat,
                            MusicType = MusicType.Melodies,
                            PublicationCode = "iam"
                        }
                    });
                    var viewModel = IocSetup.Container.Resolve<TrackSelectionViewModel>();
                    await navigationService.Navigate(viewModel);
                }

                IsBusy = false;

            });

            BackCommand = new Command(async () =>
            {
                IsBusy = true;
                await navigationService.GoBack();
                ReduxContainer.Store.Dispatch(new BackAction(this));
                IsBusy = false;
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
            if (SelectedMusicType != null)
            {
                SelectedMusicType.IsSelected = false;
            }

            SelectedMusicType = MusicTypes.First(y => y.MusicType == current.MusicType);
            SelectedMusicType.IsSelected = true;
        }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set => this.Set(ref isBusy, value);
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
            set => this.Set(ref selectedMusicType, value);
        }

        public void Dispose()
        {
            navigationService.NavigatedBack -= onNavigated;
            disposables.ForEach(x => x.Dispose());
        }
    }

    public class MusicTypeListItemViewModel : ViewModel, IComparable
    {
        public MusicType MusicType { get; set; }
        public string Name { get; set; }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set => this.Set(ref isSelected, value);
        }

        public int CompareTo(object obj)
        {
            return Name.CompareTo((obj as MusicTypeListItemViewModel).Name);
        }
    }
}
