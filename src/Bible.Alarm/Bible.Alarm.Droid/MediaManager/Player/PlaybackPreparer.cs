using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V4.Media.Session;
using Bible.Alarm;
using Bible.Alarm.Common.Extensions;
using Bible.Alarm.Contracts.Media;
using Bible.Alarm.Droid.Services.Platform;
using Bible.Alarm.Models;
using Bible.Alarm.Services;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Infrastructure;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Ext.Mediasession;
using Com.Google.Android.Exoplayer2.Source;
using MediaManager.Platforms.Android.Media;
using MediaManager.Platforms.Android.MediaSession;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace MediaManager.Platforms.Android.Player
{
    public class MediaSessionConnectorPlaybackPreparer : Java.Lang.Object, MediaSessionConnector.IPlaybackPreparer
    {
        private Logger logger => LogManager.GetCurrentClassLogger();
        protected IExoPlayer _player;
        protected ConcatenatingMediaSource _mediaSource;

        private static SemaphoreSlim @lock = new SemaphoreSlim(1);

        protected MediaManagerImplementation MediaManager => (MediaManagerImplementation)CrossMediaManager.Current;

        public MediaSessionConnectorPlaybackPreparer(IExoPlayer player, ConcatenatingMediaSource mediaSource)
        {
            LogSetup.Initialize(VersionFinder.Default,
             new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" }, Xamarin.Forms.Device.Android);

            _player = player;
            _mediaSource = mediaSource;
        }

        protected MediaSessionConnectorPlaybackPreparer(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        //public long SupportedPrepareActions => MediaSessionConnector.IPlaybackPreparer.Actions;
        public long SupportedPrepareActions =>
            PlaybackStateCompat.ActionPrepare |
            PlaybackStateCompat.ActionPrepareFromMediaId |
            PlaybackStateCompat.ActionPrepareFromSearch |
            PlaybackStateCompat.ActionPrepareFromUri |
            PlaybackStateCompat.ActionPlayFromMediaId |
            PlaybackStateCompat.ActionPlayFromSearch |
            PlaybackStateCompat.ActionPlayFromUri;

        public bool OnCommand(IPlayer p0, IControlDispatcher p1, string p2, Bundle p3, ResultReceiver p4)
        {
            logger.Info($"On command called.  Queue Count: #{MediaManager.Queue.Count}. PlaybackState: #{MediaManager.State}");
            return false;
        }


        public async void OnPrepare(bool p0)
        {
            logger.Info($"On prepare called.  Queue Count: #{MediaManager.Queue.Count}. PlaybackState: #{MediaManager.State}");

            await @lock.WaitAsync();
            try
            {
                logger.Info($"On prepare called inside lock.  Queue Count: #{MediaManager.Queue.Count}. PlaybackState: #{MediaManager.State}");

                if (MediaManager.IsPlaying())
                {
                    return;
                }

                try
                {
                    var container = BootstrapHelper.GetInitializedContainer();
                    using var scheduleDbContext = container.Resolve<ScheduleDbContext>();

                    var lastSchedule = await scheduleDbContext.GeneralSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Key == "AndroidLastPlayedScheduleId");

                    if (MediaManager.Queue.Count > 0 && (lastSchedule == null || lastSchedule.Value == "-1"))
                    {
                        //Only in case of Prepare set PlayWhenReady to true because we use this to load in the whole queue
                        _player.Prepare(_mediaSource);
                        _player.PlayWhenReady = MediaManager.AutoPlay;
                        return;
                    }

                    AlarmSchedule schedule = null;
                    if (lastSchedule != null && lastSchedule.Value != "-1")
                    {
                        schedule = await scheduleDbContext.AlarmSchedules.FirstOrDefaultAsync(x => x.Id == long.Parse(lastSchedule.Value));
                    }

                    if (schedule == null)
                    {
                        schedule = await scheduleDbContext.AlarmSchedules.FirstOrDefaultAsync();
                    }

                    if (schedule == null)
                    {
                        using var mediaDbContext = container.Resolve<MediaDbContext>();
                        schedule = await AlarmSchedule.GetSampleSchedule(false, mediaDbContext);
                        scheduleDbContext.Add(schedule);
                        await scheduleDbContext.SaveChangesAsync();
                    }

                    await MediaManager.StopEx();

                    var handler = container.Resolve<IAndroidAlarmHandler>();
                    await handler.Handle(schedule.Id, true);
                    logger.Info($"On prepare exits lock.  Queue Count: #{MediaManager.Queue.Count}. PlaybackState: #{MediaManager.State}");

                    return;
                }
                catch (Exception e)
                {
                    logger.Error(e, "An error happened when calling AlarmHandler from PlaybackPreparer.");
                }
            }
            finally
            {
                @lock.Release();
            }

        }

        public void OnPrepareFromMediaId(string p0, bool p1, Bundle p2)
        {
            _mediaSource.Clear();
            var windowIndex = 0;
            foreach (var mediaItem in MediaManager.Queue)
            {
                if (mediaItem.Id == p0)
                    windowIndex = MediaManager.Queue.IndexOf(mediaItem);

                _mediaSource.AddMediaSource(mediaItem.ToMediaSource());
            }
            _player.Prepare(_mediaSource);
            _player.SeekTo(windowIndex, 0);
        }

        public void OnPrepareFromSearch(string p0, bool p1, Bundle p2)
        {
            _mediaSource.Clear();
            foreach (var mediaItem in MediaManager.Queue.Where(x => x.Title == p0))
            {
                _mediaSource.AddMediaSource(mediaItem.ToMediaSource());
            }
            _player.Prepare(_mediaSource);
        }

        public void OnPrepareFromUri(global::Android.Net.Uri p0, bool p1, Bundle p2)
        {
            _mediaSource.Clear();
            var windowIndex = 0;
            foreach (var mediaItem in MediaManager.Queue)
            {
                var uri = global::Android.Net.Uri.Parse(mediaItem.MediaUri);
                if (uri.Equals(p0))
                    windowIndex = MediaManager.Queue.IndexOf(mediaItem);

                _mediaSource.AddMediaSource(mediaItem.ToMediaSource());
            }
            _player.Prepare(_mediaSource);
            _player.SeekTo(windowIndex, 0);
        }

    }
}
