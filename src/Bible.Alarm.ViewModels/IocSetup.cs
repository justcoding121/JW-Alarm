using Bible.Alarm.Services.Contracts;
using JW.Alarm.Services;
using JW.Alarm.Services.Contracts;

namespace JW.Alarm.ViewModels
{
    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(IContainer container)
        {
            container.Register((x) => new HomeViewModel(
                container.Resolve<ScheduleDbContext>(), 
                container.Resolve<IThreadService>(), 
                container.Resolve<IPopUpService>(),
                container.Resolve<INavigationService>()), isSingleton: false);

            container.Register((x) => new ScheduleViewModel(), isSingleton: false);

            Container = container;
        }

    }
}