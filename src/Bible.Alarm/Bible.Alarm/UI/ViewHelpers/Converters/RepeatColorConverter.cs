using System;
using System.Globalization;
using Xamarin.Forms;

namespace Bible.Alarm.UI.Views.Converters
{
    public class RepeatColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return bool.Parse(value.ToString()) ? Color.SlateBlue : Color.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
