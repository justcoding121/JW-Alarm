using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Models
{
    public class PlayItem
    {
        public string Url { get; set; }

        public PlayItem(string url)
        {
            Url = url;
        }
    }
}
