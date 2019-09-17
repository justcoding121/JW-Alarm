namespace JW.Alarm.Services
{
    using Bible.Alarm.Contracts.Network;
    using Bible.Alarm.Services;
    using JW.Alarm.Services.Contracts;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.IO;
    using System.Net.Http;
    using Xamarin.Forms;

    public static class IocSetup
    {
        public static IContainer Container { private set; get; }
        public static void Initialize(IContainer container)
        {
            container.Register<IDownloadService>((x) => new DownloadService(container.Resolve<HttpMessageHandler>()));
            container.Register((x) => new MediaIndexService(container.Resolve<IDownloadService>(), container.Resolve<IStorageService>()));
            container.Register((x) => new MediaService(container.Resolve<MediaIndexService>(), container.Resolve<MediaDbContext>()));

            container.Register<IMediaCacheService>((x) =>
                new MediaCacheService(container.Resolve<IStorageService>(),
                container.Resolve<IDownloadService>(),
                container.Resolve<IPlaylistService>(),
                container.Resolve<ScheduleDbContext>(),
                container.Resolve<MediaService>(),
                container.Resolve<INetworkStatusService>()));

            container.Register<IPlaylistService>((x) => new PlaylistService(container.Resolve<ScheduleDbContext>(),
                container.Resolve<MediaService>()));

            container.Register<IStorageService>((x) => new StorageService());

            container.Register<IAlarmService>((x) => new AlarmService(
              container.Resolve<INotificationService>(),
              container.Resolve<IPlaylistService>(),
              container.Resolve<IMediaCacheService>(),
              container.Resolve<ScheduleDbContext>()));

            Container = container;
        }


    }
}