using FontNameResources;
using System;
using System.Globalization;
using Xamarin.Forms;

namespace JW.Alarm.UI.Views.Converters
{
    public class PlayStatusIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(bool)value)
            {
                return GlyphNames.Play;
            }

            return GlyphNames.Pause;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
