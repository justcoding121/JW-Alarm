namespace JW.Alarm.Services.iOS
{
    using AVFoundation;
    using Foundation;
    using JW.Alarm.Services.Contracts;
    using JW.Alarm.Services.iOS;
    using MediaManager;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.IO;
    using System.Net.Http;

    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(IContainer container)
        {
            container.Register<HttpMessageHandler>((x) => new NSUrlSessionHandler());

            container.Register<IToastService>((x) => new iOSPopUpService());

            container.Register<INotificationService>((x) =>
            new iOSNotificationService(container.Resolve<IMediaCacheService>()));


            container.Register<IPreviewPlayService>((x) => new PreviewPlayService(container.Resolve<AVPlayer>()));
            container.Register((x) =>
            {
                var player = new AVQueuePlayer();
                return player;
            });

            container.Register((x) =>
            {
                var player = new AVPlayer();
                return player;
            });

            container.Register<IPlaybackService>((x) => new PlaybackService(container.Resolve<AVQueuePlayer>(),
                                                            container.Resolve<IPlaylistService>(),
                                                            container.Resolve<IMediaCacheService>(),
                                                            container.Resolve<IAlarmService>()));
          
            SQLitePCL.Batteries_V2.Init();

            container.Register<IMediaManager>((x) =>
            {
                return CrossMediaManager.Current;

            }, true);

            string bibleAlarmDatabasePath = getDatabasePath(dbName: "bibleAlarm.db");

            var scheduleDbConfig = new DbContextOptionsBuilder<ScheduleDbContext>()
                .UseSqlite($"Filename={bibleAlarmDatabasePath}").Options;

            if (File.Exists(bibleAlarmDatabasePath))
            {

            }
            container.Register((x) => new ScheduleDbContext(scheduleDbConfig));

            string mediaIndexDatabasePath = getDatabasePath(dbName: "bibleAlarm.db");
            var mediaDbConfig = new DbContextOptionsBuilder<MediaDbContext>()
                .UseSqlite($"Filename={mediaIndexDatabasePath}").Options;

            container.Register((x) => new MediaDbContext(mediaDbConfig));

            Container = container;
        }

        private static string getDatabasePath(string dbName)
        {
            return Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments),
                "..",
                "Library",
                dbName);
        }
    }
}