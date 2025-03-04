﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Runtime;
using Android.Support.V4.Media.Session;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.UI;
using Java.Lang;

namespace MediaManager.Platforms.Android.Media
{
    public class MediaDescriptionAdapter : Java.Lang.Object, PlayerNotificationManager.IMediaDescriptionAdapter
    {
        protected MediaManagerImplementation MediaManager => (MediaManagerImplementation)CrossMediaManager.Current;

        private readonly MediaControllerCompat controller;
        public MediaDescriptionAdapter(MediaControllerCompat controller)
        {
            this.controller = controller;
        }

        protected MediaDescriptionAdapter(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public PendingIntent CreateCurrentContentIntent(IPlayer player)
        {
            return controller.SessionActivity;
        }
       
        public string GetCurrentContentText(IPlayer player)
        {
            return controller.Metadata.Description.Subtitle;
        }

        public string GetCurrentContentTitle(IPlayer player)
        {
            return controller.Metadata.Description.Title;
        }

        public Bitmap GetCurrentLargeIcon(IPlayer player, PlayerNotificationManager.BitmapCallback callback)
        {
            var mediaItem = MediaManager.Queue.ElementAtOrDefault(player.CurrentWindowIndex);
            if (mediaItem != null && mediaItem.DisplayImage == null && !mediaItem.IsMetadataExtracted)
            {
                Task.Run(async () =>
                {
                    var image = await MediaManager.Extractor.GetMediaImage(mediaItem).ConfigureAwait(false) as Bitmap;
                    callback.OnBitmap(image);
                }).ConfigureAwait(false);
            }
            return mediaItem?.DisplayImage as Bitmap;
        }

        public ICharSequence GetCurrentContentTextFormatted(IPlayer player)
        {
            return new Java.Lang.String(MediaManager.Queue.ElementAtOrDefault(player.CurrentWindowIndex)?.DisplayTitle);
        }

        public ICharSequence GetCurrentContentTitleFormatted(IPlayer player)
        {
            return new Java.Lang.String(MediaManager.Queue.ElementAtOrDefault(player.CurrentWindowIndex)?.DisplaySubtitle);
        }

        public ICharSequence GetCurrentSubTextFormatted(IPlayer player)
        {
            return new Java.Lang.String(MediaManager.Queue.ElementAtOrDefault(player.CurrentWindowIndex)?.DisplayDescription);
        }
    }
}
