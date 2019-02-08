using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using JW.Alarm.Services.Uwp;
using JW.Alarm.Services.Uwp.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.UWP.Tests.Media
{
    [TestClass]
    public class MediaCacheServiceTests
    {
        [TestMethod]
        public async Task Media_Cache_Service_Smoke_Test()
        {
            BootstrapHelper.InitializeDatabase();

            var storageService = new UwpStorageService();
            var downloadService = new FakeDownloadService();

            var indexService = new MediaIndexService(downloadService, storageService);

            //intented async without Wait() for testing code below.
            var task = Task.Run(() => indexService.Verify());

            var mediaService = new MediaService(indexService, storageService);

            await mediaService.GetMelodyMusicReleases();

            var tableStorage = new TableStorage();
            var scheduleRepository = new ScheduleRepository(tableStorage);

            var name = $"Test Alarm";
            var newRecord = new AlarmSchedule()
            {
                Name = name,
                DaysOfWeek = new HashSet<DayOfWeek>(new[] { DayOfWeek.Sunday, DayOfWeek.Monday }),
                Hour = 10,
                Minute = 30,
                IsEnabled = true,
                Music = new AlarmMusic()
                {
                    MusicType = MusicType.Melodies,
                    PublicationCode = "iam",
                    LanguageCode = "E",
                    TrackNumber = 89
                },

                BibleReadingSchedule = new BibleReadingSchedule()
                {
                    BookNumber = 23,
                    ChapterNumber = 1,
                    LanguageCode = "E",
                    PublicationCode = "nwt"
                }
            };

            await scheduleRepository.Add(newRecord);

            var playlistService = new PlaylistService(scheduleRepository, mediaService);

            var mediaCacheService = new MediaCacheService(storageService, downloadService, playlistService);

            await mediaCacheService.SetupAlarmCache(newRecord.Id);

            var track = await playlistService.NextTrack(newRecord.Id);

            Assert.IsNotNull(mediaCacheService.GetCacheFileName(track.Url));

            Assert.IsTrue(await mediaCacheService.Exists(track.Url));
        }

        private class FakeDownloadService : IDownloadService
        {
            private Random rnd = new Random();
            public Task<byte[]> DownloadAsync(string url, string alternativeUrl = null)
            {
                var b = new byte[5];
                rnd.NextBytes(b);
                return Task.FromResult(b);
            }
        }
    }
}
