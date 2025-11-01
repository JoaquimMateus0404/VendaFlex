using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VendaFlex.Infrastructure.Converters
{
    public class NullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            var invert = parameter?.ToString() == "Invert";
            var isNullOrEmpty = string.IsNullOrWhiteSpace(str);
            if (!invert)
                return isNullOrEmpty ? Visibility.Collapsed : Visibility.Visible;
            return isNullOrEmpty ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
