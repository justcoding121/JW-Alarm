namespace JW.Alarm.Services.Droid
{
    using Android.Media;
    using JW.Alarm.Services.Contracts;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.IO;
    using System.Net.Http;
    using Xamarin.Android.Net;
    using MediaManager;
    using JW.Alarm.Services.Droid.Tasks;

    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(IContainer container)
        {
            container.Register<HttpMessageHandler>((x) => new AndroidClientHandler());

            container.Register<IToastService>((x) => new DroidPopUpService());

            container.Register<INotificationService>((x) =>
            new DroidNotificationService());

            //container.Register((x) => new SnoozeDismissTask(container.Resolve<IPlaybackService>()));


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
                container.Resolve<IMediaCacheService>()));


            string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

            var scheduleDbConfig = new DbContextOptionsBuilder<ScheduleDbContext>()
                .UseSqlite($"Filename={Path.Combine(databasePath, "bibleAlarm.db")}").Options;

            container.Register((x) => new ScheduleDbContext(scheduleDbConfig));

            var mediaDbConfig = new DbContextOptionsBuilder<MediaDbContext>()
                .UseSqlite($"Filename={Path.Combine(databasePath, "mediaIndex.db")}").Options;

            container.Register((x) => new MediaDbContext(mediaDbConfig));
            container.Register<IMediaManager>((x) =>
            {
                var mediaManager = new MediaManagerImplementation();
                CrossMediaManager.Current = mediaManager;
                CrossMediaManager.Current.Init();
               
                return mediaManager;

            }, true);

            container.Register<IAlarmService>((x) => new AlarmService(
                container.Resolve<INotificationService>(),
                container.Resolve<IPlaylistService>(),
                container.Resolve<IMediaCacheService>(),
                container.Resolve<ScheduleDbContext>()));

            Container = container;
        }
    }
}