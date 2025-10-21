using VendaFlex.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VendaFlex.Core.Interfaces
{
    public interface IReceiptPrintService
    {
        Task PrintAsync(CompanyConfigDto cfg, InvoiceDto invoice, IEnumerable<InvoiceProductDto> items, string format);
    }
}
