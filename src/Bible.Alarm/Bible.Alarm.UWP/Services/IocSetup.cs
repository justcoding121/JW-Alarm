namespace Bible.Alarm.Services.Uwp
{
    using Bible.Alarm;
    using Bible.Alarm.Contracts.Network;
    using Bible.Alarm.Contracts.Platform;
    using Bible.Alarm.Services;
    using Bible.Alarm.Services.Contracts;
    using Bible.Alarm.Services.Uwp.Tasks;
    using Bible.Alarm.Uwp.Services.Handlers;
    using Bible.Alarm.Uwp.Services.Platform;
    using Bible.Alarm.Uwp.Services.Storage;
    using JW.Alarm.Services.UWP;
    using MediaManager;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Windows.Media.Playback;

    public static class IocSetup
    {
        public static IContainer Container { get; private set; }

        public static void Initialize(IContainer container, bool isService)
        {
            Container = container;

            container.Register<HttpMessageHandler>((x) => new HttpClientHandler());

            container.Register<IToastService>((x) => new UwpToastService(container.Resolve<TaskScheduler>()));

            container.Register<INotificationService>((x) => new UwpNotificationService(container));

            container.Register((x) => new SchedulerTask(container.Resolve<ScheduleDbContext>(),
                                    container.Resolve<IMediaCacheService>(), container.Resolve<IAlarmService>(),
                                    container.Resolve<INotificationService>()));


            container.Register<IPreviewPlayService>((x) => new PreviewPlayService(container.Resolve<MediaPlayer>()));

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
                    .UseSqlite($"Data Source={Path.Combine(databasePath, "bibleAlarm.db")}").Options;
                return new ScheduleDbContext(scheduleDbConfig);
            });


            container.Register((x) =>
            {
                var storageService = container.Resolve<IStorageService>();
                string databasePath = storageService.StorageRoot;

                var mediaDbConfig = new DbContextOptionsBuilder<MediaDbContext>()
                    .UseSqlite($"Data Source={Path.Combine(databasePath, "mediaIndex.db")}").Options;
                return new MediaDbContext(mediaDbConfig);
            });

            container.Register((x) =>
            {
                return CrossMediaManager.Current;

            });

            container.Register<IVersionFinder>((x) => new UwpVersionFinder());
            container.Register<IStorageService>((x) => new UwpStorageService());
            container.Register((x) =>
                    new UwpAlarmHandler(container.Resolve<IPlaybackService>(),
                                container.Resolve<IMediaManager>(),
                                container.Resolve<ScheduleDbContext>()));
        }
    }
}