﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MediaManager.Media;
using AVFoundation;
using Foundation;
#if __IOS__ || __TVOS__
using UIKit;
#endif

namespace MediaManager.Platforms.Apple
{
    public class AppleMediaExtractor : IMediaExtractor
    {
        protected Dictionary<string, string> RequestHeaders => CrossMediaManager.Current.RequestHeaders;

        public AppleMediaExtractor()
        {
        }

        public virtual async Task<IMediaItem> CreateMediaItem(string url)
        {
            IMediaItem mediaItem = new MediaItem(url);
            return await ExtractMetadata(mediaItem);
        }

        public virtual async Task<IMediaItem> CreateMediaItem(FileInfo file)
        {
            IMediaItem mediaItem = new MediaItem(file.FullName);
            return await ExtractMetadata(mediaItem);
        }

        public virtual async Task<IMediaItem> CreateMediaItem(IMediaItem mediaItem)
        {
            return await ExtractMetadata(mediaItem);
        }

        public async Task<IMediaItem> ExtractMetadata(IMediaItem mediaItem)
        {
            var assetsToLoad = new List<string>
            {
                AVMetadata.CommonKeyArtist,
                AVMetadata.CommonKeyTitle,
                AVMetadata.CommonKeyArtwork
            };

            var url = GetUrlFor(mediaItem);

            // Default title to filename
            mediaItem.Title = url.LastPathComponent;

            var asset = AVAsset.FromUrl(url);
            await asset.LoadValuesTaskAsync(assetsToLoad.ToArray());

            foreach (var avMetadataItem in asset.CommonMetadata)
            {
                if (avMetadataItem.CommonKey == AVMetadata.CommonKeyArtist)
                {
                    mediaItem.Artist = ((NSString)avMetadataItem.Value).ToString();
                }
                else if (avMetadataItem.CommonKey == AVMetadata.CommonKeyTitle)
                {
                    mediaItem.Title = ((NSString)avMetadataItem.Value).ToString();
                }
                else if (avMetadataItem.CommonKey == AVMetadata.CommonKeyArtwork)
                {
#if __IOS__ || __TVOS__
                    var image = UIImage.LoadFromData(avMetadataItem.DataValue);
                    mediaItem.AlbumArt = image;
#endif
                }
            }

            return mediaItem;
        }

        public static NSUrl GetUrlFor(IMediaItem mediaItem)
        {
            var isLocallyAvailable = mediaItem.MediaLocation == MediaLocation.FileSystem;

            var url = isLocallyAvailable ? new NSUrl(mediaItem.MediaUri, false) : new NSUrl(mediaItem.MediaUri);

            return url;
        }
    }
}
