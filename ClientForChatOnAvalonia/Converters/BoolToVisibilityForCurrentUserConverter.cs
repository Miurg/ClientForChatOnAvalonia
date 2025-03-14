using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ClientForChatOnAvalonia.Converters
{
    public class BoolToVisibilityForCurrentUserConverter : IValueConverter
    {
        public static readonly BoolToVisibilityForCurrentUserConverter Instance = new BoolToVisibilityForCurrentUserConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue && boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
