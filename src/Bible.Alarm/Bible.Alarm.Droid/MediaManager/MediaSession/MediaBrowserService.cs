using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using AndroidX.Core.Content;
using AndroidX.Media;
using Bible.Alarm;
using Bible.Alarm.Droid;
using Bible.Alarm.Droid.Services.Platform;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Infrastructure;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Ext.Mediasession;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Ext.Cast;
using MediaManager.Platforms.Android.Media;
using NLog;
using Android.Gms.Cast.Framework;
using MediaManager.Platforms.Android.Player;
using MediaManager.Library;
using System.IO;
using AndroidX.Media.Session;

namespace MediaManager.Platforms.Android.MediaSession
{
    [Service(Exported = true, Enabled = true)]
    [IntentFilter(new[] { global::Android.Service.Media.MediaBrowserService.ServiceInterface })]
    public class MediaBrowserService : MediaBrowserServiceCompat
    {

        private Logger logger => LogManager.GetCurrentClassLogger();

        protected MediaManagerImplementation MediaManager => (MediaManagerImplementation)CrossMediaManager.Current;
        protected MediaDescriptionAdapter MediaDescriptionAdapter { get; set; }
        protected PlayerNotificationManager PlayerNotificationManager
        {
            get => (MediaManager.Notification as Notifications.NotificationManager).PlayerNotificationManager;
            set => (MediaManager.Notification as Notifications.NotificationManager).PlayerNotificationManager = value;
        }
        protected MediaControllerCompat MediaController => MediaManager.MediaController;

        protected NotificationListener NotificationListener { get; set; }

        public readonly string ChannelId = "com.jthomas.info.Bible.Alarm.NOW_PLAYING";
        public readonly int NotificationId = 0xb339;

        public bool IsForegroundService = false;

        private IContainer container;

        public MediaBrowserService()
        {
            LogSetup.Initialize(VersionFinder.Default,
                new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" }, Xamarin.Forms.Device.Android);
        }

        protected MediaBrowserService(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }

        private IMediaItem relavantMedia;

        public IPlayer CurrentPlayer { get => MediaManager.AndroidMediaPlayer.CurrentPlayer; set { MediaManager.AndroidMediaPlayer.CurrentPlayer = value; } }
        public SimpleExoPlayer ExoPlayer => MediaManager.AndroidMediaPlayer.ExoPlayer;
        public CastPlayer CastPlayer => castPlayer.Value;

        private Lazy<CastPlayer> castPlayer;

        private PlayerEventListener playerListener = new PlayerEventListener();

        public override void OnCreate()
        {
            base.OnCreate();

            try
            {
                container = BootstrapHelper.InitializeService(this);
            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened when calling BootstrapHelper from MediaBrowserService.");
            }

            //create media session and connect
            var mediaManager = container.Resolve<IMediaManager>() as MediaManagerImplementation;

            //async prepare call here
            var prepareMediaTask = Task.Run(async () => await prepareMedia());

            castPlayer = new Lazy<CastPlayer>(() =>
            {
                try
                {
                    var castContext = CastContext.GetSharedInstance(this);
                    var player = new CastPlayer(castContext);
                    player.SetSessionAvailabilityListener(new CastSessionAvailabilityListener(this));
                    return player;
                }
                catch (Exception e)
                {
                    logger.Error(e, "Cast is not available on this device");
                    return null;
                }
            });

            try
            {

                playerListener.OnPlayerErrorImpl += onPlayerError;
                playerListener.OnPlayerStateChangedImpl += onPlayerStateChanged;

                NotificationListener = new NotificationListener();

                NotificationListener.OnNotificationPostedImpl += onNotificationPosted;
                NotificationListener.OnNotificationCancelledImpl += onNotificationCancelled;

                var mediaSession = MediaManager.MediaSession = new MediaSessionCompat(this, nameof(MediaBrowserService));
                mediaSession.SetSessionActivity(MediaManager.SessionActivityPendingIntent);
                mediaSession.Active = true;

                SessionToken = mediaSession.SessionToken;

                MediaDescriptionAdapter = new MediaDescriptionAdapter(new MediaControllerCompat(this, SessionToken));
                PlayerNotificationManager = PlayerNotificationManager.CreateWithNotificationChannel(
                    this,
                    ChannelId,
                    Resource.String.notification_channel,
                    Resource.String.notification_channel_description,
                    NotificationId,
                    MediaDescriptionAdapter,
                    NotificationListener);

                PlayerNotificationManager.SetMediaSessionToken(SessionToken);
                PlayerNotificationManager.SetSmallIcon(MediaManager.NotificationIconResource);

                PlayerNotificationManager.SetRewindIncrementMs((long)MediaManager.StepSizeBackward.TotalMilliseconds);
                PlayerNotificationManager.SetFastForwardIncrementMs((long)MediaManager.StepSizeForward.TotalMilliseconds);

                PlayerNotificationManager.SetUsePlayPauseActions(MediaManager.Notification.ShowPlayPauseControls);
                PlayerNotificationManager.SetUseNavigationActions(MediaManager.Notification.ShowNavigationControls);

                mediaManager.Init(Application.Context);
                mediaManager.AndroidMediaPlayer.Initialize();

                mediaSessionConnector = mediaManager.AndroidMediaPlayer.MediaSessionConnector;

                logger.Info("Cast session available:" + CastPlayer.IsCastSessionAvailable);

                SwitchToPlayer(null, CastPlayer != null && CastPlayer.IsCastSessionAvailable ? CastPlayer : ExoPlayer);

                PlayerNotificationManager.SetPlayer(CurrentPlayer);

                relavantMedia = prepareMediaTask.Result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error happened when initializing MediaBrowserService");
            }

            logger.Info($"Service start.  Queue Count: #{MediaManager.Queue.Count}");
        }

        private class CastSessionAvailabilityListener : Java.Lang.Object, ISessionAvailabilityListener
        {
            private MediaBrowserService service;
            public CastSessionAvailabilityListener(MediaBrowserService service)
            {
                this.service = service;
            }

            public void OnCastSessionUnavailable()
            {
                service.SwitchToPlayer(service.CurrentPlayer, service.CastPlayer);
            }

            public void OnCastSessionAvailable()
            {
                service.SwitchToPlayer(service.CurrentPlayer, service.ExoPlayer);
            }
        }

        public void SwitchToPlayer(IPlayer previousPlayer, IPlayer newPlayer)
        {
            if (previousPlayer == newPlayer)
            {
                return;
            }

            CurrentPlayer = newPlayer;
            if (previousPlayer != null)
            {
                previousPlayer.RemoveListener(playerListener);

                var playbackState = previousPlayer.PlaybackState;
                if (MediaManager.Queue.Count == 0)
                {
                    CurrentPlayer.Stop(true);
                }
                else if (playbackState != IPlayer.StateIdle && playbackState != IPlayer.StateEnded)
                {
                    //prepare playlist
                }
            }

            newPlayer.AddListener(playerListener);
            mediaSessionConnector.SetPlayer(newPlayer);
            previousPlayer?.Stop(true);
        }

        private void onPlayerError(ExoPlaybackException e)
        {
            switch (e.Type)
            {
                case ExoPlaybackException.TypeSource:
                    logger.Error(e, e.SourceException.Message);
                    break;
                case ExoPlaybackException.TypeRenderer:
                    logger.Error(e, e.RendererException.Message);
                    break;
                case ExoPlaybackException.TypeUnexpected:
                    logger.Error(e, e.UnexpectedException.Message);
                    break;
                case ExoPlaybackException.TypeOutOfMemory:
                    logger.Error(e, e.OutOfMemoryError.Message);
                    break;
                case ExoPlaybackException.TypeRemote:
                    logger.Error(e, e.Message);
                    break;
            }

            using var toastService = container.Resolve<IToastService>();
            toastService.ShowMessage("An error happened while playback.");
        }

        private void onPlayerStateChanged(bool playWhenReady, int playbackState)
        {
            switch (playbackState)
            {
                case IPlayer.StateBuffering:
                case IPlayer.StateReady:
                    PlayerNotificationManager.SetPlayer(CurrentPlayer);
                    if (playbackState == IPlayer.StateReady)
                    {
                        if (!playWhenReady)
                        {
                            StopForeground(false);
                            IsForegroundService = false;
                        }
                    }
                    break;
                default:
                    PlayerNotificationManager.SetPlayer(null);
                    break;
            }
        }

        private const string channelName = "Now Playing";
        private const string channelDescription = "New World Translation";

        private MediaSessionConnector mediaSessionConnector;

        private void onNotificationCancelled(int notificationId, bool dismissedByUser)
        {
            logger.Info($"Notification cancelled. IsDismissedByUser:{dismissedByUser}, IsForeground: {IsForegroundService}");

            StopForeground(true);
            IsForegroundService = false;

            StopSelf();
        }

        private void onNotificationPosted(int notificationId, Notification notification, bool isOnGoing)
        {
            logger.Info($"Notification posted. IsOngoing:{isOnGoing}, IsForeground: {IsForegroundService}, " +
                  $"Queue Count: #{MediaManager.Queue.Count}");

            //playing state
            if (isOnGoing && !IsForegroundService)
            {
                ContextCompat.StartForegroundService(ApplicationContext, new Intent(ApplicationContext, Java.Lang.Class.FromType(typeof(MediaBrowserService))));
                StartForeground(notificationId, notification);
                IsForegroundService = true;
            }
        }

        public override StartCommandResult OnStartCommand(Intent startIntent, StartCommandFlags flags, int startId)
        {
            if (startIntent != null)
            {
                MediaButtonReceiver.HandleIntent(MediaManager.MediaSession, startIntent);
            }
            return StartCommandResult.Sticky;
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            logger.Info("Task removed.");
            base.OnTaskRemoved(rootIntent);

            CurrentPlayer.Stop(true);

            if (IsForegroundService)
            {
                StopForeground(true);
            }

            StopSelf();
        }

        public override void OnDestroy()
        {
            logger.Info("Service stop.");

            try
            {
                playerListener.OnPlayerErrorImpl -= onPlayerError;
                playerListener.OnPlayerStateChangedImpl -= onPlayerStateChanged;

                NotificationListener.OnNotificationPostedImpl -= onNotificationPosted;
                NotificationListener.OnNotificationCancelledImpl -= onNotificationCancelled;

                MediaManager.MediaSession.Active = false;
                MediaManager.MediaSession.Release();

                (MediaManager.Notification as Notifications.NotificationManager).Player = null;

                MediaDescriptionAdapter.Dispose();
                MediaDescriptionAdapter = null;

                PlayerNotificationManager.SetPlayer(null);
                PlayerNotificationManager.Dispose();
                PlayerNotificationManager = null;

                NotificationListener.Dispose();
                NotificationListener = null;

                MediaManager.MediaSession.Active = false;
                MediaManager.MediaSession.Release();
                MediaManager.MediaSession = null;

                if (CastPlayer != null)
                {
                    CastPlayer.Release();
                    CastPlayer.Dispose();
                }
            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened when disposing MediaBrowserService");
            }

            base.OnDestroy();
        }

        private const string MEDIA_SEARCH_SUPPORTED = "android.media.browse.SEARCH_SUPPORTED";
        private const string CONTENT_STYLE_SUPPORTED = "android.media.browse.CONTENT_STYLE_SUPPORTED";

        private const string UAMP_BROWSABLE_ROOT = "/";
        private const string UAMP_RECENT_ROOT = "__RECENT__";

        public override BrowserRoot OnGetRoot(string clientPackageName, int clientUid, Bundle rootHints)
        {
            var rootExtras = new Bundle();

            rootExtras.PutBoolean(MEDIA_SEARCH_SUPPORTED, false);
            rootExtras.PutBoolean(CONTENT_STYLE_SUPPORTED, false);

            var isRecentRequest = rootHints != null && rootHints.GetBoolean(BrowserRoot.ExtraRecent);
            var browserRootPath = isRecentRequest ? UAMP_RECENT_ROOT : UAMP_BROWSABLE_ROOT;

            return new BrowserRoot(browserRootPath, rootExtras);
        }

        public override void OnLoadChildren(string parentId, Result result)
        {
            logger.Info($"On load children parent {parentId}.  Queue Count: #{MediaManager.Queue.Count}. PlaybackState: #{MediaManager.State}");

            try
            {
                if (MediaManager.Queue.Count > 0)
                {
                    logger.Info("Setting media as result from queue.");
                    setResult();
                    return;
                }

                if (relavantMedia != null)
                {
                    result.SendResult(new JavaList<MediaBrowserCompat.MediaItem>() { relavantMedia.ToMediaBrowserMediaItem() });
                    logger.Info("Set relavant media as result.");
                    return;
                }

                logger.Info("Returning empty list of children.");

                result.SendResult(new JavaList<MediaBrowserCompat.MediaItem>());
                return;
            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened when loading children.");
            }

            void setResult()
            {
                var mediaItems = new JavaList<MediaBrowserCompat.MediaItem>();

                foreach (var item in MediaManager.Queue)
                    mediaItems.Add(item.ToMediaBrowserMediaItem());

                result.SendResult(mediaItems);
                logger.Info($"On load children exit.  Queue Count: #{MediaManager.Queue.Count}. PlaybackState: #{MediaManager.State}");

            }
        }

        private async Task<IMediaItem> prepareMedia()
        {
            try
            {
                if (MediaManager.Queue.Count > 0)
                {
                    return MediaManager.Queue[0];
                }

                using var playlistService = container.Resolve<IPlaylistService>();      

                var lastPlayed = await playlistService.GetRelavantScheduleToPlay();
                var nextTrack = await playlistService.NextTrack(lastPlayed);

                using var cacheService = container.Resolve<IMediaCacheService>();

                var mediaExtractor = MediaManager.Extractor;

                if (await cacheService.Exists(nextTrack.Url))
                {
                    return await mediaExtractor.CreateMediaItem(new FileInfo(cacheService.GetCacheFilePath(nextTrack.Url)));
                }

                return await mediaExtractor.CreateMediaItem(nextTrack.Url);

            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened when calling AlarmHandler from PlaybackPreparer.");
            }

            return null;
        }

        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            BootstrapHelper.Remove(this);

            disposed = true;
            base.Dispose(disposing);
        }
    }
}
