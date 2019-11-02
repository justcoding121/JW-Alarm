namespace Bible.Alarm.Services.Droid
{
    using Android.Media;
    using Bible.Alarm.Services.Contracts;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.IO;
    using System.Net.Http;
    using Xamarin.Android.Net;
    using MediaManager;
    using Bible.Alarm.Services.Droid.Tasks;
    using Android.Content;
    using Com.Google.Android.Exoplayer2.UI;
    using Bible.Alarm.Contracts.Network;
    using Bible.Alarm.Droid.Services.Network;
    using Bible.Alarm.Contracts.Battery;
    using Bible.Alarm.Droid.Services.Battery;

    public static class IocSetup
    {
        internal static IContainer Container { private set; get; }
        public static bool IsService { get; private set; }
        public static Context Context { get; private set; }
        public static void Initialize(IContainer container, bool isService)
        {
            container.Register<HttpMessageHandler>((x) => new AndroidClientHandler());

            container.Register<IToastService>((x) => new DroidToastService());

            container.Register<INotificationService>((x) =>
            new DroidNotificationService());

            container.Register((x) => new SchedulerTask(container.Resolve<ScheduleDbContext>(),
                                    container.Resolve<IMediaCacheService>(), container.Resolve<IAlarmService>(),
                                    container.Resolve<INotificationService>()));


            container.Register<IPreviewPlayService>((x) => new PreviewPlayService(container.Resolve<MediaPlayer>()));
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


            string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

            var scheduleDbConfig = new DbContextOptionsBuilder<ScheduleDbContext>()
                .UseSqlite($"Filename={Path.Combine(databasePath, "bibleAlarm.db")}").Options;

            container.Register((x) => new ScheduleDbContext(scheduleDbConfig));

            var mediaDbConfig = new DbContextOptionsBuilder<MediaDbContext>()
                .UseSqlite($"Filename={Path.Combine(databasePath, "mediaIndex.db")}").Options;

            container.Register((x) => new MediaDbContext(mediaDbConfig));
            container.Register<IMediaManager>((x) =>
            {
                return CrossMediaManager.Current;

            }, true);

            container.Register<INetworkStatusService>((x) => new NetworkStatusService());
            container.Register<IBatteryOptimizationManager>((x) => new BatteryOptimizationManager());

            Container = container;
            IsService = isService;
        }

        public static void SetContext(Context context)
        {
            Context = context;
        }
    }
}