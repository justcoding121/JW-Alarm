﻿using System;
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
using Com.Google.Android.Exoplayer2.Ext.Cast;
using Com.Google.Android.Exoplayer2.Ext.Mediasession;
using Com.Google.Android.Exoplayer2.Source;
using MediaManager.Platforms.Android.Media;
using NLog;

namespace MediaManager.Platforms.Android.Player
{
    public class MediaSessionConnectorPlaybackPreparer : Java.Lang.Object,
        MediaSessionConnector.IPlaybackPreparer
    {
        private Logger logger => LogManager.GetCurrentClassLogger();

        private IContainer container;

        protected MediaManagerImplementation MediaManager => (MediaManagerImplementation)CrossMediaManager.Current;
        protected IPlayer currentPlayer => MediaManager.AndroidMediaPlayer.CurrentPlayer;

        ConcatenatingMediaSource mediaSource;
        private IPlaybackService playbackService;
        public MediaSessionConnectorPlaybackPreparer(ConcatenatingMediaSource mediaSource)
        {
            LogSetup.Initialize(VersionFinder.Default,
             new string[] { $"AndroidSdk {Build.VERSION.SdkInt}" }, Xamarin.Forms.Device.Android);

            this.mediaSource = mediaSource;
            container = BootstrapHelper.GetInitializedContainer();
            playbackService = container.Resolve<IPlaybackService>();
        }

        protected MediaSessionConnectorPlaybackPreparer(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {
        }

        public long SupportedPrepareActions =>
            PlaybackStateCompat.ActionPrepare |
            PlaybackStateCompat.ActionPrepareFromMediaId |
            PlaybackStateCompat.ActionPrepareFromSearch;

        public bool OnCommand(IPlayer player, IControlDispatcher controlDispatcher, string command, Bundle extras, ResultReceiver cb)
        {
            logger.Info($"On command called.  Queue Count: #{MediaManager.Queue.Count}. PlaybackState: #{MediaManager.State}");
            return false;
        }

        public async void OnPrepare(bool playWhenReady)
        {
            logger.Info($"On prepare called. AutoPlay: {MediaManager.AutoPlay}, Queue Count: #{MediaManager.Queue.Count}. PlaybackState: #{MediaManager.State}");

            if (mediaSource.Size > 0)
            {
                prepare(playWhenReady);
                return;
            }

            await playbackService.PrepareRelavantPlaylist();
            prepare(playWhenReady);

            logger.Info($"On prepare finished with relavant playlist. mediaSource.Size: {mediaSource.Size}");
        }

        public async void OnPrepareFromMediaId(string mediaId, bool playWhenReady, Bundle extras)
        {
            logger.Info($"On prepare  from median Id called. AutoPlay: {MediaManager.AutoPlay}, Queue Count: #{MediaManager.Queue.Count}. PlaybackState: #{MediaManager.State}");
           
            await playbackService.PrepareRelavantPlaylist();
            prepare(playWhenReady);

            logger.Info($"On prepare finished with relavant playlist. mediaSource.Size: {mediaSource.Size}");
        }

        public async void OnPrepareFromSearch(string query, bool playWhenReady, Bundle extras)
        {
            logger.Info($"On prepare from search called. AutoPlay: {MediaManager.AutoPlay}, Queue Count: #{MediaManager.Queue.Count}. PlaybackState: #{MediaManager.State}");
            await playbackService.PrepareRelavantPlaylist();
            prepare(playWhenReady);
        }

        public void OnPrepareFromUri(global::Android.Net.Uri uri, bool playWhenReady, Bundle extras)
        {
            logger.Info("On prepare from Uri called. Uri host:" + uri.Host);
            return;
        }


        private void prepare(bool playWhenReady)
        {
            currentPlayer.PlayWhenReady = playWhenReady || MediaManager.AutoPlay;
            currentPlayer.Stop(true);

            var currentTrackIndex = playbackService.CurrentTrackIndex;
            var currentTrackPosition = playbackService.CurrentTrackPosition;
            var seek = currentTrackIndex >= 0 && currentTrackPosition != default;
            if (currentPlayer is SimpleExoPlayer)
            {
                (currentPlayer as SimpleExoPlayer).Prepare(mediaSource);
                if (seek)
                {
                    currentPlayer.SeekTo(currentTrackIndex, (long)currentTrackPosition.TotalMilliseconds);
                }
                return;
            }

            var castPlayer = currentPlayer as CastPlayer;
            castPlayer.LoadItems(MediaManager.Queue.Select(x => x.ToMediaQueueItem()).ToArray(), seek ? currentTrackIndex : 0,
                seek ? (long)currentTrackPosition.TotalMilliseconds : 0, IPlayer.RepeatModeOff);
        }

    }
}