using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bible.Alarm.Models
{
    public class AlarmSchedule : IEntity, IComparable
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public bool IsEnabled { get; set; }

        //24 hour based
        public int Hour { get; set; }
        public int MeridienHour => Meridien == Meridien.AM ?
                                    Hour == 0 ? 12
                                    : Hour : (Hour == 12 ? 12 : Hour % 12);
        public int Minute { get; set; }
        public Meridien Meridien => Hour < 12 ? Meridien.AM : Meridien.PM;
        public int Second { get; set; }

        public DaysOfWeek DaysOfWeek { get; set; }

        public string TimeText => $"{MeridienHour.ToString("D2")}:{Minute.ToString("D2")} {Meridien}";

        public string CronExpression => getCronExpression();

        public bool MusicEnabled { get; set; }

        public virtual AlarmMusic Music { get; set; }

        public virtual BibleReadingSchedule BibleReadingSchedule { get; set; }

        public int SnoozeMinutes { get; set; } = 5;

        //state
        public PlayType CurrentPlayItem { get; set; }

        public DateTimeOffset NextFireDate()
        {
            validateTime();

            var expression = new CronExpression(CronExpression);

            validateNextFire(expression);
            return expression.GetNextValidTimeAfter(DateTimeOffset.Now).Value;
        }

        public DateTimeOffset NextFireDate(DateTimeOffset after)
        {
            validateTime();

            var expression = new CronExpression(CronExpression);

            validateNextFire(expression);
            return expression.GetNextValidTimeAfter(after).Value;
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

        public int CompareTo(object obj)
        {
            return Id.CompareTo((obj as AlarmSchedule).Id);
        }
    }

}