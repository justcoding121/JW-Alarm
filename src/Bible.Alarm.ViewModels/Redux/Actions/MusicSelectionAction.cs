using JW.Alarm.Models;
using JW.Alarm.ViewModels;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.ViewModels.Redux.Actions
{
    public class MusicSelectionAction : IAction
    {
        public MusicSelectionViewModel MusicSelectionViewModel { get; set; }
        public AlarmMusic CurrentMusic { get; set; }
    }
}
