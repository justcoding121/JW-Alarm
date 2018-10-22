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
            container.Register((x) => new MediaLookUpService(container.Resolve<DownloadService>(), container.Resolve<IStorageService>()), isSingleton: true);
            container.Register((x) => new MediaService(container.Resolve<MediaLookUpService>(), container.Resolve<IStorageService>()), isSingleton:true);
            container.Register<IBibleReadingScheduleService>((x) => new BibleReadingScheduleService(container.Resolve<IDatabase>()), isSingleton: true);
            container.Register<IDatabase>((x)=> new JsonDatabase(container.Resolve<IStorageService>()), isSingleton:true);

            Container = container;
        }


    }
}