namespace Bible.Alarm.Services.iOS
{
    using Bible.Alarm.Contracts.Battery;
    using Bible.Alarm.Contracts.Network;
    using Bible.Alarm.Contracts.Platform;
    using Bible.Alarm.iOS.Services.Platform;
    using Bible.Alarm.Services.Contracts;
    using Bible.Alarm.Services.iOS.Tasks;
    using MediaManager;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.IO;
    using System.Net.Http;

    public static class IocSetup
    {
        public static void Initialize(IContainer container, bool isService)
        {
            container.Register<HttpMessageHandler>((x) => new NSUrlSessionHandler());

            if (!isService)
            {
                container.Register<IToastService>((x) => new iOSToastService(container));
            }

            container.Register<INotificationService>((x) => new iOSNotificationService(container));

            container.Register((x) => new SchedulerTask(container.Resolve<ScheduleDbContext>(),
                                    container.Resolve<IMediaCacheService>(), container.Resolve<IAlarmService>(),
                                    container.Resolve<INotificationService>()));


            container.Register<IPreviewPlayService>((x) => new PreviewPlayService(container));

            container.Register<IPlaybackService>((x) => new PlaybackService(container.Resolve<IMediaManager>(),
                container.Resolve<IPlaylistService>(),
                container.Resolve<IAlarmService>(),
                container.Resolve<IMediaCacheService>(),
                container.Resolve<IStorageService>(),
                container.Resolve<INetworkStatusService>(),
                container.Resolve<INotificationService>()));

            string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");

            var scheduleDbConfig = new DbContextOptionsBuilder<ScheduleDbContext>()
                .UseSqlite($"Filename={Path.Combine(databasePath, "bibleAlarm.db")}").Options;

            container.Register((x) => new ScheduleDbContext(scheduleDbConfig));

            var mediaDbConfig = new DbContextOptionsBuilder<MediaDbContext>()
                .UseSqlite($"Filename={Path.Combine(databasePath, "mediaIndex.db")}").Options;

            container.Register((x) => new MediaDbContext(mediaDbConfig));
            container.RegisterSingleton((x) =>
            {
                return CrossMediaManager.Current;

            });

            container.Register<IVersionFinder>((x) => new VersionFinder());

        }
    }
}