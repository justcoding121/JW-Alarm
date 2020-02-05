namespace Bible.Alarm.Services.Droid
{
    using Android.Media;
    using Bible.Alarm.Contracts.Battery;
    using Bible.Alarm.Contracts.Network;
    using Bible.Alarm.Contracts.Platform;
    using Bible.Alarm.Droid.Services.Battery;
    using Bible.Alarm.Droid.Services.Network;
    using Bible.Alarm.Droid.Services.Platform;
    using Bible.Alarm.Services.Contracts;
    using Bible.Alarm.Services.Droid.Tasks;
    using MediaManager;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.IO;
    using System.Net.Http;
    using Xamarin.Android.Net;

    public static class IocSetup
    {
        public static void Initialize(IContainer container, bool isService)
        {
            container.Register<HttpMessageHandler>((x) => new AndroidClientHandler());

            if (!isService)
            {
                container.Register<IToastService>((x) => new DroidToastService(container));
            }

            container.Register<INotificationService>((x) => new DroidNotificationService(container));

            container.Register((x) => new SchedulerTask(container.Resolve<ScheduleDbContext>(),
                                    container.Resolve<IMediaCacheService>(), container.Resolve<IAlarmService>(),
                                    container.Resolve<INotificationService>()));


            container.Register<IPreviewPlayService>((x) => new PreviewPlayService(container, container.Resolve<MediaPlayer>()));
            container.Register((x) =>
            {
                var player = new MediaPlayer();
                return player;
            });

            container.Register<IPlaybackService>((x) => new PlaybackService(container.Resolve<IMediaManager>(),
                container.Resolve<IPlaylistService>(),
                container.Resolve<IAlarmService>(),
                container.Resolve<IMediaCacheService>(),
                container.Resolve<IStorageService>(),
                container.Resolve<INetworkStatusService>(),
                container.Resolve<INotificationService>()));


            string databasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var scheduleDbConfig = new DbContextOptionsBuilder<ScheduleDbContext>()
                .UseSqlite($"Filename={Path.Combine(databasePath, "bibleAlarm.db")}").Options;

            container.Register((x) => new ScheduleDbContext(scheduleDbConfig));

            var mediaDbConfig = new DbContextOptionsBuilder<MediaDbContext>()
                .UseSqlite($"Filename={Path.Combine(databasePath, "mediaIndex.db")}").Options;

            container.Register((x) => new MediaDbContext(mediaDbConfig));
            container.RegisterSingleton((x) =>
            {
                return CrossMediaManager.Current;

            });

            container.Register<INetworkStatusService>((x) => new NetworkStatusService(container));
            container.Register<IBatteryOptimizationManager>((x) => new BatteryOptimizationManager(container));
            container.Register<IVersionFinder>((x) => new VersionFinder());

        }
    }
}