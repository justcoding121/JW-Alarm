using Bible.Alarm.Common.Helpers;
using Bible.Alarm.Models;
using MediaManager.Library;
using Xamarin.Forms;

namespace Bible.Alarm.Common.Extensions
{
    public static class MediaExtensions
    {
        public static void SetDisplay(this IMediaItem item, NotificationDetail detail)
        {
            if (detail.IsBibleReading)
            {
                //Publication Name
                if (BgSourceHelper.PublisherCodeToAuthorsCodeMap.ContainsKey(detail.PublicationCode))
                {
                    item.DisplayTitle = BgSourceHelper.PublicationCodeToNameMappings[detail.PublicationCode];
                }
                else
                {
                    item.DisplayTitle = JwSourceHelper.PublicationCodeToNameMappings[detail.PublicationCode];
                }

                //Book & Chapter
                item.DisplaySubtitle = $"{BgSourceHelper.BookNumberToNamesMap[detail.BookNumber]} {detail.ChapterNumber}";

                //Source Name
                if (BgSourceHelper.PublisherCodeToAuthorsCodeMap.ContainsKey(detail.PublicationCode))
                {
                    var authorCode = BgSourceHelper.PublisherCodeToAuthorsCodeMap[detail.PublicationCode];
                    var authorName = BgSourceHelper.AuthorCodeToAuthorNameMap[authorCode];
                    item.DisplayDescription = $"biblegateway.com ({authorName})";
                }
                else
                {
                    item.DisplayDescription = "jw.org";
                }
            }
            else
            {
                if (string.IsNullOrEmpty(item.Title)
                    || Device.RuntimePlatform == Device.UWP)
                {
                    item.Title = "Melodies";
                }
                else
                {
                    if (item.DisplayDescription.Contains("Kingdom Melodies"))
                    {
                        item.Title = $"Melody Number(s) {item.Title}";
                    }
                }


                if (string.IsNullOrEmpty(item.DisplaySubtitle)
                    || Device.RuntimePlatform == Device.UWP)
                {
                    item.DisplaySubtitle = "Watch Tower Bible and Tract Society of Pennsylvania";
                }

                if (string.IsNullOrEmpty(item.DisplayDescription)
                    || Device.RuntimePlatform == Device.UWP)
                {
                    item.DisplayDescription = "jw.org";
                }
            }
        }
    }
}
