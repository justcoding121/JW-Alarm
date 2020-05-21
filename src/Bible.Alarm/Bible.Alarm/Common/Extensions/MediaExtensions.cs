using Bible.Alarm.Common.Helpers;
using Bible.Alarm.Models;
using MediaManager.Library;
using System;
using MediaManager.Media;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using NLog;

namespace Bible.Alarm.Common.Extensions
{
    public static class MediaExtensions
    {
        private static Logger logger => LogManager.GetCurrentClassLogger();

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
            return await RetryHelper.Retry(async () =>
            {
                using var cts = new CancellationTokenSource();
                try
                {
                    cts.CancelAfter(3000);
                    return await Task.Run(async () => await mediaExtractor.CreateMediaItem(url), cts.Token);
                }
                catch (OperationCanceledException e)
                {
                    logger.Info(e, $"CreateMediaItem from URL timed Out. URL: {url}");
                    return null;
                }

            }, 3);
        }

        public static async Task<IMediaItem> CreateMediaItemEx(this IMediaExtractor mediaExtractor, FileInfo fileInfo)
        {
            using var cts = new CancellationTokenSource();
            try
            {
                cts.CancelAfter(1000);
                return await Task.Run(async () => await mediaExtractor.CreateMediaItem(fileInfo), cts.Token);
            }
            catch (OperationCanceledException e)
            {
                logger.Info(e, $"CreateMediaItem from FileInfo timed Out. Path: {fileInfo.FullName}");
                return null;
            }
        }
    }
}
