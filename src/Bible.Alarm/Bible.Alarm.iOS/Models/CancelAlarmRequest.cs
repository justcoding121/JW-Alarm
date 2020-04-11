using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Bible.Alarm.iOS.Models
{
    public class CancelAlarmRequest
    {
        public string DeviceId { get; set; }
        public string DeviceToken { get; set; }
        public long NotificationId { get; set; }
    }
}