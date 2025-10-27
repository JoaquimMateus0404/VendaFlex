using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;

namespace VendaFlex.Core.Interfaces
{
    public interface IPaymentService
    {
        // Consultas
        Task<OperationResult<PaymentDto>> GetByIdAsync(int id);
        Task<OperationResult<IEnumerable<PaymentDto>>> GetByInvoiceIdAsync(int invoiceId);
        Task<OperationResult<IEnumerable<PaymentDto>>> GetByPaymentTypeIdAsync(int paymentTypeId);
        Task<OperationResult<IEnumerable<PaymentDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Verificações
        Task<bool> ExistsAsync(int id);

        // Agregações
        Task<decimal> GetTotalAmountByInvoiceIdAsync(int invoiceId);

        // CRUD
        Task<OperationResult<PaymentDto>> AddAsync(PaymentDto payment);
        Task<OperationResult<PaymentDto>> UpdateAsync(PaymentDto payment);
        Task<OperationResult<bool>> DeleteAsync(int id);
    }
}
