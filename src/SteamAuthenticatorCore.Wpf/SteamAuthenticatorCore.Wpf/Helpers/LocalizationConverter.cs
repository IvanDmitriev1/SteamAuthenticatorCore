using System.Globalization;
using System.Windows.Data;

namespace SteamAuthenticatorCore.Desktop.Helpers;

public sealed class LocalizationConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not LocalizationMessages localizationMessages)
            return null;

        if (!AppSettings.Current.LocalizationProvider.CurrentLanguageDictionary.TryGetValue(localizationMessages.ToString(), out var result))
            return string.Empty;

        return result;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}