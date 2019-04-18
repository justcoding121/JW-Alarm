﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AVFoundation;
using Foundation;
using MediaManager.Media;

namespace MediaManager.Platforms.Apple.Media
{
    public static class MediaItemExtensions
    {
        public static Dictionary<string, string> RequestHeaders => CrossMediaManager.Current.RequestHeaders;

        public static AVPlayerItem GetPlayerItem(this IMediaItem mediaItem)
        {
            AVAsset asset;

            if(mediaItem.MediaLocation == MediaLocation.Embedded)
            {
                string directory = Path.GetDirectoryName(mediaItem.MediaUri);
                string filename = Path.GetFileNameWithoutExtension(mediaItem.MediaUri);
                string extension = Path.GetExtension(mediaItem.MediaUri).Substring(1);
                NSUrl url = NSBundle.MainBundle.GetUrlForResource(filename, extension, directory);
                asset = AVAsset.FromUrl(url);
            }
            else if (RequestHeaders != null && RequestHeaders.Any())
            {
                asset = AVUrlAsset.Create(NSUrl.FromString(mediaItem.MediaUri), GetOptionsWithHeaders(RequestHeaders));
            }
            else
            {
                asset = AVAsset.FromUrl(NSUrl.FromString(mediaItem.MediaUri));
            }

            var playerItem = AVPlayerItem.FromAsset(asset);

            return playerItem;
        }

        private static AVUrlAssetOptions GetOptionsWithHeaders(IDictionary<string, string> headers)
        {
            var nativeHeaders = new NSMutableDictionary();

            foreach (var header in headers)
            {
                nativeHeaders.Add((NSString)header.Key, (NSString)header.Value);
            }

            var nativeHeadersKey = (NSString)"AVURLAssetHTTPHeaderFieldsKey";

            var options = new AVUrlAssetOptions(NSDictionary.FromObjectAndKey(
                nativeHeaders,
                nativeHeadersKey
            ));

            return options;
        }
    }
}
