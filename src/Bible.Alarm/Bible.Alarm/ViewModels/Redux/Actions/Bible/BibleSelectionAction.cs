using JW.Alarm.Models;
using JW.Alarm.ViewModels;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.ViewModels.Redux.Actions
{
    public class BibleSelectionAction : IAction
    {
        public BibleReadingSchedule CurrentBibleReadingSchedule { get; set; }
        public BibleReadingSchedule TentativeBibleReadingSchedule { get; set; }
    }
}
