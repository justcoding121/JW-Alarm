using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Models
{
    public class PlayItem
    {
        public PlayType Type { get; set; }
        public string Url { get; set; }

        public PlayItem(PlayType type, string url)
        {
            Type = type;
            Url = url;
        }
    }
}
