namespace JW.Alarm.Services.Droid
{
    using Android.Media;
    using JW.Alarm.Services.Contracts;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.IO;
    using System.Net.Http;
    using Xamarin.Android.Net;

    public static class IocSetup
    {
        internal static IContainer Container;
        public static void Initialize(IContainer container)
        {
            container.Register<HttpMessageHandler>((x) => new AndroidClientHandler());

            container.Register<IPopUpService>((x) => new DroidPopUpService());

            container.Register<INotificationService>((x) =>
            new DroidNotificationService(container.Resolve<IMediaCacheService>()));

            //container.Register((x) => new AlarmTask(container.Resolve<IPlaybackService>()));

            //container.Register((x) => new SnoozeDismissTask(container.Resolve<IPlaybackService>()));


            //container.Register((x) => new SchedulerTask(container.Resolve<ScheduleDbContext>(),
            //                        container.Resolve<IMediaCacheService>(), container.Resolve<IAlarmService>(),
            //                        container.Resolve<INotificationService>()));


            container.Register<IPreviewPlayService>((x) => new PreviewPlayService(container.Resolve<MediaPlayer>()));
            container.Register((x) =>
            {
                var player = new MediaPlayer();
                return player;
            });

            container.Register<IPlaybackService>((x) => new PlaybackService(container.Resolve<MediaPlayer>(),
                                                            container.Resolve<IPlaylistService>(),
                                                            container.Resolve<IMediaCacheService>(),
                                                            container.Resolve<IAlarmService>()));
          
            //SQLitePCL.Batteries_V2.Init();
            //databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");

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