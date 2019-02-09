using JW.Alarm.Models;
using JW.Alarm.Services.Uwp;
using JW.Alarm.Services.Uwp.Helpers;
using Microsoft.Data.Sqlite;
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
            initDatabase();

            var testRecordCount = 10;

            var storageService = new UwpStorageService();
            var tableStorage = new TableStorage();

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

        private void initDatabase()
        {
            using (SqliteConnection db =
                 new SqliteConnection("Filename=bibleAlarm.db"))
            {
                db.Open();

                var tableCommand = "CREATE TABLE IF NOT " +
                    $"EXISTS {typeof(TestEntity).Name} (id INTEGER PRIMARY KEY, " +
                    "data text NOT NULL)";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();
            }
        }
    }

    public class TestEntity : IEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

}
