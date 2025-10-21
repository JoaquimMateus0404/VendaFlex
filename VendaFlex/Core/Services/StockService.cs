using AutoMapper;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço para controle e movimentação de estoque.
    /// </summary>
    public class StockService : IStockService
    {
        private readonly IRepository<Stock> _repo;
        private readonly IRepository<StockMovement> _movements;
        private readonly IMapper _mapper;

        public StockService(IRepository<Stock> repo, IRepository<StockMovement> movements, IMapper mapper)
        {
            _repo = repo;
            _movements = movements;
            _mapper = mapper;
        }

        public async Task<StockDto> GetByProductIdAsync(int productId)
        {
            var e = await _repo.GetByIdAsync(productId);
            return _mapper.Map<StockDto>(e);
        }

        public async Task<IEnumerable<StockDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<StockDto>>(list);
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity, int userId)
        {
            var stock = await _repo.GetByIdAsync(productId);
            var previous = stock?.Quantity ?? 0;
            if (stock == null)
            {
                stock = new Stock { ProductId = productId, Quantity = quantity, LastStockUpdateByUserId = userId };
                await _repo.AddAsync(stock);
            }
            else
            {
                stock.Quantity = quantity;
                stock.LastStockUpdateByUserId = userId;
                await _repo.UpdateAsync(stock);
            }

            // registrar movimento
            var movement = new StockMovement
            {
                ProductId = productId,
                UserId = userId,
                Quantity = quantity - previous,
                PreviousQuantity = previous,
                NewQuantity = quantity,
                Type = StockMovementType.Adjustment,
                Date = DateTime.UtcNow
            };
            await _movements.AddAsync(movement);
            return true;
        }

        public async Task<IEnumerable<StockMovementDto>> GetMovementsAsync(int productId)
        {
            var list = await _movements.FindAsync(m => m.ProductId == productId);
            return _mapper.Map<IEnumerable<StockMovementDto>>(list.OrderByDescending(m => m.Date));
        }

        public async Task<bool> RegisterMovementAsync(StockMovementDto movementDto)
        {
            var movement = _mapper.Map<StockMovement>(movementDto);
            await _movements.AddAsync(movement);
            return true;
        }
    }
}
