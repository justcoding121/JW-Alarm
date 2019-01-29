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
using Windows.ApplicationModel.Background;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Notifications;

namespace JW.Alarm.Services.UWP.Tests.Scheduler
{
    [TestClass]
    public class AlarmServiceTests
    {
        public static NotificationTaskActor Actor;

        [TestMethod]
        public async Task Alarm_Service_Smoke_Test()
        {
            BootstrapHelper.VerifyBackgroundTasks();

            var storageService = new UwpStorageService();
            var downloadService = new FakeDownloadService();

            var indexService = new MediaIndexService(downloadService, storageService);
            await indexService.Verify();

            var mediaService = new MediaService(indexService, storageService);

            var tableStorage = new TableStorage(storageService);
            var scheduleRepository = new ScheduleRepository(tableStorage);

            var playlistService = new PlaylistService(scheduleRepository, mediaService);
            var mediaCacheService = new MediaCacheService(storageService, downloadService, playlistService);
            var notificationRepository = new NotificationRepository(tableStorage);
            var notificationService = new UwpNotificationService(mediaCacheService, notificationRepository);

            var alarmService = new UwpAlarmService(notificationService, playlistService, mediaCacheService);

            Actor = new NotificationTaskActor(alarmService, notificationService, scheduleRepository, playlistService);

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
            await alarmService.Create(schedule);

            await Task.Delay(1000 * 60);
        }

        public class NotificationTaskActor
        {
            private IAlarmService alarmService;
            private INotificationService notificationService;
            private IScheduleRepository scheduleDbContext;
            private IPlaylistService playlistService;

            public NotificationTaskActor(IAlarmService alarmService,
                INotificationService notificationService,
                IScheduleRepository scheduleDbContext,
                IPlaylistService playlistService)
            {
                this.alarmService = alarmService;
                this.notificationService = notificationService;
                this.scheduleDbContext = scheduleDbContext;
                this.playlistService = playlistService;
            }


            public async void Handle(IBackgroundTaskInstance backgroundTask)
            {
                var deferral = backgroundTask.GetDeferral();

                var details = backgroundTask.TriggerDetails as ToastNotificationHistoryChangedTriggerDetail;

                if (details.ChangeType == ToastHistoryChangedType.Added)
                {
                    var history = ToastNotificationManager.History.GetHistory();

                    var s = history.Select(x => new { x.Content, x.Data, x.Group, x.SuppressPopup, x.Tag, x.ExpirationTime, x.NotificationMirroring, x.Priority }).ToList();

                    if (history.Any(x => x.Group == "Clear"))
                    {
                        var playDetails = new Dictionary<ToastNotification, NotificationDetail>();
                        foreach (var toast in history)
                        {
                            if (toast.Group != "Clear")
                            {
                                var detail = await notificationService.ParseNotificationDetail(toast.Tag);
                                playDetails.Add(toast, detail);
                            }
                        }

                        var latestTrackDetail = playDetails
                            .Select(x => x.Value)
                            .OrderByDescending(x => x.NotificationTime)
                            .FirstOrDefault();

                        if (latestTrackDetail != null)
                        {
                            var outDatedTrackDetails = playDetails.Where(x => x.Value != latestTrackDetail);

                            if (outDatedTrackDetails.Count() > 0)
                            {
                                foreach (var trackDetail in outDatedTrackDetails.Select(x => x.Value).OrderBy(x => x.NotificationTime))
                                {
                                    await playlistService.MarkTrackAsFinished(trackDetail);
                                }
                            }

                            var schedule = await scheduleDbContext.Read(latestTrackDetail.ScheduleId);
                            await alarmService.ScheduleNextTrack(schedule, latestTrackDetail);
                        }

                        ToastNotificationManager.History.Clear();
                        return;
                    }

                }

                deferral.Complete();
            }
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
