using System;
using System.Globalization;
using System.Windows.Data;

namespace VendaFlex.Infrastructure.Converters
{
    /// <summary>
    /// Converte decimal <-> string tratando string vazia/null como 0.
    /// Respeita a cultura de binding para parsing/formatação.
    /// </summary>
    public class DecimalZeroWhenEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "0";

            if (value is decimal d)
                return d.ToString(culture);

            try
            {
                var s = System.Convert.ToString(value, culture);
                return string.IsNullOrWhiteSpace(s) ? "0" : s;
            }
            catch
            {
                return "0";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = System.Convert.ToString(value, culture);
            if (string.IsNullOrWhiteSpace(s))
                return 0m;

            if (decimal.TryParse(s, NumberStyles.Any, culture, out var result))
                return result;

            // Tenta com InvariantCulture como fallback
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;

            return 0m;
        }
    }
}
