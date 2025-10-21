using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentDto> GetByIdAsync(int id);
        Task<IEnumerable<PaymentDto>> GetAllAsync();
        Task<PaymentDto> CreateAsync(PaymentDto dto);
        Task<PaymentDto> UpdateAsync(PaymentDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<PaymentTypeDto>> GetPaymentTypesAsync();
    }
}
