using JW.Alarm.Common.DataStructures;
using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace JW.Alarm.ViewModels.Redux
{
    public class ApplicationState
    {
        public ObservableHashSet<ScheduleListItem> Schedules { get; set; }
        public ScheduleViewModel Current { get; set; } 
    }
}
