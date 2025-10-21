using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    public interface IPriceHistoryService
    {
        Task<IEnumerable<PriceHistoryDto>> GetByProductAsync(int productId);
        Task<PriceHistoryDto> AddAsync(PriceHistoryDto dto);
    }
}
