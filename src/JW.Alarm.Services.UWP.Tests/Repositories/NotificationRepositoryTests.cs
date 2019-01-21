using JW.Alarm.Models;
using JW.Alarm.Services.Uwp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.UWP.Tests.Repositories
{
    [TestClass]
    public class NotificationDetailRepositoryTests
    {
        [TestMethod]
        public async Task NotificationDetail_Repository_Smoke_Tests()
        {
            var storageService = new UwpStorageService();
            var tableStorage = new TableStorage(storageService);

            var notificationRepository = new NotificationRepository(tableStorage);

            var testRecordCount = 10;

            for (int i = 1; i < testRecordCount; i++)
            {
                var scheduleId = i;
                var newRecord = new NotificationDetail()
                {
                    ScheduleId = i
                };

                await notificationRepository.Add(newRecord);

                var result = await notificationRepository.Read(newRecord.Id);

                Assert.AreEqual(scheduleId, result.ScheduleId);
                Assert.AreEqual(i, (await notificationRepository.Notifications).Count());
            }

            var records = (await notificationRepository.Notifications).ToList();

            for (int i = 1; i < testRecordCount; i++)
            {
                var scheduleId = i + 1;

                await notificationRepository.Update(new NotificationDetail()
                {
                    Id = records[i - 1].Id,
                    ScheduleId = scheduleId
                });

                var result = await notificationRepository.Read(records[i - 1].Id);

                Assert.AreEqual(scheduleId, result.ScheduleId);
            }

            for (int i = 1; i < testRecordCount; i++)
            {
                await notificationRepository.Remove(records[i - 1].Id);
            }

            records = (await notificationRepository.Notifications).ToList();
            Assert.AreEqual(0, records.Count);
        }
    }
}
