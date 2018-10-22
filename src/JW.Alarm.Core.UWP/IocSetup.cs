namespace JW.Alarm.Core.Uwp
{
 
    public static class IocSetup
    {
        public static IContainer Container;
        public static void Initialize()
        {
            var container = JW.Alarm.Container.Default;

            Services.IocSetup.Initialize(container);
            Services.Uwp.IocSetup.Initialize(container);
            ViewModels.IocSetup.Initialize(container);

            Container = container;
        }


    }
}