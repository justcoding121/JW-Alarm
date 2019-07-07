using JW.Alarm.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JW.Alarm.Services.iOS.Helpers
{
    public class BootstrapHelper
    {
        private static readonly string alarmTaskName = "AlarmTask";
        private static readonly string schedulerTaskName = "SchedulerTask";
        private static readonly string snoozeDismissTaskName = "SnoozeDismissTask";

        public async static Task VerifyPermissions()
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
            throw new NotImplementedException();
        }

        public static void unregisterBackgroundTasks(string name)
        {
            throw new NotImplementedException();
        }

        public async static Task InitializeDatabase()
        {
            using (var db = IocSetup.Container.Resolve<ScheduleDbContext>())
            {
                await db.Database.MigrateAsync();
            }
        }
    }
}
