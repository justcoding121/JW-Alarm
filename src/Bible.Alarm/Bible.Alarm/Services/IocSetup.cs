namespace Bible.Alarm.Services
{
    using Bible.Alarm.Contracts.Network;
    using Bible.Alarm.Contracts.Platform;
    using Bible.Alarm.Services;
    using Bible.Alarm.Services.Contracts;
    using MediaManager;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.IO;
    using System.Net.Http;
    using Xamarin.Forms;

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

            container.Register<IStorageService>((x) => new StorageService());

            container.Register<IAlarmService>((x) => new AlarmService(container,
              container.Resolve<INotificationService>(),
              container.Resolve<IMediaCacheService>(),
              container.Resolve<ScheduleDbContext>()));
        }

    }
}