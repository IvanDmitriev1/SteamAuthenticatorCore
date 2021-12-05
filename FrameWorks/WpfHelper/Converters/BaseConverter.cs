using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfHelper.Converters
{
    public abstract class BaseConverter : IValueConverter
    {
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Convert back not supported");
        }
    }
}
