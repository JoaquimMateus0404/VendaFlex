using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Data.Entities;

namespace CommerceHub.Data.Repositories
{
    /// <summary>
    /// Repositório para operações relacionadas ao estoque dos produtos.
    /// </summary>
    public class StockRepository : IRepository<Stock>
    {
        private readonly ApplicationDbContext _context;

        public StockRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Stock> GetByIdAsync(int productId)
        {
            return await _context.Stocks.FindAsync(productId);
        }

        public async Task<IEnumerable<Stock>> GetAllAsync()
        {
            return await _context.Stocks.ToListAsync();
        }

        public async Task<IEnumerable<Stock>> FindAsync(System.Linq.Expressions.Expression<System.Func<Stock, bool>> predicate)
        {
            return await _context.Stocks.Where(predicate).ToListAsync();
        }

        public async Task<Stock> AddAsync(Stock entity)
        {
            _context.Stocks.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Stock> UpdateAsync(Stock entity)
        {
            _context.Stocks.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int productId)
        {
            var stock = await _context.Stocks.FindAsync(productId);
            if (stock == null) return false;
            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Stock>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.Stocks
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém movimentações de estoque de um produto.
        /// </summary>
        public async Task<IEnumerable<StockMovement>> GetMovementsAsync(int productId)
        {
            return await _context.StockMovements
                .Where(sm => sm.ProductId == productId)
                .OrderByDescending(sm => sm.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Registra uma movimentação de estoque.
        /// </summary>
        public async Task<bool> RegisterMovementAsync(StockMovement movement)
        {
            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
