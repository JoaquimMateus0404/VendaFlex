using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;

namespace VendaFlex.Core.Interfaces
{
    public interface IInvoiceProductService
    {
        // Consultas
        Task<OperationResult<InvoiceProductDto>> GetByIdAsync(int id);
        Task<OperationResult<IEnumerable<InvoiceProductDto>>> GetByInvoiceIdAsync(int invoiceId);

        // Verificações
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsProductInInvoiceAsync(int invoiceId, int productId, int? excludeId = null);

        // CRUD
        Task<OperationResult<InvoiceProductDto>> AddAsync(InvoiceProductDto item);
        Task<OperationResult<InvoiceProductDto>> UpdateAsync(InvoiceProductDto item);
        Task<OperationResult<bool>> DeleteAsync(int id);
    }
}
