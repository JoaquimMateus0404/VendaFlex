using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceDto> GetByIdAsync(int id);
        Task<IEnumerable<InvoiceDto>> GetAllAsync();
        Task<InvoiceDto> CreateAsync(InvoiceDto dto);
        Task<InvoiceDto> UpdateAsync(InvoiceDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<InvoiceDto>> GetByCustomerAsync(int personId);
        Task<bool> RegisterPaymentAsync(PaymentDto paymentDto);
        Task<IEnumerable<InvoiceProductDto>> GetProductsAsync(int invoiceId);
        // Added: add products to an invoice
        Task<bool> AddProductsAsync(int invoiceId, IEnumerable<InvoiceProductDto> products);
    }
}
