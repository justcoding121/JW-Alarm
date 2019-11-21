using Bible.Alarm.Contracts.Battery;
using Bible.Alarm.Services;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.ViewModels.Shared;

namespace Bible.Alarm.ViewModels
{
    public static class IocSetup
    {
        internal static IContainer Container { private set; get; }
        public static void Initialize(IContainer container)
        {
            //marking as singleton because this is the starting point
            //and it has calls to Messenger.Subscribe
            container.Register((x) => new HomeViewModel(
                container.Resolve<ScheduleDbContext>(),
                container.Resolve<IToastService>(),
                container.Resolve<INavigationService>(),
                container.Resolve<IMediaCacheService>(),
                container.Resolve<IAlarmService>(),
                container.Resolve<IBatteryOptimizationManager>()),
                isSingleton: true);

            container.Register((x) => new ScheduleViewModel());

            container.Register((x) => new MusicSelectionViewModel());
            container.Register((x) => new SongBookSelectionViewModel());
            container.Register((x) => new TrackSelectionViewModel());

            container.Register((x) => new BibleSelectionViewModel());
            container.Register((x) => new BookSelectionViewModel());
            container.Register((x) => new ChapterSelectionViewModel());

            container.Register((x) => new AlarmViewModal());

            //marked as singleton due to call to Messenger.Subscribe
            container.Register((x) => new MediaProgressViewModal(), isSingleton: true);

            Container = container;
        }

    }
}