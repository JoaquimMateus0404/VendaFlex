using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace VendaFlex.Infrastructure.Converters
{
    /// <summary>
    /// Converte string hexadecimal (#RRGGBB) para Color
    /// </summary>
    public class HexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hexColor && !string.IsNullOrEmpty(hexColor))
            {
                try
                {
                    // Remove # se existir
                    hexColor = hexColor.TrimStart('#');

                    // Converte para Color
                    if (hexColor.Length == 6)
                    {
                        byte r = System.Convert.ToByte(hexColor.Substring(0, 2), 16);
                        byte g = System.Convert.ToByte(hexColor.Substring(2, 2), 16);
                        byte b = System.Convert.ToByte(hexColor.Substring(4, 2), 16);
                        return Color.FromRgb(r, g, b);
                    }
                    else if (hexColor.Length == 8) // Com alpha
                    {
                        byte a = System.Convert.ToByte(hexColor.Substring(0, 2), 16);
                        byte r = System.Convert.ToByte(hexColor.Substring(2, 2), 16);
                        byte g = System.Convert.ToByte(hexColor.Substring(4, 2), 16);
                        byte b = System.Convert.ToByte(hexColor.Substring(6, 2), 16);
                        return Color.FromArgb(a, r, g, b);
                    }
                }
                catch
                {
                    // Se falhar, retorna branco
                    return Colors.White;
                }
            }

            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }

            return "#FFFFFF";
        }
    }
}
