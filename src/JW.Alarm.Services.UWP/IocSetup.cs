namespace JW.Alarm.Services.Uwp
{
    using Autofac;
    using JW.Alarm.Services.Contracts;
    using JW.Alarm.Services.Uwp.Tasks;
    using JW.Alarm.Services.UWP;
    using System.Net.Http;

    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<UwpScheduleService>().As<IScheduleService>().SingleInstance();

            containerBuilder.RegisterType<UwpStorageService>().As<IStorageService>();
            containerBuilder.RegisterType<UwpThreadService>().As<IThreadService>();
            containerBuilder.RegisterType<AlarmTask>();
            containerBuilder.RegisterType<SchedulerTask>();
            containerBuilder.RegisterType<HttpClientHandler>();
            containerBuilder.RegisterType<UwpPopUpService>().As<IPopUpService>();
            containerBuilder.RegisterType<UwpMediaPlayService>().As<MediaPlayService>();
        }

        public static void SetContainer(IContainer iocContainer)
        {
            Container = iocContainer;
        }
    }
}