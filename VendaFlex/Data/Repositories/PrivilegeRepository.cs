using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Repositório para operações de acesso a dados de privilégios.
    /// Responsável apenas por interagir com o banco de dados, sem lógica de negócio.
    /// </summary>
    public class PrivilegeRepository
    {
        private readonly ApplicationDbContext _context;

        public PrivilegeRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Basic CRUD

        public async Task<Privilege?> GetByIdAsync(int id)
        {
            return await _context.Privileges.FindAsync(id);
        }

        public async Task<Privilege?> GetByIdAsNoTrackingAsync(int id)
        {
            return await _context.Privileges
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PrivilegeId == id);
        }

        public async Task<IEnumerable<Privilege>> GetAllAsync()
        {
            return await _context.Privileges
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Privilege>> FindAsync(Expression<Func<Privilege, bool>> predicate)
        {
            return await _context.Privileges
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Privilege> AddAsync(Privilege entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.Privileges.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Privilege> UpdateAsync(Privilege entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Privileges.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var privilege = await _context.Privileges.FindAsync(id);
            if (privilege == null)
                return false;

            _context.Privileges.Remove(privilege);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Pagination

        public async Task<IEnumerable<Privilege>> GetPagedAsync(int page, int pageSize)
        {
            if (page < 1)
                throw new ArgumentException("Página deve ser maior ou igual a 1.", nameof(page));

            if (pageSize < 1)
                throw new ArgumentException("Tamanho da página deve ser maior que 0.", nameof(pageSize));

            return await _context.Privileges
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Privileges.CountAsync();
        }

        #endregion

        #region Query Operations

        public async Task<IEnumerable<Privilege>> GetActiveAsync()
        {
            return await _context.Privileges
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Privilege>> GetInactiveAsync()
        {
            return await _context.Privileges
                .Where(p => !p.IsActive)
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Privilege>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Enumerable.Empty<Privilege>();

            term = term.ToLower();

            return await _context.Privileges
                .Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Code != null && p.Code.ToLower().Contains(term)) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)))
                .OrderBy(p => p.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Privilege?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            return await _context.Privileges
                .FirstOrDefaultAsync(p => p.Code == code);
        }

        #endregion

        #region Validation Operations

        public async Task<bool> CodeExistsAsync(string code, int? excludePrivilegeId = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            var query = _context.Privileges.Where(p => p.Code == code);

            if (excludePrivilegeId.HasValue)
                query = query.Where(p => p.PrivilegeId != excludePrivilegeId.Value);

            return await query.AnyAsync();
        }

        #endregion

        #region Batch Operations

        public async Task AddRangeAsync(IEnumerable<Privilege> privileges)
        {
            if (privileges == null || !privileges.Any())
                throw new ArgumentException("Lista de privilégios não pode ser vazia.", nameof(privileges));

            await _context.Privileges.AddRangeAsync(privileges);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}