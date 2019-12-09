using Bible.Alarm.Services.Infrastructure;
using NLog;

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
