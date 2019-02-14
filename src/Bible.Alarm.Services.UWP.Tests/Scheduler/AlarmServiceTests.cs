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
            var mediaDbContext = new MediaDbContext();
            var downloadService = new FakeDownloadService();

            var indexService = new MediaIndexService(downloadService, storageService);
            await indexService.Verify();

            var mediaService = new MediaService(indexService, mediaDbContext);

            var db = new ScheduleDbContext();
            db.Database.EnsureCreated();

            var playlistService = new PlaylistService(db, mediaService);
            var mediaCacheService = new MediaCacheService(storageService, downloadService, playlistService);
            var notificationService = new UwpNotificationService(mediaCacheService);

            var alarmService = new UwpAlarmService(notificationService, playlistService, mediaCacheService, db);

            var player = new MediaPlayer();
            player.AutoPlay = false;

            var playbackService = new PlaybackService(player, playlistService, mediaCacheService, alarmService);
            Actor = new NotificationTaskActor(playbackService);

            var name = $"Test Alarm";
            var alarmTime = DateTime.Now.AddSeconds(3);

            var schedule = new AlarmSchedule()
            {
                Name = name,
                DaysOfWeek =
                    DaysOfWeek.Sunday |
                    DaysOfWeek.Monday |
                    DaysOfWeek.Tuesday |
                    DaysOfWeek.Wednesday |
                    DaysOfWeek.Thursday |
                    DaysOfWeek.Friday |
                    DaysOfWeek.Saturday,
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

            db.AlarmSchedules.Add(schedule);
            db.SaveChanges();

            await alarmService.Create(schedule);

            await Actor.TaskCompletionSource.Task;
        }

        public static NotificationTaskActor Actor;

        public class NotificationTaskActor
        {
            private IPlaybackService playbackService;

            public TaskCompletionSource<bool> TaskCompletionSource = new TaskCompletionSource<bool>();

            public NotificationTaskActor(IPlaybackService playbackService)
            {
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
