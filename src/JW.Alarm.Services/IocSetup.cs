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
            container.Register<IBibleReadingScheduleService>((x) => new BibleReadingScheduleService(container.Resolve<IDatabase>()), isSingleton: true);
            container.Register<IDatabase>((x) => new JsonDatabase(container.Resolve<IStorageService>()), isSingleton: true);
            container.Register<IMediaCacheService>((x) =>
                new MediaCacheService(container.Resolve<IStorageService>(),
                container.Resolve<DownloadService>(),
                container.Resolve<IMediaPlayService>()), isSingleton: true);

            container.Register<IAlarmScheduleService>((x) => new AlarmScheduleService(
                container.Resolve<IDatabase>(),
                container.Resolve<IBibleReadingScheduleService>()), isSingleton: true);

            Container = container;
        }


    }
}