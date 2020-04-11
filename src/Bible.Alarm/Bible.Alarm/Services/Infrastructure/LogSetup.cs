using Bible.Alarm.Contracts.Platform;
using NLog;
using NLog.AWS.Logger;
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
#if DEBUG
                    LogManager.ThrowConfigExceptions = true;
                    LogManager.ThrowExceptions = true;
#endif

                    var config = new LoggingConfiguration();

                    string accessKey = "AKIA4L6DRN6IWENWSZU6";
                    string secretKey = "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";

                    var awsTarget = new AWSTarget()
                    {
                        LogGroup = "bible-alarm-iOS",
                        Region = "us-east-1",
                        Credentials = new Amazon.Runtime.BasicAWSCredentials(accessKey, secretKey)
                    };
                   
                    GlobalDiagnosticsContext.Set("Version", getVersionName(versionFinder));
                    if (tags != null)
                    {
                        GlobalDiagnosticsContext.Set("Tags", string.Join(",", tags));
                    }

                    config.AddTarget("aws", awsTarget);
                    config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, awsTarget));


                    LogManager.Configuration = config;

                    initialized = true;
                }
            }
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
