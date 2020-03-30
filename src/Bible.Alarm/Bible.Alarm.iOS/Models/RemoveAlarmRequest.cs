using System;
namespace Bible.Alarm.iOS.Models
{
    public class RemoveAlarmRequest : DeviceRequest
    {
        public long NotificationId { get; set; }
    }
}
