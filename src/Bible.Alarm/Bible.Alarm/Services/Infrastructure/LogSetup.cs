using Loggly;
using Loggly.Config;
using NLog;
using NLog.Config;
using Xamarin.Forms;

namespace Bible.Alarm.Services.Infrastructure
{
    public class LogSetup
    {
        private static bool initialized = false;

        private static object @lock = new object();
        public static void Initialize(string osName)
        {
            lock (@lock)
            {
                if (!initialized)
                {
                    setupLoggly(osName);

                    var config = new LoggingConfiguration();
                    var logglyTarget = new NLog.Targets.LogglyTarget();
                    config.AddTarget("loggly", logglyTarget);
                    config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, logglyTarget));

                    LogManager.Configuration = config;

                    initialized = true;
                }
            }
        }

        private static void setupLoggly(string osName)
        {

            var config = LogglyConfig.Instance;
            config.CustomerToken = "ca6fc8af-4beb-4548-8b6c-c5955a288cc6";
            config.ApplicationName = $"Bible-Alarm-{osName}";

            config.Transport.EndpointHostname = "logs-01.loggly.com";
            config.Transport.EndpointPort = 514;
            config.Transport.LogTransport = LogTransport.SyslogUdp;

            var ct = new ApplicationNameTag();
            ct.Formatter = "application-{0}";
            config.TagConfig.Tags.Add(ct);
        }
    }
}
