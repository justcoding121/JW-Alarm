using JW.Alarm.Models;
using JW.Alarm.ViewModels;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.ViewModels.Redux.Actions.Music
{
    public class TrackSelectionAction : IAction
    {
        public AlarmMusic TentativeMusic { get; set; }
    }
}
