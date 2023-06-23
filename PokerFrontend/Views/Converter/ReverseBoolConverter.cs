using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PokerFrontend.Views.Converter;

public class ReverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    { return!(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !(bool)value;
    }
}