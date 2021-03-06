﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace SearchSharp.Converters
{
    class SearchStateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? "Stop" : "Search";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
