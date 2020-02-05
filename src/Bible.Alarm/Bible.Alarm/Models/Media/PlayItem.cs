using System;

namespace Bible.Alarm.Models
{
    public class PlayItem
    {
        public NotificationDetail PlayDetail { get; set; }

        public string Url { get; set; }
        public TimeSpan Duration { get; set; }

        public PlayItem(NotificationDetail detail, TimeSpan duration, string url)
        {
            PlayDetail = detail;
            Duration = duration;
            Url = url;
        }

        public override string ToString()
        {
            return PlayDetail.LanguageCode + " " + PlayDetail.PublicationCode + " "
                + (PlayDetail.IsAlarmMusic ? PlayDetail.TrackNumber.ToString()
                : PlayDetail.BookNumber.ToString() + " " + PlayDetail.ChapterNumber.ToString());
        }
    }
}
