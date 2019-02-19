using JW.Alarm.Models;
using JW.Alarm.ViewModels;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.ViewModels.Redux.Actions
{
    public class AddScheduleAction : IAction
    {
        public ScheduleListItem ScheduleListItem { get; set; }
    }
}
