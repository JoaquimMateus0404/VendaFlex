using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Repositório para operações relacionadas a logs de auditoria.
    /// </summary>
    public class AuditLogRepository : IRepository<AuditLog>
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuditLog> GetByIdAsync(int id)
        {
            return await _context.AuditLogs.FindAsync(id);
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs.ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> FindAsync(System.Linq.Expressions.Expression<System.Func<AuditLog, bool>> predicate)
        {
            return await _context.AuditLogs.Where(predicate).ToListAsync();
        }

        public async Task<AuditLog> AddAsync(AuditLog entity)
        {
            _context.AuditLogs.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<AuditLog> UpdateAsync(AuditLog entity)
        {
            _context.AuditLogs.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var log = await _context.AuditLogs.FindAsync(id);
            if (log == null) return false;
            _context.AuditLogs.Remove(log);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AuditLog>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Busca logs por usuário.
        /// </summary>
        public async Task<IEnumerable<AuditLog>> GetByUserAsync(int userId)
        {
            return await _context.AuditLogs
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Busca logs por entidade.
        /// </summary>
        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityName, int entityId)
        {
            return await _context.AuditLogs
                .Where(l => l.EntityName == entityName && l.EntityId == entityId)
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync();
        }
    }
}
