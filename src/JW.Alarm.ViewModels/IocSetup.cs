using JW.Alarm.Services.Contracts;

namespace JW.Alarm.ViewModels
{
    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(IContainer container)
        {
            container.Register((x) => new ScheduleListViewModel(container.Resolve<IAlarmScheduleService>(), 
                container.Resolve<IThreadService>(), container.Resolve<IPopUpService>()), isSingleton: true);
            Container = container;
        }

    }
}