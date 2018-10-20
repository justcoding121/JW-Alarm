namespace JW.Alarm.ViewModels
{
    using Autofac;

    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<MainViewModel>().SingleInstance();
        }

        public static void SetContainer(IContainer iocContainer)
        {
            Container = iocContainer;
        }
    }
}