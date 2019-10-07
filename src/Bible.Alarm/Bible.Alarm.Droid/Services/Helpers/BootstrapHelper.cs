using Android.App.Job;
using Android.Content;
using Bible.Alarm.Droid.Services.Helpers;
using Bible.Alarm.Droid.Services.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Droid.Helpers
{
    public class BootstrapHelper
    {
        public static void VerifyPermissions()
        {
            throw new NotImplementedException();
        }

        public async static Task VerifyMediaLookUpService()
        {
            var service = IocSetup.Container.Resolve<MediaIndexService>();
            await service.Verify();
        }

        public static void VerifyBackgroundTasks(Context context)
        {
            schedulerSetupTask(context);
        }

        public static async Task InitializeDatabase()
        {
            using (var db = IocSetup.Container.Resolve<ScheduleDbContext>())
            {
                await db.Database.MigrateAsync();
            }
        }

        private static bool schedulerSetupTask(Context context)
        {
            // Sample usage - creates a JobBuilder for a SchedulerJob and sets the Job ID to 1.
            var jobBuilder = context.CreateJobBuilderUsingJobId<SchedulerJob>(SchedulerJob.JobId, 15);

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
}
