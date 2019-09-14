using Android.App;
using Android.Content;
using Bible.Alarm.Droid.Services.Tasks;
using JW.Alarm.Models;
using JW.Alarm.Services.Droid.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JW.Alarm.Services.Droid.Helpers
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
            if (!IsScheduleTaskScheduled(context))
            {
                schedulerSetupTask(context);
            }
        }

        public static async Task InitializeDatabase()
        {
            using (var db = IocSetup.Container.Resolve<ScheduleDbContext>())
            {

                await db.Database.MigrateAsync();
            }
        }

        private static void schedulerSetupTask(Context context)
        {
            var intent = new Intent(context, typeof(SchedulerReceiver));

            var pIntent = PendingIntent.GetBroadcast(
                    context,
                    0,
                    intent,
                    PendingIntentFlags.UpdateCurrent);

            var alarmService = (AlarmManager)context.GetSystemService(Context.AlarmService);

            var interval = 30 * 1000 * 15;
            var firstTrigger = Java.Lang.JavaSystem.CurrentTimeMillis();

            alarmService.SetRepeating(AlarmType.RtcWakeup, firstTrigger, interval, pIntent);
        }

        private static bool IsScheduleTaskScheduled(Context context)
        {
            var pIntent = findIntent(context);
            return pIntent != null;
        }

        private static PendingIntent findIntent(Context context)
        {
            var intent = new Intent(context, typeof(SchedulerReceiver));

            var pIntent = PendingIntent.GetBroadcast(
                    context,
                    0,
                    intent,
                    PendingIntentFlags.NoCreate);

            return pIntent;
        }
    }
}
