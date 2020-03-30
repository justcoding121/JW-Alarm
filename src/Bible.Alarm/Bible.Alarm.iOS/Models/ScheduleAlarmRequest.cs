using System;

namespace Bible.Alarm.iOS.Models
{
    public class ScheduleAlarmRequest
    {
        public string DeviceId { get; set; }
        public DateTime AlarmTime { get; set; }
        public long AlarmId { get; set; }
    }
}