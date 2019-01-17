using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using JW.Alarm.Services;
using JW.Alarm.Services.Contracts;
using Mvvmicro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.ViewModels
{
    public class MusicSelectionViewModel : ViewModelBase
    {
        private readonly AlarmMusic current;
        private readonly IThreadService threadService;

        public MusicSelectionViewModel(AlarmMusic current)
        {
            this.current = current;
            this.threadService = IocSetup.Container.Resolve<IThreadService>();

            refresh();
        }

        private void refresh()
        {
            Task.Run(() => initializeAsync());
        }

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

        public SongBookSelectionViewModel GetBookSelectionViewModel(MusicTypeListItemViewModel musicTypeListItemViewModel)
        {
            return new SongBookSelectionViewModel(this.current, new AlarmMusic()
            {
                Fixed = current.Fixed,
                LanguageCode = current.LanguageCode,
                MusicType = current.MusicType,
                PublicationCode = current.PublicationCode,
                TrackNumber = current.TrackNumber
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
