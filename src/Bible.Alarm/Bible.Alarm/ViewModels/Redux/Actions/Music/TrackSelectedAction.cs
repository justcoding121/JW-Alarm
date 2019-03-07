using JW.Alarm.Models;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.ViewModels.Redux.Actions.Music
{
    public class TrackSelectedAction : IAction
    {
        public AlarmMusic CurrentMusic { get; set; }
    }
}
