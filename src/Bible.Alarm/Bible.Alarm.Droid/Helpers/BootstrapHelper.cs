using Android.App;
using Android.App.Job;
using Android.Content;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Droid;
using Bible.Alarm.Droid.Services.Helpers;
using Bible.Alarm.Droid.Services.Tasks;
using Bible.Alarm.Services.Droid.Tasks;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Droid.Helpers
{
    public class BootstrapHelper
    {
        public static IContainer InitializeService(Context context)
        {
            var result = Alarm.Droid.IocSetup.Initialize(context, true);

            var containerCreated = result.Item2;
            if (containerCreated)
            {
                var application = (Application)context.ApplicationContext;
                Xamarin.Essentials.Platform.Init(application);
            }

            return result.Item1;
        }

        public static IContainer InitializeUI(Logger logger, Context context, Application application)
        {
            var result = Alarm.Droid.IocSetup.Initialize(context, false);
            var container = result.Item1;

            var containerCreated = result.Item2;
            if (containerCreated)
            {
                Xamarin.Essentials.Platform.Init(application);

                Task.Run(async () =>
                {
                    try
                    {
                        await VerifyServices(container);
                        Messenger<bool>.Publish(MvvmMessages.Initialized, true);

                    }
                    catch (Exception e)
                    {
                        logger.Fatal(e, "Android initialization crashed.");
                        throw;
                    }
                });

            }
            else
            {
                Task.Run(() =>
                {
                    try
                    {
                        Messenger<bool>.Publish(MvvmMessages.Initialized, true);
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "An error happened bootstrap helper Messenger publish.");
                    }
                });
            }

            return container;
        }

        public async static Task VerifyServices(IContainer container)
        {
            var task1 = BootstrapHelper.verifyMediaLookUp(container);
            var task2 = BootstrapHelper.initializeDatabase(container);

            await Task.WhenAll(task1, task2);
        }

        internal static void Remove(Context context)
        {
            Alarm.Droid.IocSetup.Remove(context);
        }

        private async static Task verifyMediaLookUp(IContainer container)
        {
            using (var service = container.Resolve<MediaIndexService>())
            {
                await service.Verify();
            }
        }

        public static void VerifyBackgroundTasks(Context context)
        {
            schedulerSetupTask(context);
        }

        private static bool schedulerSetupTask(Context context)
        {
            // Sample usage - creates a JobBuilder for a SchedulerJob and sets the Job ID to 1.
            using (var jobBuilder = context.CreateJobBuilderUsingJobId<SchedulerJob>(SchedulerJob.JobId, 30))
            {
                var jobInfo = jobBuilder.Build();  // creates a JobInfo object.

                var jobScheduler = (JobScheduler)context.GetSystemService(Context.JobSchedulerService);
                var scheduleResult = jobScheduler.Schedule(jobInfo);

                if (JobScheduler.ResultSuccess == scheduleResult)
                {
                    return true;
                }

                return false;
            }
        }

        private static async Task initializeDatabase(IContainer container)
        {
            using (var db = container.Resolve<ScheduleDbContext>())
            {
                await db.Database.MigrateAsync();
            }
        }

    }
}
