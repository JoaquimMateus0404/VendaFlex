using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Entities;

namespace VendaFlex.Core.Interfaces
{
    public interface IInvoiceService
    {
        // Consultas b�sicas
        Task<OperationResult<InvoiceDto>> GetByIdAsync(int id);
        Task<OperationResult<IEnumerable<InvoiceDto>>> GetAllAsync();

        // Consultas espec�ficas
        Task<OperationResult<InvoiceDto>> GetByNumberAsync(string invoiceNumber);
        Task<OperationResult<IEnumerable<InvoiceDto>>> GetByStatusAsync(InvoiceStatus status);
        Task<OperationResult<IEnumerable<InvoiceDto>>> GetByPersonIdAsync(int personId);
        Task<OperationResult<IEnumerable<InvoiceDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Verifica��es auxiliares
        Task<bool> ExistsAsync(int id);
        Task<bool> NumberExistsAsync(string invoiceNumber, int? excludeId = null);

        // CRUD
        Task<OperationResult<InvoiceDto>> AddAsync(InvoiceDto invoice);
        Task<OperationResult<InvoiceDto>> UpdateAsync(InvoiceDto invoice);
        Task<OperationResult<bool>> DeleteAsync(int id);

        // Pagina��o e contagem
        Task<OperationResult<IEnumerable<InvoiceDto>>> GetPaginatedAsync(int pageNumber, int pageSize);
        Task<int> GetTotalCountAsync();

        // Ações de negócio
        Task<OperationResult<bool>> CancelAsync(int invoiceId, string reason);
        Task<OperationResult<bool>> ReopenAsync(int invoiceId);
        Task<OperationResult<InvoiceDto>> DuplicateAsync(int invoiceId);
    }
}
