using Bible.Alarm.Models;
using Bible.Alarm.ViewModels;
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
