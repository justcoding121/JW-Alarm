using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace Bible.Alarm.UI.Views.Converters
{
    public class IsEnabledColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(parameter is Color)
            {
                return bool.Parse(value.ToString()) ? (Color)parameter : Color.LightGray;
            }

            return bool.Parse(value.ToString()) ? Color.Black : Color.LightGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
