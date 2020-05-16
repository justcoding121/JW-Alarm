namespace Bible.Alarm.Services
{
    using Bible.Alarm.Contracts.Network;
    using Bible.Alarm.Contracts.Platform;
    using Bible.Alarm.Services.Contracts;
    using Bible.Alarm.Services.Network;
    using Bible.Alarm.Services.Tasks;
    using MediaManager;
    using System.Net.Http;

    public static class IocSetup
    {
        public static void Initialize(IContainer container, bool isService)
        {
            container.Register<IDownloadService>((x) => new DownloadService(container.Resolve<HttpMessageHandler>()));
            container.Register((x) => new MediaIndexService(container.Resolve<IStorageService>(), container.Resolve<IVersionFinder>()));
            container.Register((x) => new MediaService(container.Resolve<MediaIndexService>(), container.Resolve<MediaDbContext>()));

            container.Register<IMediaCacheService>((x) =>
                new MediaCacheService(container.Resolve<IStorageService>(),
                container.Resolve<IDownloadService>(),
                container.Resolve<IPlaylistService>(),
                container.Resolve<ScheduleDbContext>(),
                container.Resolve<MediaService>(),
                container.Resolve<INetworkStatusService>(),
                container.Resolve<IMediaManager>()));

            container.Register<IPlaylistService>((x) => new PlaylistService(container.Resolve<ScheduleDbContext>(),
                container.Resolve<MediaService>()));

            container.Register<IAlarmService>((x) => new AlarmService(container,
              container.Resolve<INotificationService>(),
              container.Resolve<IMediaCacheService>(),
              container.Resolve<ScheduleDbContext>()));

            container.Register<INetworkStatusService>((x) => new NetworkStatusService(container));

            container.Register<IPlaybackService>((x) => new PlaybackService(container.Resolve<IMediaManager>(),
                  container.Resolve<IPlaylistService>(),
                  container.Resolve<IAlarmService>(),
                  container.Resolve<IMediaCacheService>(),
                  container.Resolve<IStorageService>(),
                  container.Resolve<INetworkStatusService>(),
                  container.Resolve<INotificationService>(),
                  container.Resolve<IDownloadService>()));

            container.Register((x) => new SchedulerTask(container.Resolve<ScheduleDbContext>(),
                                 container.Resolve<IMediaCacheService>(), container.Resolve<IAlarmService>(),
                                 container.Resolve<INotificationService>()));
        }

    }
}