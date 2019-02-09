using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Models
{
    public class SongBook : Publication
    {
        public int Id { get; set; }
        public MusicType MusicType { get; set; }
        public List<MusicTrack> Tracks { get; set; }
    }
}
