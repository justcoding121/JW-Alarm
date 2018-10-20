namespace JW.Alarm.Core.Uwp
{
    using Autofac;
    using JW.Alarm.ViewModels;

    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize()
        {
            var containerBuilder = new ContainerBuilder();
            Services.IocSetup.Initialize(containerBuilder);
            Services.Uwp.IocSetup.Initialize(containerBuilder);
            ViewModels.IocSetup.Initialize(containerBuilder);

            containerBuilder.RegisterType<MainViewModel>().SingleInstance();
       
            var container = containerBuilder.Build();
            SetContainer(container);
        }

        private static void SetContainer(IContainer container)
        {
            Container = container;
            Services.IocSetup.SetContainer(container);
            Services.Uwp.IocSetup.SetContainer(container);
            ViewModels.IocSetup.SetContainer(container);
        }
    }
}