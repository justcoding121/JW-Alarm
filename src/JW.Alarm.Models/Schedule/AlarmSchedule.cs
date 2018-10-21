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
        
        public int Hour { get; set; } //24 hour based
        public int MeridienHour => Meridien == Meridien.AM ? Hour : Hour % 12;
        public int Minute { get; set; }
        public Meridien Meridien => Hour < 12 ? Meridien.AM : Meridien.PM;
        public HashSet<DayOfWeek> DaysOfWeek { get; set; } = new HashSet<DayOfWeek>(new DayOfWeek[] {
            DayOfWeek.Sunday,
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday });
        [JsonIgnore]
        public string TimeText => $"{MeridienHour.ToString("D2")}:{Minute.ToString("D2")} {Meridien}";
        [JsonIgnore]
        public string CronExpression => getCronExpression();

        public bool MusicEnabled { get; set; } = true;
        public AlarmMusic Music { get; set; }

        public bool BibleReadingEnabled { get; set; } = true;
        public int BibleReadingScheduleId { get; set; }

        public PlayType CurrentPlayItem { get; set; }

        public AlarmSchedule()
        {
           
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
            string days = string.Join(",", DaysOfWeek.Select(x => (int)x + 1).OrderBy(x => x));
            var expression = new CronExpression($"0 {Minute} {Hour} ? * {days}");
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

            if (DaysOfWeek?.Count == 0)
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