using JW.Alarm.Models;
using JW.Alarm.Services.Contracts;
using JW.Alarm.Services.Uwp;
using JW.Alarm.Services.Uwp.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Notifications;

namespace JW.Alarm.Services.UWP.Tests.UI
{
    [TestClass]
    public class NotificationServiceTests
    {
        [TestMethod]
        public async Task Notification_Service_Smoke_Test()
        {
            BootstrapHelper.InitializeDatabase();

            var storageService = new UwpStorageService();
            var downloadService = new FakeDownloadService();

            var indexService = new MediaIndexService(downloadService, storageService);
            await indexService.Verify();

            var mediaService = new MediaService(indexService, storageService);

            var tableStorage = new TableStorage();
            var scheduleRepository = new ScheduleRepository(tableStorage);

            var playlistService = new PlaylistService(scheduleRepository, mediaService);
            var mediaCacheService = new MediaCacheService(storageService, downloadService, playlistService);
            var notificationService = new UwpNotificationService(mediaCacheService);

            var name = $"Test Alarm";
            var alarmTime = DateTime.Now.AddSeconds(3);

            var schedule = new AlarmSchedule()
            {
                Name = name,
                DaysOfWeek = new HashSet<DayOfWeek>(new[] {
                    DayOfWeek.Sunday,
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Wednesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday,
                    DayOfWeek.Saturday
                }),
                Hour = alarmTime.Hour,
                Minute = alarmTime.Minute,
                Second = alarmTime.Second,
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

            await scheduleRepository.Add(schedule);
            await mediaCacheService.SetupAlarmCache(schedule.Id);

            var track = await playlistService.NextTrack(schedule.Id);

            var notificationTime = DateTime.Now.AddSeconds(1);


            notificationService.Add(schedule.Id, notificationTime, "Test", "Body", null);

            await Task.Delay(2 * 1000);

            var history = ToastNotificationManager.History.GetHistory();

            var notification = history.FirstOrDefault(x => x.Group == schedule.Id.ToString());

            Assert.IsNotNull(notification);
        }

        private class FakeDownloadService : IDownloadService
        {
            private Random rnd = new Random();
            private byte[] cache;
            private SemaphoreSlim @lock = new SemaphoreSlim(1);

            public async Task<byte[]> DownloadAsync(string url, string alternativeUrl = null)
            {
                if (cache != null)
                {
                    return cache;
                }

                await @lock.WaitAsync();

                try
                {
                    var sampleFile = new Uri("ms-appx:///Assets/Media/Alarm_Sample.mp3", UriKind.Absolute);

                    var file = await StorageFile.GetFileFromApplicationUriAsync(sampleFile);

                    byte[] buffer = new byte[1024];

                    using (var ms = new MemoryStream())
                    {
                        using (BinaryWriter fileWriter = new BinaryWriter(ms))
                        {
                            using (BinaryReader fileReader = new BinaryReader(await file.OpenStreamForReadAsync()))
                            {
                                long readCount = 0;
                                while (readCount < fileReader.BaseStream.Length)
                                {
                                    int read = fileReader.Read(buffer, 0, buffer.Length);
                                    readCount += read;
                                    fileWriter.Write(buffer, 0, read);
                                }
                            }
                        }

                        cache = ms.ToArray();
                    }
                }
                finally
                {
                    @lock.Release();
                }

                return cache;
            }
        }
    }
}
