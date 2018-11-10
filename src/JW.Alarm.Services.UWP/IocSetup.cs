namespace JW.Alarm.Services.Uwp
{
    using JW.Alarm.Services.Contracts;
    using JW.Alarm.Services.Uwp.Tasks;
    using JW.Alarm.Services.UWP;
    using System.Net.Http;

    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(IContainer container)
        {
            container.Register((x) => new HttpClientHandler());

            container.Register<IStorageService>((x) => new UwpStorageService(), isSingleton: true);
            container.Register<IThreadService>((x) => new UwpThreadService(), isSingleton: true);
            container.Register<IPopUpService>((x) => new UwpPopUpService(container.Resolve<IThreadService>()), isSingleton: true);

            container.Register<INotificationService>((x) => 
            new UwpNotificationService(container.Resolve<IMediaCacheService>(),
                container.Resolve<IPlayDetailDbContext>()), isSingleton: true);

            container.Register((x) => new AlarmTask(container.Resolve<IAlarmService>(),
                                                    container.Resolve<INotificationService>(),
                                                    container.Resolve<IScheduleDbContext>(),
                                                    container.Resolve<IPlaylistService>()));

            container.Register((x) => new SchedulerTask(container.Resolve<IScheduleDbContext>(),
                                    container.Resolve<IMediaCacheService>()), isSingleton: true);

            container.Register<IAlarmService>((x) => new UwpAlarmService(
                container.Resolve<IDatabase>(),
                container.Resolve<INotificationService>(),
                container.Resolve<IPlaylistService>(),
                container.Resolve<IMediaCacheService>()), isSingleton: true);

            Container = container;
        }
    }
}