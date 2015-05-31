using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SearchSharp.Converters
{
    class ValidityToColorConverter : IValueConverter
    {
        private SolidColorBrush _validColor = new SolidColorBrush(Colors.Black);
        private SolidColorBrush _invalidColor = new SolidColorBrush(Colors.Red);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool) value) ? _validColor : _invalidColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
