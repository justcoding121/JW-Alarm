using Bible.Alarm.Models;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.ViewModels.Redux.Actions.Bible
{
    public class ChapterSelectedAction : IAction
    {
        public BibleReadingSchedule CurrentBibleReadingSchedule { get; set; }
    }
}
