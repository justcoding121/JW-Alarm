using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Models
{
    public class PlayItem
    {
        public string Url { get; set; }
        public int Second { get; set; }

        public PlayItem(string url, int second)
        {
            Url = url;
            Second = second;
        }
    }
}
