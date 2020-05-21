using Bible.Alarm.Common.Helpers;
using Bible.Alarm.Models;
using MediaManager;
using MediaManager.Library;
using MediaManager.Media;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bible.Alarm.Common.Extensions
{
    public static class MediaManagerExtensions
    {
        private static readonly int retryAttempts = 3;
        private static Logger logger => LogManager.GetCurrentClassLogger();

        public static bool IsPreparedEx(this IMediaManager mediaManager)
        {
            if (CurrentDevice.RuntimePlatform == Device.Android)
            {
                return mediaManager.IsPrepared();
            }

            return mediaManager.IsPrepared()
                && mediaManager.Position.TotalMilliseconds > 0;
        }

        public static async Task StopEx(this IMediaManager mediaManager)
        {
            await mediaManager.Stop();
        }

        public static void SetDisplay(this IMediaItem item, NotificationDetail detail)
        {
            if (detail.IsBibleReading)
            {
                //Publication Name
                item.DisplayTitle = JwSourceHelper.PublicationCodeToNameMappings[detail.PublicationCode];

                //Book & Chapter
                item.DisplaySubtitle = $"{BgSourceHelper.BookNumberToNamesMap[detail.BookNumber]} {detail.ChapterNumber}";
                item.DisplayDescription = "jw.org";

            }
            else
            {
                if (string.IsNullOrEmpty(item.Title)
                    || CurrentDevice.RuntimePlatform == Device.UWP)
                {
                    item.Title = "Orchestral Melodies";
                }
                else
                {
                    if (item.DisplayDescription.Contains("Kingdom Melodies"))
                    {
                        item.Title = $"Melody Number(s) {item.Title}";
                    }
                }

                if (string.IsNullOrEmpty(item.DisplaySubtitle)
                    || CurrentDevice.RuntimePlatform == Device.UWP)
                {
                    item.DisplaySubtitle = "Watch Tower Bible and Tract Society of Pennsylvania";
                }

                if (string.IsNullOrEmpty(item.DisplayDescription)
                    || CurrentDevice.RuntimePlatform == Device.UWP)
                {
                    item.DisplayDescription = "jw.org";
                }
            }
        }

        public static async Task<IMediaItem> CreateMediaItemEx(this IMediaExtractor mediaExtractor, string url)
        {
            try
            {
                return await RetryHelper.Retry(async () =>
                {
                    using var cts = new CancellationTokenSource();
                    cts.CancelAfter(3000);
                    return await Task.Run(async () => await mediaExtractor.CreateMediaItem(url), cts.Token);

                }, retryAttempts, true);
            }
            catch (OperationCanceledException e)
            {
                logger.Info(e, $"CreateMediaItem from URL timed Out. URL: {url}");
                return null;
            }
        }

        public static async Task<IMediaItem> CreateMediaItemEx(this IMediaExtractor mediaExtractor, FileInfo fileInfo)
        {
            try
            {
                return await RetryHelper.Retry(async () =>
                {
                    using var cts = new CancellationTokenSource();
                    cts.CancelAfter(1000);
                    return await Task.Run(async () => await mediaExtractor.CreateMediaItem(fileInfo), cts.Token);

                }, retryAttempts, true);
            }
            catch (OperationCanceledException e)
            {
                logger.Info(e, $"CreateMediaItem from FileInfo timed Out. Path: {fileInfo.FullName}");
                return null;
            }
        }
    }
}
