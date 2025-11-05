using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    public class StockMovementRepository
    {
        private readonly ApplicationDbContext _context;

        public StockMovementRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Basic CRUD

        /// <summary>
        /// Busca uma movimentação de estoque por ID.
        /// </summary>
        public async Task<StockMovement?> GetByIdAsync(int id)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User)
                .FirstOrDefaultAsync(sm => sm.StockMovementId == id);
        }

        /// <summary>
        /// Busca uma movimentação de estoque por ID sem tracking.
        /// </summary>
        public async Task<StockMovement?> GetByIdAsNoTrackingAsync(int id)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(sm => sm.StockMovementId == id);
        }

        /// <summary>
        /// Retorna todas as movimentações de estoque.
        /// </summary>
        public async Task<IEnumerable<StockMovement>> GetAllAsync()
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User)
                    .ThenInclude(u => u.Person)
                .OrderByDescending(sm => sm.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca movimentações usando um predicado customizado.
        /// </summary>
        public async Task<IEnumerable<StockMovement>> FindAsync(Expression<Func<StockMovement, bool>> predicate)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User)
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Adiciona uma nova movimentação de estoque.
        /// </summary>
        public async Task<StockMovement> AddAsync(StockMovement entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.StockMovements.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Atualiza uma movimentação de estoque existente.
        /// </summary>
        public async Task<StockMovement> UpdateAsync(StockMovement entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.StockMovements.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Remove uma movimentação de estoque do banco de dados.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var movement = await _context.StockMovements.FindAsync(id);
            if (movement == null)
                return false;

            _context.StockMovements.Remove(movement);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Specific Queries

        /// <summary>
        /// Retorna movimentações de um produto específico.
        /// </summary>
        public async Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User)
                .Where(sm => sm.ProductId == productId)
                .OrderByDescending(sm => sm.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna movimentações de um usuário específico.
        /// </summary>
        public async Task<IEnumerable<StockMovement>> GetByUserIdAsync(int userId)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User)
                .Where(sm => sm.UserId == userId)
                .OrderByDescending(sm => sm.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna movimentações por tipo.
        /// </summary>
        public async Task<IEnumerable<StockMovement>> GetByTypeAsync(StockMovementType type)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User)
                .Where(sm => sm.Type == type)
                .OrderByDescending(sm => sm.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna movimentações dentro de um intervalo de datas.
        /// </summary>
        public async Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User)
                .Where(sm => sm.Date >= startDate && sm.Date <= endDate)
                .OrderByDescending(sm => sm.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna movimentações de um produto dentro de um intervalo de datas.
        /// </summary>
        public async Task<IEnumerable<StockMovement>> GetByProductAndDateRangeAsync(int productId, DateTime startDate, DateTime endDate)
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User)
                .Where(sm => sm.ProductId == productId && sm.Date >= startDate && sm.Date <= endDate)
                .OrderByDescending(sm => sm.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Calcula o custo total das movimentações de um produto.
        /// </summary>
        public async Task<decimal> GetTotalCostByProductAsync(int productId)
        {
            return await _context.StockMovements
                .Where(sm => sm.ProductId == productId && sm.TotalCost.HasValue)
                .SumAsync(sm => sm.TotalCost ?? 0);
        }

        #endregion

        #region Pagination

        /// <summary>
        /// Retorna movimentações paginadas.
        /// </summary>
        public async Task<IEnumerable<StockMovement>> GetPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Página deve ser maior ou igual a 1.", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Tamanho da página deve ser maior que 0.", nameof(pageSize));

            return await _context.StockMovements
                .Include(sm => sm.Product)
                .Include(sm => sm.User)
                .OrderByDescending(sm => sm.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna o total de movimentações de estoque.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.StockMovements.CountAsync();
        }

        /// <summary>
        /// Retorna o total de movimentações que atendem a um predicado.
        /// </summary>
        public async Task<int> GetCountAsync(Expression<Func<StockMovement, bool>> predicate)
        {
            return await _context.StockMovements.CountAsync(predicate);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Verifica se uma movimentação existe.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.StockMovements.AnyAsync(sm => sm.StockMovementId == id);
        }

        #endregion
    }
}
