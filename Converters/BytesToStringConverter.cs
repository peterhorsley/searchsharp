using System;
using System.Globalization;
using System.Windows.Data;

namespace SearchSharp.Converters
{
    class BytesToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((long)value).ToString("N0");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
