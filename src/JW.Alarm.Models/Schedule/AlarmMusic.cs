using System.IO;

namespace JW.Alarm.Models
{
    public class Music
    {
        public MusicType MusicType { get; set; }
        public string PublicationCode { get; set; }

        public string LanguageCode { get; set; }
        public int TrackNumber { get; set; }

    }

    public class AlarmMusic : Music
    {
        //Always play current track.
        public bool Fixed { get; set; }

        public AlarmMusic()
        {

        }
    }

}
