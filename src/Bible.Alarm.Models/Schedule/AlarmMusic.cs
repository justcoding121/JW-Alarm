using System.Collections.Generic;
using System.IO;

namespace JW.Alarm.Models
{
    public class AlarmMusic : IEntity
    {
        public int Id { get; set; }

        public MusicType MusicType { get; set; }
        public string PublicationCode { get; set; }

        public string LanguageCode { get; set; }
        public int TrackNumber { get; set; }

        //Always play current track.
        public bool Fixed { get; set; }

        public int AlarmScheduleId { get; set; }
        public AlarmSchedule AlarmSchedule { get; set; }

        public AlarmMusic()
        {

        }
    }

}
