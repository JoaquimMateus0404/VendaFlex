using AutoMapper;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    public class PriceHistoryService : IPriceHistoryService
    {
        private readonly IRepository<PriceHistory> _repo;
        private readonly IMapper _mapper;
        public PriceHistoryService(IRepository<PriceHistory> repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

        public async Task<IEnumerable<PriceHistoryDto>> GetByProductAsync(int productId)
        {
            var list = await _repo.FindAsync(ph => ph.ProductId == productId);
            return _mapper.Map<IEnumerable<PriceHistoryDto>>(list.OrderByDescending(p => p.ChangeDate));
        }

        public async Task<PriceHistoryDto> AddAsync(PriceHistoryDto dto)
        {
            var added = await _repo.AddAsync(_mapper.Map<PriceHistory>(dto));
            return _mapper.Map<PriceHistoryDto>(added);
        }
    }
}
