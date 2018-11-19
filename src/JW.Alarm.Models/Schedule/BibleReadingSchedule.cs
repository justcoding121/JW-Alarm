
using System;
using System.IO;

namespace JW.Alarm.Models
{

    public class BibleReadingSchedule 
    {
        public string LanguageCode { get; set; }
        public string PublicationCode { get; set; }

        public int BookNumber { get; set; }
        public int ChapterNumber { get; set; }
    }
}
