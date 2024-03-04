using System;

namespace Bible.Alarm.iOS.Models
{
    public class ScheduleAlarmRequest : DeviceRequest
    {
        public DateTime AlarmTime { get; set; }
        public long NotificationId { get; set; }
    }
}