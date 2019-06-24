using System;
using Amazon.Runtime;
using NLog;
using NLog.AWS.Logger;
using NLog.Config;

namespace Bible.Alarm.Services.Infrastructure
{
    public class LogSetup
    {
        private static bool initialized = false;
        public static void Initialize()
        {
            if (!initialized)
            {
                var accessKey = "AKIA4L6DRN6IU62TWR34";
                var secretKey = "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";

                var config = new LoggingConfiguration();
                var awsTarget = new AWSTarget()
                {
                    LogGroup = "Bible.Alarm.Android",
                    Region = "us-east-2",
                    Credentials = new BasicAWSCredentials(accessKey, secretKey)
                };
                config.AddTarget("aws", awsTarget);
                config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, awsTarget));

                LogManager.Configuration = config;

                initialized = true;
            }
        }
    }
}
