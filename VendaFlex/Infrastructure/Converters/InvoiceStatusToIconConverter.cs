using System.Globalization;
using System.Windows.Data;
using VendaFlex.Data.Entities;

namespace VendaFlex.Infrastructure.Converters;

/// <summary>
/// Converte InvoiceStatus para um ícone MaterialDesign (string do Kind).
/// </summary>
public class InvoiceStatusToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Paid => "CheckCircle",
                InvoiceStatus.Confirmed => "ClockOutline",
                InvoiceStatus.Pending => "ClockOutline",
                InvoiceStatus.Cancelled => "Cancel",
                InvoiceStatus.Draft => "FileDocumentEditOutline",
                InvoiceStatus.Refunded => "CashRefund",
                _ => "FileDocument"
            };
        }

        if (value is string statusStr)
        {
            return statusStr.ToLower() switch
            {
                "paga" or "paid" => "CheckCircle",
                "confirmado" or "confirmed" => "ClockOutline",
                "pendente" or "pending" => "ClockOutline",
                "cancelada" or "cancelled" => "Cancel",
                "rascunho" or "draft" => "FileDocumentEditOutline",
                "reembolsada" or "refunded" => "CashRefund",
                _ => "FileDocument"
            };
        }

        return "FileDocument";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}