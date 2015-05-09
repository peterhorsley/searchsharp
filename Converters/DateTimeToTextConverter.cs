using System;
using System.Globalization;
using System.Windows.Data;

namespace SearchSharp.Converters
{
    class DateTimeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dt = (DateTime)value;
            return dt.ToString("yyy-MM-dd HH:mm:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
