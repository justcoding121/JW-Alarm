namespace Bible.Alarm.Services.iOS
{
    using Bible.Alarm.Contracts.Network;
    using Bible.Alarm.Contracts.Platform;
    using Bible.Alarm.Droid.Services.Storage;
    using Bible.Alarm.iOS.Services.Handlers;
    using Bible.Alarm.iOS.Services.Platform;
    using Bible.Alarm.Services.Contracts;
    using Bible.Alarm.Services.iOS.Tasks;
    using MediaManager;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    public static class IocSetup
    {
        public static void Initialize(IContainer container, bool isService)
        {
            container.Register<HttpMessageHandler>((x) => new NSUrlSessionHandler());

            if (!isService)
            {
                container.Register<IToastService>((x) => new iOSToastService(container.Resolve<TaskScheduler>()));
            }

            container.Register<INotificationService>((x) => new iOSNotificationService(container));

            container.Register((x) => new SchedulerTask(container.Resolve<ScheduleDbContext>(),
                                    container.Resolve<IMediaCacheService>(), container.Resolve<IAlarmService>(),
                                    container.Resolve<INotificationService>()));


            container.Register<IPreviewPlayService>((x) => new PreviewPlayService(container, container.Resolve<IDownloadService>()));

            container.Register<IPlaybackService>((x) => new PlaybackService(container.Resolve<IMediaManager>(),
                container.Resolve<IPlaylistService>(),
                container.Resolve<IAlarmService>(),
                container.Resolve<IMediaCacheService>(),
                container.Resolve<IStorageService>(),
                container.Resolve<INetworkStatusService>(),
                container.Resolve<INotificationService>(),
                container.Resolve<IDownloadService>(),
                container.Resolve<IToastService>()));

            container.Register((x) =>
            {
                var storageService = container.Resolve<IStorageService>();
                string databasePath = storageService.StorageRoot;

                var scheduleDbConfig = new DbContextOptionsBuilder<ScheduleDbContext>()
                    .UseSqlite($"Filename={Path.Combine(databasePath, "bibleAlarm.db")}").Options;
                return new ScheduleDbContext(scheduleDbConfig);
            });


            container.Register((x) =>
            {
                var storageService = container.Resolve<IStorageService>();
                string databasePath = storageService.StorageRoot;

                var mediaDbConfig = new DbContextOptionsBuilder<MediaDbContext>()
                    .UseSqlite($"Filename={Path.Combine(databasePath, "mediaIndex.db")}").Options;
                return new MediaDbContext(mediaDbConfig);
            });

            container.Register((x) =>
            {
                return CrossMediaManager.Current;
            });

            container.Register<IVersionFinder>((x) => new VersionFinder());
            container.Register<IStorageService>((x) => new iOSStorageService());
            container.Register((x) =>
                    new iOSAlarmHandler(container.Resolve<IPlaybackService>(),
                                container.Resolve<IMediaManager>(),
                                container.Resolve<ScheduleDbContext>(),
                                container.Resolve<TaskScheduler>()));
        }
    }
}