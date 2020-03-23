using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Bible.Alarm.iOS.Models
{
    public class RegisterDeviceRequest
    {
        public string DeviceId { get; set; }
        public string DeviceToken { get; set; }
    }
}