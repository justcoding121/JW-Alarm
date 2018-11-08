using System.IO;

namespace JW.Alarm.Models
{
    public class AlarmMusic
    {
        public MusicType MusicType { get; set; }
        public string PublicationCode { get; set; }

        public string LanguageCode { get; set; }
        public int TrackNumber { get; set; }

        //Always play current track.
        public bool Fixed { get; set; }

        public AlarmMusic()
        {

        }
    }

}
