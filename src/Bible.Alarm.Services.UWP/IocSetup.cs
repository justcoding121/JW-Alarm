namespace JW.Alarm.Services.Uwp
{
    using JW.Alarm.Services.Contracts;
    using JW.Alarm.Services.Uwp.Tasks;
    using JW.Alarm.Services.UWP;
    using System.Net.Http;
    using Windows.Media.Playback;

    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(IContainer container)
        {
            container.Register((x) => new HttpClientHandler());

            container.Register<IStorageService>((x) => new UwpStorageService());
            container.Register<IThreadService>((x) => new UwpThreadService());
            container.Register<IPopUpService>((x) => new UwpPopUpService(container.Resolve<IThreadService>()));

            container.Register<INotificationService>((x) =>
            new UwpNotificationService(container.Resolve<IMediaCacheService>()));

            container.Register((x) => new AlarmTask(container.Resolve<IPlaybackService>()));

            container.Register((x) => new SnoozeDismissTask(container.Resolve<IPlaybackService>()));


            container.Register((x) => new SchedulerTask(container.Resolve<ScheduleDbContext>(),
                                    container.Resolve<IMediaCacheService>(), container.Resolve<IAlarmService>(),
                                    container.Resolve<INotificationService>()));

            container.Register<IAlarmService>((x) => new UwpAlarmService(
                container.Resolve<INotificationService>(),
                container.Resolve<IPlaylistService>(),
                container.Resolve<IMediaCacheService>(),
                container.Resolve<ScheduleDbContext>()));

            container.Register<IPreviewPlayService>((x) => new PreviewPlayService(container.Resolve<MediaPlayer>()));
            container.Register((x) =>
            {
                var player = new MediaPlayer();
                player.AutoPlay = false;
                return player;
            });

            container.Register<IPlaybackService>((x) => new PlaybackService(container.Resolve<MediaPlayer>(),
                                                            container.Resolve<IPlaylistService>(),
                                                            container.Resolve<IMediaCacheService>(),
                                                            container.Resolve<IAlarmService>()), isSingleton: true);


            Container = container;
        }
    }
}