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
        private readonly IRepository<Payment> _repo;
        private readonly IRepository<PaymentType> _types;
        private readonly IMapper _mapper;

        public PaymentService(IRepository<Payment> repo, IRepository<PaymentType> types, IMapper mapper)
        {
            _repo = repo;
            _types = types;
            _mapper = mapper;
        }

        public async Task<PaymentDto> GetByIdAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id);
            return _mapper.Map<PaymentDto>(e);
        }

        public async Task<IEnumerable<PaymentDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<PaymentDto>>(list);
        }

        public async Task<PaymentDto> CreateAsync(PaymentDto dto)
        {
            var e = _mapper.Map<Payment>(dto);
            var created = await _repo.AddAsync(e);
            return _mapper.Map<PaymentDto>(created);
        }

        public async Task<PaymentDto> UpdateAsync(PaymentDto dto)
        {
            var e = _mapper.Map<Payment>(dto);
            var updated = await _repo.UpdateAsync(e);
            return _mapper.Map<PaymentDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }

        public async Task<IEnumerable<PaymentTypeDto>> GetPaymentTypesAsync()
        {
            var list = await _types.GetAllAsync();
            return _mapper.Map<IEnumerable<PaymentTypeDto>>(list);
        }
    }
}
