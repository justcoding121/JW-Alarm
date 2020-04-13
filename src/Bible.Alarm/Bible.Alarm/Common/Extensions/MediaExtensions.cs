using Bible.Alarm.Common.Helpers;
using Bible.Alarm.Models;
using MediaManager.Library;

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

        }
    }
}
