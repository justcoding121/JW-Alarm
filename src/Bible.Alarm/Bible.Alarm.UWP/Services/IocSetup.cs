namespace JW.Alarm.Services.Uwp
{
    using JW.Alarm.Services.Contracts;
    using JW.Alarm.Services.Uwp.Tasks;
    using JW.Alarm.Services.UWP;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.IO;
    using System.Net.Http;
    using Windows.Media.Playback;

    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(IContainer container)
        {
            container.Register<HttpMessageHandler>((x) => new HttpClientHandler());

            container.Register<IPopUpService>((x) => new UwpPopUpService());

            container.Register<INotificationService>((x) =>
            new UwpNotificationService(container.Resolve<IMediaCacheService>()));

            container.Register((x) => new AlarmTask(container.Resolve<IPlaybackService>()));

            container.Register((x) => new SnoozeDismissTask(container.Resolve<IPlaybackService>()));


            container.Register((x) => new SchedulerTask(container.Resolve<ScheduleDbContext>(),
                                    container.Resolve<IMediaCacheService>(), container.Resolve<IAlarmService>(),
                                    container.Resolve<INotificationService>()));

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
                                                            container.Resolve<IAlarmService>()));


            string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

            var scheduleDbConfig = new DbContextOptionsBuilder<ScheduleDbContext>()
                .UseSqlite($"Filename={Path.Combine(databasePath, "bibleAlarm.db")}").Options;

            container.Register((x) => new ScheduleDbContext(scheduleDbConfig));

            var mediaDbConfig = new DbContextOptionsBuilder<MediaDbContext>()
                .UseSqlite($"Filename={Path.Combine(databasePath, "mediaIndex.db")}").Options;

            container.Register((x) => new MediaDbContext(mediaDbConfig));

            Container = container;
        }
    }
}