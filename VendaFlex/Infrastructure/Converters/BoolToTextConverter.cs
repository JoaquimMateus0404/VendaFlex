using System;
using System.Globalization;
using System.Windows.Data;

namespace VendaFlex.Infrastructure.Converters
{
    /// <summary>
    /// Converts a boolean to text based on parameter.
    /// Parameter format: "TrueText|FalseText"
    /// Example: "Ocultar|Mostrar"
    /// </summary>
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || string.IsNullOrWhiteSpace(parameter.ToString()))
                return value?.ToString() ?? string.Empty;

            var texts = parameter.ToString()!.Split('|');
            if (texts.Length != 2)
                return value?.ToString() ?? string.Empty;

            var boolValue = false;
            if (value is bool b)
                boolValue = b;
            else if (value != null)
                bool.TryParse(value.ToString(), out boolValue);

            return boolValue ? texts[0] : texts[1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

