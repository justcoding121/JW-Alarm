using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.App.Job;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Bible.Alarm.Droid.Services.Helpers
{
    public static class JobSchedulerHelper
    {
        public static JobInfo.Builder CreateJobBuilderUsingJobId<T>(this Context context, int jobId, int intervalMinutes) where T : JobService
        {
            var javaClass = Java.Lang.Class.FromType(typeof(T));
            var componentName = new ComponentName(context, javaClass);
            var builder = new JobInfo.Builder(jobId, componentName);
            builder.SetRequiredNetworkType(NetworkType.Any);
            builder.SetRequiresBatteryNotLow(true);
            //builder.SetRequiresDeviceIdle(true);
            builder.SetPeriodic(1000 * 60 * intervalMinutes);

            return builder;
        }
    }

}