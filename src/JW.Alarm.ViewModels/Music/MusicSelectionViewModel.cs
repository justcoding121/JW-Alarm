using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using JW.Alarm.Services;
using JW.Alarm.Services.Contracts;
using Mvvmicro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.ViewModels
{
    public class MusicSelectionViewModel : ViewModelBase
    {
        private readonly AlarmMusic model;
        public MusicSelectionViewModel(AlarmMusic model)
        {
            this.model = model;
            this.musicType = model.MusicType;
        }

        public HashSet<MusicType> MusicTypes = new HashSet<MusicType>(new[] { MusicType.Melodies, MusicType.Vocals });

        public void Navigated()
        {
            MusicType = model.MusicType;
        }


        private MusicType musicType;
        public MusicType MusicType
        {
            get => musicType;
            set => this.Set(ref musicType, value);
        }

        
    }
}
