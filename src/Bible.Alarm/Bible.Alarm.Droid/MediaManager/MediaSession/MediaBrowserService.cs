using System;
using Android.App;
using Android.Content;
using Android.Media.Session;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using AndroidX.Core.Content;
using AndroidX.Media;
using AndroidX.Media.Session;
using Bible.Alarm;
using Bible.Alarm.Droid;
using Bible.Alarm.Droid.Services.Platform;
using Bible.Alarm.Services;
using Bible.Alarm.Services.Droid.Helpers;
using Bible.Alarm.Services.Infrastructure;
using Com.Google.Android.Exoplayer2.UI;
using MediaManager.Platforms.Android.Media;
using NLog;

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

        public readonly string ChannelId = "audio_channel";
        public readonly int ForegroundNotificationId = 1;
        public bool IsForeground = false;

        private IContainer container;

        public MediaBrowserService()
        {
            LogSetup.Initialize(VersionFinder.Default,
                new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" }, Xamarin.Forms.Device.Android);
        }

        protected MediaBrowserService(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }

        public override void OnCreate()
        {
            base.OnCreate();

            try
            {
                container = BootstrapHelper.InitializeService(this);
                container.Resolve<IMediaManager>();
            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened when calling BootstrapHelper from MediaBrowserService.");
            }

            try
            {
                PrepareMediaSession();
                PrepareNotificationManager();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error happened when initializing MediaBrowserService");
            }

            MediaManager.StateChanged += MediaManager_StateChanged;
            logger.Info($"Service start.  Queue Count: #{MediaManager.Queue.Count}");
        }

        private void MediaManager_StateChanged(object sender, MediaManager.Playback.StateChangedEventArgs e)
        {
            logger.Info($"Player state change:{e.State}");

            switch (e.State)
            {
                case global::MediaManager.Player.MediaPlayerState.Failed:
                case global::MediaManager.Player.MediaPlayerState.Stopped:
                    PlayerNotificationManager.SetPlayer(null);
                    break;
                case global::MediaManager.Player.MediaPlayerState.Loading:
                case global::MediaManager.Player.MediaPlayerState.Buffering:
                case global::MediaManager.Player.MediaPlayerState.Playing:
                    PlayerNotificationManager.SetPlayer(MediaManager.Player);
                    break;
                case global::MediaManager.Player.MediaPlayerState.Paused:
                    if (IsForeground)
                    {
                        StopForeground(false);
                        IsForeground = false;
                    }
                    break;
                default:
                    break;
            }
        }

        protected virtual void PrepareMediaSession()
        {
            var mediaSession = MediaManager.MediaSession = new MediaSessionCompat(this, nameof(MediaBrowserService));
            mediaSession.SetSessionActivity(MediaManager.SessionActivityPendingIntent);
            mediaSession.Active = true;

            SessionToken = mediaSession.SessionToken;

            mediaSession.SetFlags((int)MediaSessionFlags.HandlesMediaButtons |
                                   (int)MediaSessionFlags.HandlesTransportControls);
        }

        protected virtual void PrepareNotificationManager()
        {
            NotificationListener = new NotificationListener();

            MediaDescriptionAdapter = new MediaDescriptionAdapter();
            PlayerNotificationManager = PlayerNotificationManager.CreateWithNotificationChannel(
                this,
                ChannelId,
                Resource.String.exo_download_notification_channel_name,
                0,
                ForegroundNotificationId,
                MediaDescriptionAdapter,
                NotificationListener);

            //Needed for enabling the notification as a mediabrowser.
            PlayerNotificationManager.NotificationPosted += onNotificationPosted;
            PlayerNotificationManager.NotificationCancelled += onNotificationCancelled;

            PlayerNotificationManager.SetFastForwardIncrementMs((long)MediaManager.StepSizeForward.TotalMilliseconds);
            PlayerNotificationManager.SetRewindIncrementMs((long)MediaManager.StepSizeBackward.TotalMilliseconds);

            PlayerNotificationManager.SetMediaSessionToken(SessionToken);
            PlayerNotificationManager.SetUsePlayPauseActions(MediaManager.Notification.ShowPlayPauseControls);
            PlayerNotificationManager.SetUseNavigationActions(MediaManager.Notification.ShowNavigationControls);
            PlayerNotificationManager.SetSmallIcon(MediaManager.NotificationIconResource);

            //TODO: Check for cast Player here

            //Must be called to start the connection
            (MediaManager.Notification as Notifications.NotificationManager).Player = MediaManager.Player;

        }
        private void onNotificationPosted(object sender, PlayerNotificationManager.NotificationPostedEventArgs e)
        {
            logger.Info($"Notification posted. IsOngoing:{e.Ongoing}, IsForeground: {IsForeground}, " +
                $"Queue Count: #{MediaManager.Queue.Count}");

            //playing state
            if (e.Ongoing && !IsForeground)
            {
                ContextCompat.StartForegroundService(ApplicationContext, new Intent(ApplicationContext, Java.Lang.Class.FromType(typeof(MediaBrowserService))));
                StartForeground(e.NotificationId, e.Notification);
                IsForeground = true;
                return;
            }

            //paused state
            if (!e.Ongoing && IsForeground)
            {
                StopForeground(false);
                IsForeground = false;
            }
        }

        private void onNotificationCancelled(object sender, PlayerNotificationManager.NotificationCancelledEventArgs e)
        {
            logger.Info($"Notification cancelled. IsDismissedByUser:{e.DismissedByUser}, IsForeground: {IsForeground}");

            if (IsForeground)
            {
                StopForeground(true);
                IsForeground = false;
            }

            if (e.DismissedByUser || Build.VERSION.SdkInt <= BuildVersionCodes.Q)
            {
                StopSelf();
            }
        }

        public override StartCommandResult OnStartCommand(Intent startIntent, StartCommandFlags flags, int startId)
        {
            logger.Info("Start command.");

            if (startIntent != null)
            {
                MediaButtonReceiver.HandleIntent(MediaManager.MediaSession, startIntent);
            }

            return StartCommandResult.Sticky;
        }

        public async override void OnTaskRemoved(Intent rootIntent)
        {
            logger.Info("Task removed.");
            await MediaManager.Stop();
            base.OnTaskRemoved(rootIntent);

            if (IsForeground)
            {
                StopForeground(true);
                IsForeground = false;
            }

            StopSelf();
        }

        public override void OnDestroy()
        {
            logger.Info("Service stop.");

            try
            {
                MediaManager.StateChanged -= MediaManager_StateChanged;
                PlayerNotificationManager.NotificationPosted -= onNotificationPosted;
                PlayerNotificationManager.NotificationCancelled -= onNotificationCancelled;

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
            }
            catch (Exception e)
            {
                logger.Error(e, "An error happened when disposing MediaBrowserService");
            }

            base.OnDestroy();
        }

        public override BrowserRoot OnGetRoot(string clientPackageName, int clientUid, Bundle rootHints)
        {
            return new BrowserRoot(nameof(ApplicationContext.ApplicationInfo.Name), null);
        }

        public override void OnLoadChildren(string parentId, Result result)
        {
            var mediaItems = new JavaList<MediaBrowserCompat.MediaItem>();

            foreach (var item in MediaManager.Queue)
                mediaItems.Add(item.ToMediaBrowserMediaItem());

            result.SendResult(mediaItems);
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
