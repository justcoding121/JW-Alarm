using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.Models
{
    public enum DaysOfWeek
    {
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64
    }

    public static class DaysOfWeekExtensions
    {
        public static List<int> ToList(this DaysOfWeek daysOfWeek)
        {
            var result = new List<int>();

            int day = 1;
            foreach(var item in Enum.GetValues(typeof(DaysOfWeek)))
            {
                if((daysOfWeek & (DaysOfWeek)item) == (DaysOfWeek)item)
                {
                    result.Add(day);
                }
                day++;
            }

            return result;
        }
    }
}
