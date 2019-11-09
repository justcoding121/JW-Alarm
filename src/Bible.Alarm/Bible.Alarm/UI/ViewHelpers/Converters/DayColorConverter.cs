using Bible.Alarm.Models;
using Bible.Alarm.ViewModels;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace Bible.Alarm.UI.Views.Converters
{
    public class DayColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is null)
            {
                return Color.LightGray;
            }

            if (value is DaysOfWeek)
            {
                var isEnabled = ((DaysOfWeek)value & (DaysOfWeek)parameter) == (DaysOfWeek)parameter;
                return isEnabled ? Color.SlateBlue : Color.LightGray;
            }
            else
            {
                var schedule = value as ScheduleListItem;

                var isEnabled = ((DaysOfWeek)schedule.DaysOfWeek & (DaysOfWeek)parameter) == (DaysOfWeek)parameter;

                if (schedule.IsEnabled)
                {
                    return isEnabled ? Color.SlateBlue : Color.LightGray;
                }
                else
                {
                    return isEnabled ? Color.LightGray : Color.WhiteSmoke;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
