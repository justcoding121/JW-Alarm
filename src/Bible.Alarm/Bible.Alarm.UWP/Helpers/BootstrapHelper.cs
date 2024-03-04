﻿using Bible.Alarm.Common.Helpers;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services.Tasks;
using NLog;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Bible.Alarm.Services.Uwp.Helpers
{
    public class BootstrapHelper
    {
        public static bool IsBackgroundTaskEnabled = true;

        public static async Task SetupBackgroundTask(IContainer container)
        {
            await BackgroundExecutionManager.RequestAccessAsync();
            var allowed = BackgroundExecutionManager.GetAccessStatus();
            switch (allowed)
            {
                case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:
                case BackgroundAccessStatus.AlwaysAllowed:
                    break;
                case BackgroundAccessStatus.Unspecified:
                case BackgroundAccessStatus.DeniedBySystemPolicy:
                case BackgroundAccessStatus.DeniedByUser:
                    IsBackgroundTaskEnabled = false;
                    break;
            }

            registerSchedulerTask();
            registerMediaIndexUpdateTask();
        }

        private static void registerMediaIndexUpdateTask()
        {
            var registered = false;

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == "MediaIndexUpdateTask")
                {
                    registered = true;
                }
            }

            if (!registered)
            {
                var builder = new BackgroundTaskBuilder
                {
                    Name = "MediaIndexUpdateTask"
                };

                builder.SetTrigger(new TimeTrigger(60, true));
                builder.Register();
            }
        }

        private static void registerSchedulerTask()
        {
            var registered = false;

            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == "SchedulerTask")
                {
                    registered = true;
                }
            }

            if (!registered)
            {
                var builder = new BackgroundTaskBuilder
                {
                    Name = "SchedulerTask"
                };

                builder.SetTrigger(new TimeTrigger(15, true));
                builder.Register();
            }
        }

        public static void Initialize(IContainer container, Logger logger)
        {
            Task.Run(() => SetupBackgroundTask(container));

            Task.Run(async () =>
            {
                try
                {
                    await CommonBootstrapHelper.VerifyServices(container);

                    Messenger<bool>.Publish(MvvmMessages.Initialized, true);

                    await Task.Delay(1000);

                    await container.Resolve<SchedulerTask>().Handle();
                }
                catch (Exception e)
                {
                    logger.Fatal(e, "Uwp initialization crashed.");
                }
            });
        }
    }
}
