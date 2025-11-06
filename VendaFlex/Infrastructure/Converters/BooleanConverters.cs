using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace VendaFlex.Infrastructure.Converters
{
    /// <summary>
    /// Converte Boolean para Visibility (True = Visible, False = Collapsed)
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    /// <summary>
    /// Converte Boolean para Visibility (True = Collapsed, False = Visible)
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return true;
        }
    }


    /// <summary>
    /// Converte Boolean para String baseado em parâmetro
    /// Parameter format: "TrueValue|FalseValue"
    /// </summary>
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string paramString)
            {
                var parts = paramString.Split('|');
                if (parts.Length == 2)
                {
                    return boolValue ? parts[0] : parts[1];
                }
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte Boolean para Color (Brush) baseado em parâmetro
    /// Parameter format: "TrueColor|FalseColor" (hex colors)
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string paramString)
            {
                var parts = paramString.Split('|');
                if (parts.Length == 2)
                {
                    var colorString = boolValue ? parts[0] : parts[1];
                    try
                    {
                        var converter = new System.Windows.Media.BrushConverter();
                        return converter.ConvertFrom(colorString) ?? System.Windows.Media.Brushes.Transparent;
                    }
                    catch
                    {
                        return System.Windows.Media.Brushes.Transparent;
                    }
                }
            }
            return System.Windows.Media.Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte Boolean para PackIcon Kind baseado em parâmetro
    /// Parameter format: "TrueIcon|FalseIcon"
    /// </summary>
    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string paramString)
            {
                var parts = paramString.Split('|');
                if (parts.Length == 2)
                {
                    var iconName = boolValue ? parts[0] : parts[1];
                    if (Enum.TryParse(typeof(MaterialDesignThemes.Wpf.PackIconKind), iconName, out var iconKind))
                    {
                        return iconKind;
                    }
                }
            }
            return MaterialDesignThemes.Wpf.PackIconKind.Help;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte Enum para String
    /// </summary>
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            return value.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                try
                {
                    return Enum.Parse(targetType, stringValue);
                }
                catch
                {
                    return Binding.DoNothing;
                }
            }
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Converte valor maior que zero para Visibility
    /// </summary>
    public class GreaterThanZeroToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            if (value is double doubleValue)
            {
                return doubleValue > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            if (value is decimal decimalValue)
            {
                return decimalValue > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte ProductStatus para cor
    /// </summary>
    public class ProductStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VendaFlex.Data.Entities.ProductStatus status)
            {
                return status switch
                {
                    VendaFlex.Data.Entities.ProductStatus.Active => "#4CAF50",      // Verde
                    VendaFlex.Data.Entities.ProductStatus.Inactive => "#9E9E9E",    // Cinza
                    VendaFlex.Data.Entities.ProductStatus.Discontinued => "#F44336", // Vermelho
                    VendaFlex.Data.Entities.ProductStatus.OutOfStock => "#FF9800",  // Laranja
                    _ => "#9E9E9E"
                };
            }
            return "#9E9E9E";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converte ProductStatus para texto traduzido
    /// </summary>
    public class ProductStatusToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is VendaFlex.Data.Entities.ProductStatus status)
            {
                return status switch
                {
                    VendaFlex.Data.Entities.ProductStatus.Active => "Ativo",
                    VendaFlex.Data.Entities.ProductStatus.Inactive => "Inativo",
                    VendaFlex.Data.Entities.ProductStatus.Discontinued => "Descontinuado",
                    VendaFlex.Data.Entities.ProductStatus.OutOfStock => "Sem Estoque",
                    _ => status.ToString()
                };
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
