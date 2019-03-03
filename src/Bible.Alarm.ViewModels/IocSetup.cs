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
                container.Resolve<INavigationService>()), isSingleton: true);

            container.Register((x) => new ScheduleViewModel(), isSingleton: false);

            container.Register((x) => new MusicSelectionViewModel(), isSingleton: false);

            container.Register((x) => new SongBookSelectionViewModel(), isSingleton: false);
            container.Register((x) => new TrackSelectionViewModel(), isSingleton: false);

            Container = container;
        }

    }
}