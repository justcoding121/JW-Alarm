﻿using System;
using System.Globalization;
using Xamarin.Forms;

namespace JW.Alarm.UI.Views.Converters
{
    public class IsEnabledColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return bool.Parse(value.ToString()) ? Color.Black : Color.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
