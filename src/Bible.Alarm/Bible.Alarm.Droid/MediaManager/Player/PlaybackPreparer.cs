using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media.Session;
using Bible.Alarm;
using Bible.Alarm.Droid.Services.Platform;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Infrastructure;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Ext.Mediasession;
using Com.Google.Android.Exoplayer2.Source;
using MediaManager.Platforms.Android.Media;
using NLog;

namespace MediaManager.Platforms.Android.Player
{
    public class MediaSessionConnectorPlaybackPreparer : Java.Lang.Object, MediaSessionConnector.IPlaybackPreparer
    {
        private Logger logger => LogManager.GetCurrentClassLogger();
        protected IExoPlayer _player;
        protected ConcatenatingMediaSource _mediaSource;

        private static SemaphoreSlim @lock = new SemaphoreSlim(1);
        private IContainer container;

        protected MediaManagerImplementation MediaManager => (MediaManagerImplementation)CrossMediaManager.Current;

        public MediaSessionConnectorPlaybackPreparer(IExoPlayer player, ConcatenatingMediaSource mediaSource)
        {
            LogSetup.Initialize(VersionFinder.Default,
             new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" }, Xamarin.Forms.Device.Android);

            container = BootstrapHelper.GetInitializedContainer();

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

        private async Task prepare()
        {
            if(MediaManager.Queue.Count > 0)
            {
                //Only in case of Prepare set PlayWhenReady to true because we use this to load in the whole queue
                _player.Prepare(_mediaSource);
                _player.PlayWhenReady = MediaManager.AutoPlay;
                return;
            }

            var playbackService = container.Resolve<IPlaybackService>();
            await playbackService.PrepareRelavantPlaylist();

            prepareMediaFromQueue();
            await playbackService.Play();
        }

        public async void OnPrepare(bool p0)
        {
            logger.Info($"On prepare called. AutoPlay: {MediaManager.AutoPlay}, Queue Count: #{MediaManager.Queue.Count}. PlaybackState: #{MediaManager.State}");
            await prepare();
        }

        public void OnPrepareFromMediaId(string p0, bool p1, Bundle p2)
        {
            logger.Info($"On prepare  from median Id called. AutoPlay: {MediaManager.AutoPlay}, Queue Count: #{MediaManager.Queue.Count}. PlaybackState: #{MediaManager.State}");

            prepareMediaFromQueue();
        }

        private void prepareMediaFromQueue()
        {
            _mediaSource.Clear();
            var windowIndex = 0;
            foreach (var mediaItem in MediaManager.Queue)
            {
                _mediaSource.AddMediaSource(mediaItem.ToMediaSource());
            }
            _player.Prepare(_mediaSource);
            _player.SeekTo(windowIndex, 0);
            _player.PlayWhenReady = true;
        }

        public void OnPrepareFromSearch(string p0, bool p1, Bundle p2)
        {

            logger.Info($"On prepare from search called. AutoPlay: {MediaManager.AutoPlay}, Queue Count: #{MediaManager.Queue.Count}. PlaybackState: #{MediaManager.State}");


            _mediaSource.Clear();
            foreach (var mediaItem in MediaManager.Queue.Where(x => x.Title == p0))
            {
                _mediaSource.AddMediaSource(mediaItem.ToMediaSource());
            }
            _player.Prepare(_mediaSource);
        }

        public void OnPrepareFromUri(global::Android.Net.Uri p0, bool p1, Bundle p2)
        {

            logger.Info($"On prepare from Uri called. AutoPlay: {MediaManager.AutoPlay}, Queue Count: #{MediaManager.Queue.Count}. PlaybackState: #{MediaManager.State}");


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
