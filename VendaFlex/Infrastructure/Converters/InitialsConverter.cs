using System;
using System.Globalization;
using System.Windows.Data;

namespace VendaFlex.Infrastructure.Converters
{
    /// <summary>
    /// Converts a name string to initials (e.g., "John Doe" -> "JD")
    /// </summary>
    public class InitialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return "??";

            var name = value.ToString()!.Trim();
            var parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return "??";

            if (parts.Length == 1)
            {
                // Single word: take first 2 characters
                return parts[0].Length >= 2 
                    ? parts[0].Substring(0, 2).ToUpper() 
                    : parts[0].ToUpper();
            }

            // Multiple words: take first character of first and last word
            return $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

