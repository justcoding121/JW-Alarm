
using System.IO;

namespace JW.Alarm.Models
{
    public class AlarmMusic
    {
        public MusicType MusicType { get; set; }
        public string PublicationCode { get; set; }

        public string LanguageCode { get; set; }
        public int TrackNumber { get; set; }

        public AlarmMusic()
        {
        }

        //Always play current track or let the alarm move to next track when alarm is fired next time.
        public bool IsFixed { get; set; }
    }



}
