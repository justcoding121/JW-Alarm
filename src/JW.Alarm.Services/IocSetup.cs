namespace JW.Alarm.Services
{
    using Autofac;
    using JW.Alarm.Services.Contracts;

    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<DownloadService>();
            containerBuilder.RegisterType<MediaLookUpService>();
            containerBuilder.RegisterType<MediaService>();

            containerBuilder.RegisterType<BibleReadingScheduleService>().As<IBibleReadingScheduleService>();
            containerBuilder.RegisterType<JsonDatabase>().As<IDatabase>().SingleInstance();
        }

        public static void SetContainer(IContainer iocContainer)
        {
            Container = iocContainer;
        }
    }
}