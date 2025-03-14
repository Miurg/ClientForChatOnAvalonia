using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientForChatOnAvalonia.Converters
{
    internal class BoolToVisibilityForOtherUserConverter : IValueConverter
    {
        public static readonly BoolToVisibilityForCurrentUserConverter Instance = new BoolToVisibilityForCurrentUserConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is bool boolValue && boolValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
