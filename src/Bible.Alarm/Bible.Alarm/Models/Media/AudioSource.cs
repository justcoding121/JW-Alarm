using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.Models
{
    public class AudioSource
    {
        public int Id { get; set; }

        public TimeSpan Duration { get; set; }
        public string Url { get; set; }

        public string LookUpPath { get; set; }
    }
}
