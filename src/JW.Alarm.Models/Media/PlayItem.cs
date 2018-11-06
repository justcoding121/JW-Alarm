using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Models
{
    public class PlayItem
    {
        public PlayType Type { get; set; }
        public string Url { get; set; }
        public TimeSpan Duration { get; set; }

        public PlayItem(PlayType type, TimeSpan duration, string url)
        {
            Type = type;
            Duration = duration;
            Url = url;
        }
    }
}
