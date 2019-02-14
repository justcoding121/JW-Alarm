namespace JW.Alarm.Services
{
    using JW.Alarm.Services.Contracts;
    using Microsoft.EntityFrameworkCore;
    using System.Net.Http;

    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(IContainer container)
        {
            container.Register<IDownloadService>((x) => new DownloadService(container.Resolve<HttpClientHandler>()));
            container.Register((x) => new MediaIndexService(container.Resolve<IDownloadService>(), container.Resolve<IStorageService>()), isSingleton: true);
            container.Register((x) => new MediaService(container.Resolve<MediaIndexService>(), container.Resolve<MediaDbContext>()), isSingleton: true);

            container.Register<IMediaCacheService>((x) =>
                new MediaCacheService(container.Resolve<IStorageService>(),
                container.Resolve<IDownloadService>(),
                container.Resolve<IPlaylistService>()), isSingleton: true);

            var scheduleDbConfig = new DbContextOptionsBuilder<ScheduleDbContext>()
                .UseSqlite("Data Source=bibleAlarm.db").Options;

            container.Register((x) => new ScheduleDbContext(scheduleDbConfig));

            var mediaDbConfig = new DbContextOptionsBuilder<MediaDbContext>()
                .UseSqlite("Data Source=mediaIndex.db").Options;

            container.Register((x) => new MediaDbContext(mediaDbConfig));

            container.Register<IPlaylistService>((x) => new PlaylistService(container.Resolve<ScheduleDbContext>(),
                container.Resolve<MediaService>()), isSingleton: true);

            Container = container;
        }


    }
}