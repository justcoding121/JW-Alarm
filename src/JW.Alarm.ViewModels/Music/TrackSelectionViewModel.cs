using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.ViewModels
{
    public class TrackSelectionViewModel
    {
        private Music current;
        public TrackSelectionViewModel(Music current, AlarmMusic music)
        {
            this.current = current;
        }
    }
}
