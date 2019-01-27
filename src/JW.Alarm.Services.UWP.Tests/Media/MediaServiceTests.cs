using JW.Alarm.Services.Contracts;
using JW.Alarm.Services.Uwp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.UWP.Tests.Media
{
    [TestClass]
    public class MediaServiceTests
    {
        [TestMethod]
        public async Task Media_Service_Smoke_Test_Bible_Index()
        {
            var storageService = new UwpStorageService();
            var downloadService = new FakeDownloadService();

            var indexService = new MediaIndexService(downloadService, storageService);
            await indexService.Verify();

            var service = new MediaService(indexService, storageService);

            var languages = await service.GetBibleLanguages();
            Assert.IsTrue(languages.Count > 0);

            var testLanguage = languages.First().Value.Code;
            var translations = await service.GetBibleTranslations(testLanguage);
            Assert.IsTrue(translations.Count > 0);

            var testTranslation = translations.First().Value.Code;
            var books = await service.GetBibleBooks(testLanguage, testTranslation);
            Assert.IsTrue(books.Count > 0);

            var testBook = books.First().Value.Number;
            var chapters = await service.GetBibleChapters(testLanguage, testTranslation, testBook);
            Assert.IsTrue(chapters.Count > 0);
        }

        private class FakeDownloadService : IDownloadService
        {
            public Task<byte[]> DownloadAsync(string url, string alternativeUrl = null)
            {
                throw new NotImplementedException();
            }
        }
    }
}
