namespace JW.Alarm.Services
{
    using JW.Alarm.Services.Contracts;
    using System.Net.Http;

    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(IContainer container)
        {
            container.Register((x) => new DownloadService(container.Resolve<HttpClientHandler>()));
            container.Register((x) => new MediaIndexService(container.Resolve<DownloadService>(), container.Resolve<IStorageService>()), isSingleton: true);
            container.Register((x) => new MediaService(container.Resolve<MediaIndexService>(), container.Resolve<IStorageService>()), isSingleton: true);
         
            container.Register<ITableStorage>((x) => new TableStorage(container.Resolve<IStorageService>()), isSingleton: true);

            container.Register<IMediaCacheService>((x) =>
                new MediaCacheService(container.Resolve<IStorageService>(),
                container.Resolve<DownloadService>(),
                container.Resolve<IPlaylistService>()), isSingleton: true);

            container.Register<IScheduleRepository>((x) => new ScheduleRepository(
                container.Resolve<ITableStorage>()), isSingleton: true);

            container.Register<INotificationRepository>((x) => new NotificationRepository(
               container.Resolve<ITableStorage>()), isSingleton: true);

            container.Register<IPlaylistService>((x) => new PlaylistService(container.Resolve<IScheduleRepository>(), 
                container.Resolve<MediaService>()), isSingleton: true);

            Container = container;
        }


    }
}