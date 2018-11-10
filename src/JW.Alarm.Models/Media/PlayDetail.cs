using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Models
{
    public class PlayDetail : IEntity
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public DateTimeOffset NotificationTime { get; set; }


        [JsonIgnore]
        public PlayType PlayType => BookNumber > 0 ? PlayType.Bible : PlayType.Music;

        public int BookNumber { get; set; }
        public int Chapter { get; set; }

        public int TrackNumber { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
