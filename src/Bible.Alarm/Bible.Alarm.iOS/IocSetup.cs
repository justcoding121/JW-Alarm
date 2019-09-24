using Bible.Alarm;

namespace Bible.Alarm.iOS
{
 
    public static class IocSetup
    {
        public static IContainer Container;
        public static void Initialize()
        {
            var container = Bible.Alarm.Container.Default;

            UI.IocSetup.Initialize(container);
            Bible.Alarm.Services.IocSetup.Initialize(container);
            Bible.Alarm.Services.iOS.IocSetup.Initialize(container);
            Bible.Alarm.ViewModels.IocSetup.Initialize(container);

            Container = container;
        }


    }
}