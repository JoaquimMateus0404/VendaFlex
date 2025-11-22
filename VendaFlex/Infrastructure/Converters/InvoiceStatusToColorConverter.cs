using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VendaFlex.Data.Entities;

namespace VendaFlex.Infrastructure.Converters;

 /// <summary>
    /// Converte InvoiceStatus para uma cor SolidColorBrush.
    /// </summary>
    public class InvoiceStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InvoiceStatus status)
            {
                return status switch
                {
                    InvoiceStatus.Paid => new SolidColorBrush(Color.FromRgb(76, 175, 80)),      // Green
                    InvoiceStatus.Confirmed => new SolidColorBrush(Color.FromRgb(255, 152, 0)),   // Orange
                    InvoiceStatus.Cancelled => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Red
                    InvoiceStatus.Draft => new SolidColorBrush(Color.FromRgb(158, 158, 158)),   // Gray
                    InvoiceStatus.Refunded => new SolidColorBrush(Color.FromRgb(156, 39, 176)), // Purple
                    _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))                       // Default Gray
                };
            }

            // Se for string (para compatibilidade)
            if (value is string statusStr)
            {
                return statusStr.ToLower() switch
                {
                    "paga" or "paid" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                    "confirmado" or "confirmed" => new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                    "pendente" or "pending" => new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                    "cancelada" or "cancelled" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                    "rascunho" or "draft" => new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                    "reembolsada" or "refunded" => new SolidColorBrush(Color.FromRgb(156, 39, 176)),
                    _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))
                };
            }

            return new SolidColorBrush(Color.FromRgb(158, 158, 158));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }