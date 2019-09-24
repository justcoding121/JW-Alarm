using Bible.Alarm.Models;
using Bible.Alarm.ViewModels;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.ViewModels.Redux.Actions
{
    public class SongBookSelectionAction : IAction
    {
        public AlarmMusic TentativeMusic { get; set; }
    }
}
