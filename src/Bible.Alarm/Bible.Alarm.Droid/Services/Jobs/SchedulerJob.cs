﻿using System;
using System.Threading.Tasks;
using Android.App;
using Bible.Alarm.Services.Droid.Tasks;
using NLog;
using Android.App.Job;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Infrastructure;
using Bible.Alarm.Droid.Services.Platform;

namespace Bible.Alarm.Droid.Services.Tasks
{
    [Service(Name = "com.jthomas.info.Bible.Alarm.jobscheduler.SchedulerJob",
         Permission = "android.permission.BIND_JOB_SERVICE")]
    public class SchedulerJob : JobService
    {
        public const int JobId = 1;
        private IContainer container;
        private Logger logger;
        public SchedulerJob()
        {
            LogSetup.Initialize(VersionFinder.Default);
            logger = LogManager.GetCurrentClassLogger();
        }
        public override bool OnStartJob(JobParameters jobParams)
        {

            Task.Run(async () =>
            {
                try
                {
                    var result = IocSetup.Initialize(this, true);
                    this.container = result.Item1;

                    var task1 = BootstrapHelper.VerifyMediaLookUpService(container);
                    var task2 = BootstrapHelper.InitializeDatabase(container);

                    await Task.WhenAll(task1, task2);

                    using (var schedulerTask = container.Resolve<SchedulerTask>())
                    {
                        await schedulerTask.Handle();
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "An error happened when handliing the repeating scheduler task.");
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