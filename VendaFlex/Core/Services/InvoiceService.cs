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
        private readonly IRepository<Invoice> _repo;
        private readonly IRepository<InvoiceProduct> _invoiceProducts;
        private readonly IRepository<Payment> _payments;
        private readonly IMapper _mapper;

        public InvoiceService(IRepository<Invoice> repo,
                              IRepository<InvoiceProduct> invoiceProducts,
                              IRepository<Payment> payments,
                              IMapper mapper)
        {
            _repo = repo;
            _invoiceProducts = invoiceProducts;
            _payments = payments;
            _mapper = mapper;
        }

        public async Task<InvoiceDto> GetByIdAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id);
            return _mapper.Map<InvoiceDto>(e);
        }

        public async Task<IEnumerable<InvoiceDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<InvoiceDto>>(list);
        }

        public async Task<InvoiceDto> CreateAsync(InvoiceDto dto)
        {
            var e = _mapper.Map<Invoice>(dto);
            var created = await _repo.AddAsync(e);
            return _mapper.Map<InvoiceDto>(created);
        }

        public async Task<InvoiceDto> UpdateAsync(InvoiceDto dto)
        {
            var e = _mapper.Map<Invoice>(dto);
            var updated = await _repo.UpdateAsync(e);
            return _mapper.Map<InvoiceDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }

        public async Task<IEnumerable<InvoiceDto>> GetByCustomerAsync(int personId)
        {
            var list = await _repo.FindAsync(i => i.PersonId == personId);
            return _mapper.Map<IEnumerable<InvoiceDto>>(list);
        }

        public async Task<bool> RegisterPaymentAsync(PaymentDto paymentDto)
        {
            var payment = _mapper.Map<Payment>(paymentDto);
            await _payments.AddAsync(payment);
            return true;
        }

        public async Task<IEnumerable<InvoiceProductDto>> GetProductsAsync(int invoiceId)
        {
            var list = await _invoiceProducts.FindAsync(ip => ip.InvoiceId == invoiceId);
            return _mapper.Map<IEnumerable<InvoiceProductDto>>(list);
        }

        public async Task<bool> AddProductsAsync(int invoiceId, IEnumerable<InvoiceProductDto> products)
        {
            foreach (var p in products)
            {
                var entity = _mapper.Map<InvoiceProduct>(p);
                entity.InvoiceId = invoiceId;
                await _invoiceProducts.AddAsync(entity);
            }
            return true;
        }
    }
}
