using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    public class PriceHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public PriceHistoryRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Basic CRUD

        /// <summary>
        /// Busca um histórico de preço por ID.
        /// </summary>
        public async Task<PriceHistory?> GetByIdAsync(int id)
        {
            return await _context.PriceHistories
                .Include(ph => ph.Product)
                .FirstOrDefaultAsync(ph => ph.PriceHistoryId == id);
        }

        /// <summary>
        /// Busca um histórico de preço por ID sem tracking.
        /// </summary>
        public async Task<PriceHistory?> GetByIdAsNoTrackingAsync(int id)
        {
            return await _context.PriceHistories
                .Include(ph => ph.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(ph => ph.PriceHistoryId == id);
        }

        /// <summary>
        /// Retorna todos os históricos de preços.
        /// </summary>
        public async Task<IEnumerable<PriceHistory>> GetAllAsync()
        {
            return await _context.PriceHistories
                .Include(ph => ph.Product)
                .OrderByDescending(ph => ph.ChangeDate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca históricos usando um predicado customizado.
        /// </summary>
        public async Task<IEnumerable<PriceHistory>> FindAsync(Expression<Func<PriceHistory, bool>> predicate)
        {
            return await _context.PriceHistories
                .Include(ph => ph.Product)
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Adiciona um novo histórico de preço.
        /// </summary>
        public async Task<PriceHistory> AddAsync(PriceHistory entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.PriceHistories.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Atualiza um histórico de preço existente.
        /// </summary>
        public async Task<PriceHistory> UpdateAsync(PriceHistory entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.PriceHistories.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Remove um histórico de preço do banco de dados.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var priceHistory = await _context.PriceHistories.FindAsync(id);
            if (priceHistory == null)
                return false;

            _context.PriceHistories.Remove(priceHistory);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Specific Queries

        /// <summary>
        /// Retorna históricos de preço de um produto específico.
        /// </summary>
        public async Task<IEnumerable<PriceHistory>> GetByProductIdAsync(int productId)
        {
            return await _context.PriceHistories
                .Include(ph => ph.Product)
                .Where(ph => ph.ProductId == productId)
                .OrderByDescending(ph => ph.ChangeDate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna o histórico de preço mais recente de um produto.
        /// </summary>
        public async Task<PriceHistory?> GetLatestByProductIdAsync(int productId)
        {
            return await _context.PriceHistories
                .Include(ph => ph.Product)
                .Where(ph => ph.ProductId == productId)
                .OrderByDescending(ph => ph.ChangeDate)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retorna históricos de preço dentro de um intervalo de datas.
        /// </summary>
        public async Task<IEnumerable<PriceHistory>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.PriceHistories
                .Include(ph => ph.Product)
                .Where(ph => ph.ChangeDate >= startDate && ph.ChangeDate <= endDate)
                .OrderByDescending(ph => ph.ChangeDate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna históricos onde o preço aumentou.
        /// </summary>
        public async Task<IEnumerable<PriceHistory>> GetPriceIncreaseHistoryAsync()
        {
            return await _context.PriceHistories
                .Include(ph => ph.Product)
                .Where(ph => ph.NewSalePrice > ph.OldSalePrice)
                .OrderByDescending(ph => ph.ChangeDate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna históricos onde o preço diminuiu.
        /// </summary>
        public async Task<IEnumerable<PriceHistory>> GetPriceDecreaseHistoryAsync()
        {
            return await _context.PriceHistories
                .Include(ph => ph.Product)
                .Where(ph => ph.NewSalePrice < ph.OldSalePrice)
                .OrderByDescending(ph => ph.ChangeDate)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region Pagination

        /// <summary>
        /// Retorna históricos paginados.
        /// </summary>
        public async Task<IEnumerable<PriceHistory>> GetPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Página deve ser maior ou igual a 1.", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Tamanho da página deve ser maior que 0.", nameof(pageSize));

            return await _context.PriceHistories
                .Include(ph => ph.Product)
                .OrderByDescending(ph => ph.ChangeDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna o total de históricos de preço.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.PriceHistories.CountAsync();
        }

        /// <summary>
        /// Retorna o total de históricos que atendem a um predicado.
        /// </summary>
        public async Task<int> GetCountAsync(Expression<Func<PriceHistory, bool>> predicate)
        {
            return await _context.PriceHistories.CountAsync(predicate);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Verifica se um histórico existe.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.PriceHistories.AnyAsync(ph => ph.PriceHistoryId == id);
        }

        #endregion
    }
}
