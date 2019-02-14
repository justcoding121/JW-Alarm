using System;
using System.Globalization;
using Xamarin.Forms;

namespace JW.Alarm.UI.Views.Converters
{
    public class BoolVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Boolean && (bool)value)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return true;
            }
            return false;
        }
    }
}
