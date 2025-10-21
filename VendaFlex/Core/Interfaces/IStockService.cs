using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    public interface IStockService
    {
        Task<StockDto> GetByProductIdAsync(int productId);
        Task<IEnumerable<StockDto>> GetAllAsync();
        Task<bool> UpdateStockAsync(int productId, int quantity, int userId);
        Task<IEnumerable<StockMovementDto>> GetMovementsAsync(int productId);
        Task<bool> RegisterMovementAsync(StockMovementDto movementDto);
    }
}
