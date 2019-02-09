using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JW.Alarm.Models
{
    public class AlarmSchedule : IEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public bool IsEnabled { get; set; } = true;

        //24 hour based
        public int Hour { get; set; }
        public int MeridienHour => Meridien == Meridien.AM ? Hour : Hour % 12;
        public int Minute { get; set; }
        public Meridien Meridien => Hour < 12 ? Meridien.AM : Meridien.PM;
        public int Second { get; set; }

        public DaysOfWeek DaysOfWeek { get; set; } 

        [JsonIgnore]
        public string TimeText => $"{MeridienHour.ToString("D2")}:{Minute.ToString("D2")} {Meridien}";
        [JsonIgnore]
        public string CronExpression => getCronExpression();

        public bool MusicEnabled { get; set; }
        public AlarmMusic Music { get; set; }

        public int BibleReadingScheduleId { get; set; }
        public BibleReadingSchedule BibleReadingSchedule { get; set; }

        public int SnoozeMinutes { get; set; } = 5;

        //state
        public PlayType CurrentPlayItem { get; set; }

        public AlarmSchedule()
        {
            Music = new AlarmMusic()
            {
                MusicType = MusicType.Melodies,
                PublicationCode = "iam",
                LanguageCode = "E",
                TrackNumber = 89
            };

            BibleReadingSchedule = new BibleReadingSchedule()
            {
                BookNumber = 23,
                ChapterNumber = 1,
                LanguageCode = "E",
                PublicationCode = "nwt"
            };

        }
        public DateTimeOffset NextFireDate()
        {
            validateTime();

            var expression = new CronExpression(CronExpression);

            validateNextFire(expression);

            return expression.GetNextValidTimeAfter(DateTimeOffset.Now).Value;
        }

        private string getCronExpression()
        {
            string days = string.Join(",", DaysOfWeek.ToList().OrderBy(x => x));
            var expression = new CronExpression($"{Second} {Minute} {Hour} ? * {days}");
            return expression.CronExpressionString;
        }

        private void validateTime()
        {

            if (Minute < 0 || Minute >= 60)
            {
                throw new Exception("Invalid minute.");
            }

            if (Hour < 0 || Hour >= 24)
            {
                throw new Exception("Invalid hour.");
            }

            if (DaysOfWeek == 0)
            {
                throw new Exception("DaysOfWeek is empty.");
            }
        }

        private void validateNextFire(CronExpression expression)
        {
            var nextFire = expression.GetNextValidTimeAfter(DateTimeOffset.Now);

            if (nextFire == null)
            {
                throw new Exception("Invalid alarm time.");
            }
        }
    }

}