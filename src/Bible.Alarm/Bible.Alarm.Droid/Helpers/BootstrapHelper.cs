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
            var container = result.Item1;

            return container;
        }

        public static IContainer InitializeUI(Activity mainActivity, Logger logger, Application application)
        {
            var result = Alarm.Droid.IocSetup.InitializeWithContainerName("MainActivity", Application.Context, false);
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

                        await Messenger<bool>.Publish(MvvmMessages.Initialized, true);

                        await Task.Delay(1000);

                        var i = new Intent(container.AndroidContext(), typeof(AlarmSetupService));
                        i.PutExtra("Action", "SetupBackgroundTasks");
                        mainActivity.StartService(i);

                    }
                    catch (Exception e)
                    {
                        logger.Fatal(e, "Android initialization crashed.");
                    }
                });

            }
            else
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await Messenger<bool>.Publish(MvvmMessages.Initialized, true);
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
