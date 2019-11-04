using System;
using System.Threading.Tasks;
using Android.App;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.Services.Droid.Tasks;
using NLog;
using Android.App.Job;

namespace Bible.Alarm.Droid.Services.Tasks
{
    [Service(Name = "com.jthomas.info.Bible.Alarm.jobscheduler.SchedulerJob",
         Permission = "android.permission.BIND_JOB_SERVICE")]
    public class SchedulerJob : JobService
    {
        public const int JobId = 1;
        private static Logger logger => LogHelper.GetLogger(global::Xamarin.Forms.Forms.IsInitialized);

        public override bool OnStartJob(JobParameters jobParams)
        {

            Task.Run(async () =>
            {
                try
                {
                    IocSetup.Initialize(this, true);

                    var schedulerTask = IocSetup.Container.Resolve<SchedulerTask>();
                    await schedulerTask.Handle();
                }
                catch (Exception e)
                {
                    logger.Error(e, "An error happened when creating the repeating scheduler task.");
                }


                // Have to tell the JobScheduler the work is done. 
                JobFinished(jobParams, false);
            });

            // Return true because of the asynchronous work
            return true;
        }

        public override bool OnStopJob(JobParameters jobParams)
        {
            // we don't want to reschedule the job if it is stopped or cancelled.
            return false;
        }
    }
}