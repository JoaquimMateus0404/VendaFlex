using AutoMapper;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço para gestão de faturas e operações relacionadas.
    /// </summary>
    public class InvoiceService : IInvoiceService
    {

        private readonly IMapper _mapper;

        public Task<bool> AddProductsAsync(int invoiceId, IEnumerable<InvoiceProductDto> products)
        {
            throw new NotImplementedException();
        }

        public Task<InvoiceDto> CreateAsync(InvoiceDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InvoiceDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InvoiceDto>> GetByCustomerAsync(int personId)
        {
            throw new NotImplementedException();
        }

        public Task<InvoiceDto> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InvoiceProductDto>> GetProductsAsync(int invoiceId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RegisterPaymentAsync(PaymentDto paymentDto)
        {
            throw new NotImplementedException();
        }

        public Task<InvoiceDto> UpdateAsync(InvoiceDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
