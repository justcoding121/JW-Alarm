using JW.Alarm.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.ViewModels.Redux
{
    public class ApplicationState
    {
        public HashSet<AlarmSchedule> Schedules { get; set; }
        public AlarmSchedule Current { get; set; }
    }
}
