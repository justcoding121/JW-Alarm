﻿using JW.Alarm.Models;
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
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Notifications;

namespace JW.Alarm.Services.UWP.Tests.Scheduler
{
    [TestClass]
    public class AlarmServiceTests
    {
        [TestMethod]
        public async Task Alarm_Service_Smoke_Test()
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

            var alarmService = new UwpAlarmService(notificationService, playlistService, mediaCacheService, scheduleRepository);

            var player = new MediaPlayer();
            player.AutoPlay = false;

            var playbackService = new PlaybackService(player, playlistService, mediaCacheService, alarmService);
            Actor = new NotificationTaskActor(alarmService, notificationService, scheduleRepository, playbackService);

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

            await Actor.TaskCompletionSource.Task;
        }

        public static NotificationTaskActor Actor;

        public class NotificationTaskActor
        {
            private IAlarmService alarmService;
            private INotificationService notificationService;
            private IScheduleRepository scheduleDbContext;
            private IPlaybackService playbackService;

            public TaskCompletionSource<bool> TaskCompletionSource = new TaskCompletionSource<bool>();

            public NotificationTaskActor(IAlarmService alarmService,
                INotificationService notificationService,
                IScheduleRepository scheduleDbContext,
                IPlaybackService playbackService)
            {
                this.alarmService = alarmService;
                this.notificationService = notificationService;
                this.scheduleDbContext = scheduleDbContext;
                this.playbackService = playbackService;
            }


            public async void Handle(IBackgroundTaskInstance backgroundTask)
            {
                var deferral = backgroundTask.GetDeferral();

                var details = backgroundTask.TriggerDetails as ToastNotificationHistoryChangedTriggerDetail;

                if (details.ChangeType == ToastHistoryChangedType.Added)
                {
                    var toast = ToastNotificationManager.History.GetHistory().Select(x => new
                    {
                        x.Group
                    })
                    .FirstOrDefault();

                    Assert.IsNotNull(toast);

                    await playbackService.Play(long.Parse(toast.Group));

                }

                deferral.Complete();

                TaskCompletionSource.SetResult(true);
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
