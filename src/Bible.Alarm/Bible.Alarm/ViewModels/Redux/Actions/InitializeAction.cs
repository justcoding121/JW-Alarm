using Bible.Alarm.Common.DataStructures;
using Bible.Alarm.ViewModels;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.ViewModels.Redux.Actions
{
    public class InitializeAction : IAction
    {
        public ObservableHashSet<ScheduleListItem> ScheduleList =
            new ObservableHashSet<ScheduleListItem>();
    }
}
