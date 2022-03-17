using System;
using System.Globalization;
using System.Windows.Data;

namespace SteamAuthenticatorCore.Desktop.Helpers;

internal class DoubleToIntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double num = System.Convert.ToDouble(value);
        return num;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var num = System.Convert.ToInt32(value);
        return num;
    }
}