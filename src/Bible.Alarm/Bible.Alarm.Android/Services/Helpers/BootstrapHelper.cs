using Android.App;
using Android.Content;
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

        public static void VerifyBackgroundTasks()
        {
            if (!IsAlarmSetupTaskScheduled())
            {
                scheduleSetupTask();
            }
        }

        public static async Task InitializeDatabase()
        {
            using (var db = IocSetup.Container.Resolve<ScheduleDbContext>())
            {
                await db.Database.MigrateAsync();
            }
        }

        private static void scheduleSetupTask()
        {
            var context = Application.Context;
            var intent = new Intent(context, typeof(AlarmSetupTask));
            intent.SetAction("com.bible.alarm.SETUP");

            var pIntent = PendingIntent.GetService(
                    context,
                    0,
                    intent,
                    PendingIntentFlags.UpdateCurrent);

            var alarmService = (AlarmManager)context.GetSystemService(Context.AlarmService);

            var interval = 30 * 1000 * 15;
            var firstTrigger = Java.Lang.JavaSystem.CurrentTimeMillis();

            alarmService.SetRepeating(AlarmType.RtcWakeup, firstTrigger, interval, pIntent);

        }

        private static bool IsAlarmSetupTaskScheduled()
        {
            var pIntent = findIntent();
            return pIntent != null;
        }

        private static PendingIntent findIntent()
        {
            var context = Application.Context;
            var intent = new Intent(context, typeof(AlarmSetupTask));
            intent.SetAction("com.bible.alarm.SETUP");

            var pIntent = PendingIntent.GetService(
                    context,
                    0,
                    intent,
                    PendingIntentFlags.NoCreate);

            return pIntent;
        }
    }
}
