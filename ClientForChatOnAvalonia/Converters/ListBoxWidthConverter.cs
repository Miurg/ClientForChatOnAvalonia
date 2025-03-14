using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ClientForChatOnAvalonia.Converters
{
    public class ListBoxWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width && width > 0)
            {
                return width - 29;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
