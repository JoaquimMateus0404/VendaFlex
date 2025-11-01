using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace VendaFlex.UI.Converters
{
    // Retorna Disabled quando a altura atual é maior/igual ao limite, e Auto quando menor (telas pequenas)
    public class HeightToScrollBarVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double actualHeight)
            {
                double threshold = 700; // padrão
                if (parameter != null && double.TryParse(System.Convert.ToString(parameter, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
                {
                    threshold = parsed;
                }

                return actualHeight < threshold ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
            }
            return ScrollBarVisibility.Auto;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
