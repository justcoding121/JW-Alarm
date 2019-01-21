
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JW.Alarm.Services.Uwp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Windows.Storage;

namespace JW.Alarm.Services.UWP.Tests
{
    [TestClass]
    public class UwpStorageServiceTests
    {
        [TestMethod]
        public async Task Uwp_Storage_Service_Smoke_Tests()
        {
            var testRoot = ApplicationData.Current.LocalFolder.Path;

            var service = new UwpStorageService();

            Assert.IsTrue(await service.DirectoryExists(testRoot));

            var testDir = Path.Combine(testRoot, $"Test");
            var testFileName = "test.txt";
            var testFilePath = Path.Combine(testDir, testFileName);

            //write file
            await service.SaveFile(testDir, testFileName, "Test data.");
            Assert.IsTrue(await service.FileExists(testFilePath));

            //overwrite
            await service.SaveFile(testDir, testFileName, "Test data updated.");
            Assert.IsTrue(await service.FileExists(testFilePath));
            Assert.AreEqual("Test data updated.", await service.ReadFile(testFilePath));

            //get all files
            var allFiles = await service.GetAllFiles(testDir);
            Assert.AreEqual(1, allFiles.Count);
            Assert.AreEqual(testFilePath, allFiles[0]);

            //delete
            await service.DeleteFile(testFilePath);
            Assert.IsFalse(await service.FileExists(testFilePath));
        }

        [TestMethod]
        public async Task Uwp_Storage_Service_MultiThreading_Tests()
        {
            var tasks = new List<Task>();

            //multi-threaded async producer
            tasks.AddRange(Enumerable.Range(1, 10).Select(async x =>
            {
                var testRoot = ApplicationData.Current.LocalFolder.Path;

                var service = new UwpStorageService();

                Assert.IsTrue(await service.DirectoryExists(testRoot));

                var testDir = Path.Combine(testRoot, $"Test{x}");
                var testFileName = "test.txt";
                var testFilePath = Path.Combine(testDir, testFileName);

                //write file
                await service.SaveFile(testDir, testFileName, "Test data.");
                Assert.IsTrue(await service.FileExists(testFilePath));

                //overwrite
                await service.SaveFile(testDir, testFileName, "Test data updated.");
                Assert.IsTrue(await service.FileExists(testFilePath));
                Assert.AreEqual("Test data updated.", await service.ReadFile(testFilePath));

                //get all files
                var allFiles = await service.GetAllFiles(testDir);
                Assert.AreEqual(1, allFiles.Count);
                Assert.AreEqual(testFilePath, allFiles[0]);

                //delete
                await service.DeleteFile(testFilePath);
                Assert.IsFalse(await service.FileExists(testFilePath));

            }));

            await Task.WhenAll(tasks.ToArray());

            tasks.Clear();

            //multi-threaded async producer
            tasks.AddRange(Enumerable.Range(1, 10).Select(async x =>
            {
                var testRoot = ApplicationData.Current.LocalFolder.Path;

                var service = new UwpStorageService();

                Assert.IsTrue(await service.DirectoryExists(testRoot));

                var testDir = Path.Combine(testRoot, "Test");
                var testFileName = $"test{x}.txt";

                var testFilePath = Path.Combine(testDir, testFileName);

                //write file
                await service.SaveFile(testDir, testFileName, "Test data.");
                Assert.IsTrue(await service.FileExists(testFilePath));

                //overwrite
                await service.SaveFile(testDir, testFileName, "Test data updated.");
                Assert.IsTrue(await service.FileExists(testFilePath));
                Assert.AreEqual("Test data updated.", await service.ReadFile(testFilePath));

                //delete
                await service.DeleteFile(testFilePath);
                Assert.IsFalse(await service.FileExists(testFilePath));

            }));

            await Task.WhenAll(tasks.ToArray());
        }
    }
}
