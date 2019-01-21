using JW.Alarm.Models;
using JW.Alarm.Services.Uwp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.UWP.Tests.Storage
{
    [TestClass]
    public class UwpTableStorageTests
    {
        [TestMethod]
        public async Task Uwp_Table_Storage_Smoke_Tests()
        {
            var testRecordCount = 10;

            var storageService = new UwpStorageService();
            var tableStorage = new TableStorage(storageService);

            for (int i = 1; i < testRecordCount; i++)
            {
                var name = $"Test{i}";

                var newRecord = new TestEntity()
                {
                    Name = name
                };

                await tableStorage.Insert(newRecord);

                var result = await tableStorage.Read<TestEntity>(newRecord.Id);

                Assert.AreEqual(name, result.Name);
                Assert.AreEqual(i, (await tableStorage.ReadAll<TestEntity>()).Count());
            }

            var records = (await tableStorage.ReadAll<TestEntity>()).ToList();

            for (int i = 1; i < testRecordCount; i++)
            {
                var name = $"Test{ i + 1}";

                await tableStorage.Update(new TestEntity()
                {
                    Id = records[i - 1].Id,
                    Name = name
                });

                var result = await tableStorage.Read<TestEntity>(records[i - 1].Id);

                Assert.AreEqual(name, result.Name);
            }

            for (int i = 1; i < testRecordCount; i++)
            {
                await tableStorage.Delete<TestEntity>(records[i - 1].Id);
            }

            records = (await tableStorage.ReadAll<TestEntity>()).ToList();
            Assert.AreEqual(0, records.Count);
        }
    }

    public class TestEntity : IEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

}
