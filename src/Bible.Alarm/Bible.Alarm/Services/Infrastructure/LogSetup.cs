using Bible.Alarm.Contracts.Platform;
using Loggly;
using Loggly.Config;
using NLog;
using NLog.Config;

namespace Bible.Alarm.Services.Infrastructure
{
    public class LogSetup
    {
        private static bool initialized = false;
        private static object @lock = new object();
        public static void Initialize(IVersionFinder versionFinder, string[] tags)
        {
            lock (@lock)
            {
                if (!initialized)
                {
                    setupLoggly();

                    var config = new LoggingConfiguration();
                    var logglyTarget = new NLog.Targets.LogglyTarget();
                    logglyTarget.Tags.Add(new NLog.Targets.LogglyTagProperty()
                    {
                        Name = getVersionName(versionFinder)
                    });

                    if (tags != null)
                    {
                        foreach (var tag in tags)
                        {
                            logglyTarget.Tags.Add(new NLog.Targets.LogglyTagProperty()
                            {
                                Name = tag
                            });
                        }

                    }

                    config.AddTarget("loggly", logglyTarget);
                    config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, logglyTarget));

                    LogManager.Configuration = config;

                    initialized = true;
                }
            }
        }

        private static void setupLoggly()
        {

            var config = LogglyConfig.Instance;
            config.CustomerToken = "ca6fc8af-4beb-4548-8b6c-c5955a288cc6";
            config.ApplicationName = $"Bible-Alarm";

            config.Transport.EndpointHostname = "logs-01.loggly.com";
            config.Transport.EndpointPort = 514;
            config.Transport.LogTransport = LogTransport.SyslogUdp;

            var ct = new ApplicationNameTag();
            ct.Formatter = "application-{0}";
            config.TagConfig.Tags.Add(ct);
        }

        private static string getVersionName(IVersionFinder versionFinder)
        {
            try
            {
                return versionFinder.GetVersionName();
            }
            catch
            {
                return "AssemblyVersionNotFound";
            }
        }

    }
}