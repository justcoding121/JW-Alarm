using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Bible.Alarm.iOS.Models
{
    public class ScheduleAlarmRequest 
    {
        public string DeviceId { get; set; }
        public DateTime AlarmTime { get; set; }
        public long AlarmId { get; set; }
    }
}