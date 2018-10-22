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
            container.Register((x)=>new HttpClientHandler());

            container.Register<IStorageService>((x) => new UwpStorageService(), isSingleton: true);
            container.Register<IThreadService>((x) => new UwpThreadService(), isSingleton: true);
            container.Register<IPopUpService>((x) => new UwpPopUpService(container.Resolve<IThreadService>()), isSingleton: true);

            container.Register<IAlarmScheduleService>((x) => new UwpScheduleService(container.Resolve<IDatabase>()), isSingleton: true);
            container.Register((x)=> new AlarmTask(container.Resolve<IMediaPlayService>()));
            container.Register((x)=> new SchedulerTask(container.Resolve<IAlarmScheduleService>()), isSingleton: true);

            container.Register<IMediaPlayService>((x) => new UwpMediaPlayService(container.Resolve<IAlarmScheduleService>(),
                container.Resolve<IBibleReadingScheduleService>(), container.Resolve<MediaService>()));

            Container = container;
        }
    }
}