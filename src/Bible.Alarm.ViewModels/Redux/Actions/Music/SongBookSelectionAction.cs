using JW.Alarm.Models;
using JW.Alarm.ViewModels;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.ViewModels.Redux.Actions
{
    public class SongBookSelectionAction : IAction
    {
        public SongBookSelectionViewModel SongBookSelectionViewModel { get; set; }
        public AlarmMusic CurrentMusic { get; set; }
        public AlarmMusic TentativeMusic { get; set; }
    }
}
