using AutoMapper;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço para pagamentos e tipos de pagamento.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IMapper _mapper;

        public Task<PaymentDto> CreateAsync(PaymentDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PaymentDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PaymentDto> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PaymentTypeDto>> GetPaymentTypesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PaymentDto> UpdateAsync(PaymentDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
