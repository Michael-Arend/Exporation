﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PokerFrontend.Views.Converter;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = (bool)value;
        boolValue = parameter != null ? !boolValue : boolValue;
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}