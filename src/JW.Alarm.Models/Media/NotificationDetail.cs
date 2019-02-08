using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Models
{
    public class NotificationDetail 
    {
        public long ScheduleId { get; set; }
        public DateTimeOffset NotificationTime { get; set; }

        [JsonIgnore]
        public PlayType PlayType => BookNumber > 0 ? PlayType.Bible : PlayType.Music;

        public int BookNumber { get; set; }
        public int ChapterNumber { get; set; }

        public int TrackNumber { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
