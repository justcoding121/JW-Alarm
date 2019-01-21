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
    public class ScheduleRepositoryTests
    {
        [TestMethod]
        public async Task Schedule_Repository_Smoke_Tests()
        {
            var storageService = new UwpStorageService();
            var tableStorage = new TableStorage(storageService);

            var scheduleRepository = new ScheduleRepository(tableStorage);

            var testRecordCount = 10;

            for (int i = 1; i < testRecordCount; i++)
            {
                var name = $"Test{i}";
                var newRecord = new AlarmSchedule()
                {
                    Name = name
                };

                await scheduleRepository.Add(newRecord);

                var result = await scheduleRepository.Read(newRecord.Id);

                Assert.AreEqual(name, result.Name);
                Assert.AreEqual(i, (await scheduleRepository.AlarmSchedules).Count());
            }

            var records = (await scheduleRepository.AlarmSchedules).ToList();

            for (int i = 1; i < testRecordCount; i++)
            {
                var name = $"Test{ i + 1}";

                await scheduleRepository.Update(new AlarmSchedule()
                {
                    Id = records[i - 1].Id,
                    Name = name
                });

                var result = await scheduleRepository.Read(records[i - 1].Id);

                Assert.AreEqual(name, result.Name);
            }

            for (int i = 1; i < testRecordCount; i++)
            {
                await scheduleRepository.Remove(records[i - 1].Id);
            }

            records = (await scheduleRepository.AlarmSchedules).ToList();
            Assert.AreEqual(0, records.Count);
        }
    }
}
