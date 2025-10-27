using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Repositório para operações relacionadas ao estoque dos produtos.
    /// </summary>
    public class StockRepository
    {
        private readonly ApplicationDbContext _context;

        public StockRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Basic CRUD

        /// <summary>
        /// Busca estoque por ID do produto.
        /// </summary>
        public async Task<Stock?> GetByProductIdAsync(int productId)
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.ProductId == productId);
        }

        /// <summary>
        /// Busca estoque por ID do produto sem tracking.
        /// </summary>
        public async Task<Stock?> GetByProductIdAsNoTrackingAsync(int productId)
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ProductId == productId);
        }

        /// <summary>
        /// Retorna todos os registros de estoque.
        /// </summary>
        public async Task<IEnumerable<Stock>> GetAllAsync()
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca estoques usando um predicado customizado.
        /// </summary>
        public async Task<IEnumerable<Stock>> FindAsync(Expression<Func<Stock, bool>> predicate)
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Adiciona um novo registro de estoque.
        /// </summary>
        public async Task<Stock> AddAsync(Stock entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.Stocks.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Atualiza um registro de estoque existente.
        /// </summary>
        public async Task<Stock> UpdateAsync(Stock entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.LastStockUpdate = DateTime.UtcNow;
            _context.Stocks.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Remove um registro de estoque do banco de dados.
        /// </summary>
        public async Task<bool> DeleteAsync(int productId)
        {
            var stock = await _context.Stocks.FindAsync(productId);
            if (stock == null)
                return false;

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Stock Operations

        /// <summary>
        /// Atualiza a quantidade de estoque de um produto.
        /// </summary>
        public async Task<bool> UpdateQuantityAsync(int productId, int quantity, int? userId = null)
        {
            var stock = await _context.Stocks.FindAsync(productId);
            if (stock == null)
                return false;

            stock.Quantity = quantity;
            stock.LastStockUpdate = DateTime.UtcNow;
            stock.LastStockUpdateByUserId = userId;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Reserva uma quantidade de estoque.
        /// </summary>
        public async Task<bool> ReserveQuantityAsync(int productId, int quantity)
        {
            var stock = await _context.Stocks.FindAsync(productId);
            if (stock == null)
                return false;

            if (stock.AvailableQuantity < quantity)
                return false;

            stock.ReservedQuantity = (stock.ReservedQuantity ?? 0) + quantity;
            stock.LastStockUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Libera uma quantidade reservada de estoque.
        /// </summary>
        public async Task<bool> ReleaseReservedQuantityAsync(int productId, int quantity)
        {
            var stock = await _context.Stocks.FindAsync(productId);
            if (stock == null)
                return false;

            var currentReserved = stock.ReservedQuantity ?? 0;
            if (currentReserved < quantity)
                return false;

            stock.ReservedQuantity = currentReserved - quantity;
            stock.LastStockUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retorna a quantidade disponível de um produto.
        /// </summary>
        public async Task<int> GetAvailableQuantityAsync(int productId)
        {
            var stock = await _context.Stocks
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ProductId == productId);

            return stock?.AvailableQuantity ?? 0;
        }

        #endregion

        #region Specific Queries

        /// <summary>
        /// Retorna produtos com estoque baixo.
        /// </summary>
        public async Task<IEnumerable<Stock>> GetLowStockAsync()
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .Where(s => s.Product.ControlsStock && 
                           s.Product.MinimumStock.HasValue && 
                           s.Quantity <= s.Product.MinimumStock.Value)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna produtos sem estoque.
        /// </summary>
        public async Task<IEnumerable<Stock>> GetOutOfStockAsync()
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .Where(s => s.Product.ControlsStock && s.Quantity <= 0)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region Validation

        /// <summary>
        /// Verifica se um estoque existe para um produto.
        /// </summary>
        public async Task<bool> ExistsAsync(int productId)
        {
            return await _context.Stocks.AnyAsync(s => s.ProductId == productId);
        }

        #endregion

        #region Pagination

        /// <summary>
        /// Retorna estoques paginados.
        /// </summary>
        public async Task<IEnumerable<Stock>> GetPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Página deve ser maior ou igual a 1.", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Tamanho da página deve ser maior que 0.", nameof(pageSize));

            return await _context.Stocks
                .Include(s => s.Product)
                .OrderBy(s => s.Product.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna o total de registros de estoque.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Stocks.CountAsync();
        }

        /// <summary>
        /// Retorna o total de estoques que atendem a um predicado.
        /// </summary>
        public async Task<int> GetCountAsync(Expression<Func<Stock, bool>> predicate)
        {
            return await _context.Stocks.CountAsync(predicate);
        }

        #endregion
    }
}
