using Bible.Alarm.Services.Infrastructure;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm
{
    public static class LogHelper
    {
        public static Logger GetLogger(bool isXamarinFormsInitialized)
        {
            LogSetup.Initialize(isXamarinFormsInitialized ? Xamarin.Forms.Device.RuntimePlatform : "Unknown");
            return LogManager.GetLogger("GlobalLogger");
        }
    }
}
