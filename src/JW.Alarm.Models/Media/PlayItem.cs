using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Models
{
    public class PlayItem
    {
        public PlayDetail PlayDetail { get; set; }

        public string Url { get; set; }
        public TimeSpan Duration { get; set; }

        public PlayItem(PlayDetail detail, TimeSpan duration, string url)
        {
            PlayDetail = detail;
            Duration = duration;
            Url = url;
        }
    }
}
