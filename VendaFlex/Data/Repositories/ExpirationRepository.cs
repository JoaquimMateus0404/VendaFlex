using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    public class ExpirationRepository
    {
        private readonly ApplicationDbContext _context;

        public ExpirationRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Basic CRUD

        /// <summary>
        /// Busca um registro de validade por ID.
        /// </summary>
        public async Task<Expiration?> GetByIdAsync(int id)
        {
            return await _context.Expirations
                .Include(e => e.Product)
                .FirstOrDefaultAsync(e => e.ExpirationId == id);
        }

        /// <summary>
        /// Busca um registro de validade por ID sem tracking.
        /// </summary>
        public async Task<Expiration?> GetByIdAsNoTrackingAsync(int id)
        {
            return await _context.Expirations
                .Include(e => e.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ExpirationId == id);
        }

        /// <summary>
        /// Retorna todos os registros de validade.
        /// </summary>
        public async Task<IEnumerable<Expiration>> GetAllAsync()
        {
            return await _context.Expirations
                .Include(e => e.Product)
                .OrderBy(e => e.ExpirationDate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca registros de validade usando um predicado customizado.
        /// </summary>
        public async Task<IEnumerable<Expiration>> FindAsync(Expression<Func<Expiration, bool>> predicate)
        {
            return await _context.Expirations
                .Include(e => e.Product)
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Adiciona um novo registro de validade.
        /// </summary>
        public async Task<Expiration> AddAsync(Expiration entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.Expirations.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Atualiza um registro de validade existente.
        /// </summary>
        public async Task<Expiration> UpdateAsync(Expiration entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Expirations.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Remove um registro de validade do banco de dados.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var expiration = await _context.Expirations.FindAsync(id);
            if (expiration == null)
                return false;

            _context.Expirations.Remove(expiration);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Specific Queries

        /// <summary>
        /// Retorna registros de validade de um produto específico.
        /// </summary>
        public async Task<IEnumerable<Expiration>> GetByProductIdAsync(int productId)
        {
            return await _context.Expirations
                .Include(e => e.Product)
                .Where(e => e.ProductId == productId)
                .OrderBy(e => e.ExpirationDate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna registros de validade por número de lote.
        /// </summary>
        public async Task<IEnumerable<Expiration>> GetByBatchNumberAsync(string batchNumber)
        {
            if (string.IsNullOrWhiteSpace(batchNumber))
                return Enumerable.Empty<Expiration>();

            return await _context.Expirations
                .Include(e => e.Product)
                .Where(e => e.BatchNumber == batchNumber)
                .OrderBy(e => e.ExpirationDate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna produtos que já expiraram.
        /// </summary>
        public async Task<IEnumerable<Expiration>> GetExpiredAsync()
        {
            var now = DateTime.Now;
            return await _context.Expirations
                .Include(e => e.Product)
                .Where(e => e.ExpirationDate < now)
                .OrderBy(e => e.ExpirationDate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna produtos próximos ao vencimento (30 dias).
        /// </summary>
        public async Task<IEnumerable<Expiration>> GetNearExpirationAsync(int daysThreshold = 30)
        {
            var now = DateTime.Now;
            var thresholdDate = now.AddDays(daysThreshold);

            return await _context.Expirations
                .Include(e => e.Product)
                .Where(e => e.ExpirationDate >= now && e.ExpirationDate <= thresholdDate)
                .OrderBy(e => e.ExpirationDate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna registros de validade dentro de um intervalo de datas.
        /// </summary>
        public async Task<IEnumerable<Expiration>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Expirations
                .Include(e => e.Product)
                .Where(e => e.ExpirationDate >= startDate && e.ExpirationDate <= endDate)
                .OrderBy(e => e.ExpirationDate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna a quantidade total de produtos expirados de um produto específico.
        /// </summary>
        public async Task<int> GetExpiredQuantityByProductAsync(int productId)
        {
            var now = DateTime.Now;
            return await _context.Expirations
                .Where(e => e.ProductId == productId && e.ExpirationDate < now)
                .SumAsync(e => e.Quantity);
        }

        #endregion

        #region Pagination

        /// <summary>
        /// Retorna registros de validade paginados.
        /// </summary>
        public async Task<IEnumerable<Expiration>> GetPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Página deve ser maior ou igual a 1.", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Tamanho da página deve ser maior que 0.", nameof(pageSize));

            return await _context.Expirations
                .Include(e => e.Product)
                .OrderBy(e => e.ExpirationDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna o total de registros de validade.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Expirations.CountAsync();
        }

        /// <summary>
        /// Retorna o total de registros que atendem a um predicado.
        /// </summary>
        public async Task<int> GetCountAsync(Expression<Func<Expiration, bool>> predicate)
        {
            return await _context.Expirations.CountAsync(predicate);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Verifica se um registro de validade existe.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Expirations.AnyAsync(e => e.ExpirationId == id);
        }

        #endregion
    }
}
