using Android.App.Job;
using Android.Content;
using Bible.Alarm.Droid.Services.Helpers;
using Bible.Alarm.Droid.Services.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Droid.Helpers
{
    public class BootstrapHelper
    {
        public async static Task VerifyMediaLookUpService(IContainer container)
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

        public static async Task InitializeDatabase(IContainer container)
        {
            using (var db = container.Resolve<ScheduleDbContext>())
            {
                await db.Database.MigrateAsync();
            }
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

    }
}
