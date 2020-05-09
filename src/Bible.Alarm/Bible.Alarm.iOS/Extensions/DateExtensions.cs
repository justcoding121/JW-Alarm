using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Bible.Alarm.iOS.Extensions
{
    public static class DateExtensions
    {
        public static NSDateComponents ToNSDateComponents(this DateTime date)
        {
            return new NSDateComponents()
            {
                Hour = date.Hour,
                Minute = date.Minute,
                Second = date.Second
            };
        }
    }
}