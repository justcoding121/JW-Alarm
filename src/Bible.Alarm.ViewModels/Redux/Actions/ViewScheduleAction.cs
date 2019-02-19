using JW.Alarm.ViewModels;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.ViewModels.Redux.Actions
{
    public class ViewScheduleAction : IAction
    {
        public ScheduleViewModel ScheduleViewModel { get; set; }
        public ScheduleListItem SelectedScheduleListItem { get; set; }
    }
}
