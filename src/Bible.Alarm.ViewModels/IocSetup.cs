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
                container.Resolve<INavigationService>(),
                container.Resolve<IMediaCacheService>()), isSingleton: true);

            container.Register((x) => new ScheduleViewModel());

            container.Register((x) => new MusicSelectionViewModel());
            container.Register((x) => new SongBookSelectionViewModel());
            container.Register((x) => new TrackSelectionViewModel());

            container.Register((x) => new BibleSelectionViewModel());
            container.Register((x) => new BookSelectionViewModel());
            container.Register((x) => new ChapterSelectionViewModel());

            Container = container;
        }

    }
}