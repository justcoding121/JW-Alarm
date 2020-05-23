namespace Bible.Alarm.Services.Droid
{
    using Android.App;
    using Android.Media;
    using Bible.Alarm.Contracts.Battery;
    using Bible.Alarm.Contracts.Platform;
    using Bible.Alarm.Droid.Services.Battery;
    using Bible.Alarm.Droid.Services.Handlers;
    using Bible.Alarm.Droid.Services.Platform;
    using Bible.Alarm.Droid.Services.Storage;
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

            container.Register<IToastService>((x) => new DroidToastService(container));

            container.Register<INotificationService>((x) => new DroidNotificationService(container));

            container.Register<IPreviewPlayService>((x) => new PreviewPlayService(container, container.Resolve<MediaPlayer>()));
            container.Register((x) =>
            {
                var player = new MediaPlayer();
                return player;
            });

            container.Register<IStorageService>((x) => new AndroidStorageService());

            container.Register((x) =>
            {
                var storageService = container.Resolve<IStorageService>();
                string databasePath = storageService.StorageRoot;

                var scheduleDbConfig = new DbContextOptionsBuilder<ScheduleDbContext>()
                    .UseSqlite($"Filename={Path.Combine(databasePath, "bibleAlarm.db")}").Options;
                return new ScheduleDbContext(scheduleDbConfig);
            });


            container.Register((x) =>
            {
                var storageService = container.Resolve<IStorageService>();
                string databasePath = storageService.StorageRoot;

                var mediaDbConfig = new DbContextOptionsBuilder<MediaDbContext>()
                    .UseSqlite($"Filename={Path.Combine(databasePath, "mediaIndex.db")}").Options;
                return new MediaDbContext(mediaDbConfig);
            });

            container.Register((x) =>
            {
                return CrossMediaManager.Current;
            });

            container.Register<IBatteryOptimizationManager>((x) => new BatteryOptimizationManager(container));
            container.Register<IVersionFinder>((x) => new VersionFinder());
            container.Register<AndroidAlarmHandler>((x) => new AndroidAlarmHandler(container.Resolve<IMediaManager>(),
                                                        container.Resolve<IPlaybackService>()));
        }
    }
}