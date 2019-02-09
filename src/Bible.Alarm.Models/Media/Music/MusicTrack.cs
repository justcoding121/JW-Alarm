using System;

namespace JW.Alarm.Models
{
    public class MusicTrack : IComparable
    {
        public int Id { get; set; }

        public int Number { get; set; }
        public string Title { get; set; }
        public TimeSpan Duration { get; set; }
        public string Url { get; set; }

        public int SongBookId { get; set; }
        public SongBook SongBook { get; set; }

        public int CompareTo(object obj)
        {
            return Number.CompareTo((obj as MusicTrack).Number);
        }
    }
}
