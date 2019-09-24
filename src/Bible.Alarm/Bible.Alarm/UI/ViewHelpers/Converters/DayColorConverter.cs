using Bible.Alarm.Models;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace Bible.Alarm.UI.Views.Converters
{
    public class DayColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isEnabled = ((DaysOfWeek)value & (DaysOfWeek)parameter) == (DaysOfWeek)parameter;

            return isEnabled ? Color.Black :  Color.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
