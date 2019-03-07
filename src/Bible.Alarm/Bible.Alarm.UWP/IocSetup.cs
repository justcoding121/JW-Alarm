using JW.Alarm;

namespace Bible.Alarm.UWP
{
 
    public static class IocSetup
    {
        public static IContainer Container;
        public static void Initialize()
        {
            var container = JW.Alarm.Container.Default;

            UI.IocSetup.Initialize(container);
            JW.Alarm.Services.IocSetup.Initialize(container);
            JW.Alarm.Services.Uwp.IocSetup.Initialize(container);
            JW.Alarm.ViewModels.IocSetup.Initialize(container);

            Container = container;
        }


    }
}