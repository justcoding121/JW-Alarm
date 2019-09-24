using Bible.Alarm.Services;
using Bible.Alarm.Services.Contracts;

namespace Bible.Alarm.ViewModels
{
    public static class IocSetup
    {
        internal static IContainer Container { private set; get; }
        public static void Initialize(IContainer container)
        {
            container.Register((x) => new HomeViewModel(
                container.Resolve<ScheduleDbContext>(),
                container.Resolve<IToastService>(),
                container.Resolve<INavigationService>(),
                container.Resolve<IMediaCacheService>(),
                container.Resolve<IAlarmService>()), isSingleton: true);

            container.Register((x) => new ScheduleViewModel());

            container.Register((x) => new MusicSelectionViewModel());
            container.Register((x) => new SongBookSelectionViewModel());
            container.Register((x) => new TrackSelectionViewModel());

            container.Register((x) => new BibleSelectionViewModel());
            container.Register((x) => new BookSelectionViewModel());
            container.Register((x) => new ChapterSelectionViewModel());

            container.Register((x) => new AlarmViewModal());

            Container = container;
        }

    }
}