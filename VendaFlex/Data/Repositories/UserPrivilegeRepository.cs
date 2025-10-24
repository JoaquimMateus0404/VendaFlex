using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Reposit�rio para opera��es de acesso a dados de privil�gios de usu�rios.
    /// Respons�vel por gerenciar a associa��o entre usu�rios e privil�gios.
    /// </summary>
    public class UserPrivilegeRepository
    {
        private readonly ApplicationDbContext _context;

        public UserPrivilegeRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Basic CRUD

        public async Task<UserPrivilege?> GetByIdAsync(int id)
        {
            return await _context.UserPrivileges
                .Include(up => up.User)
                .Include(up => up.Privilege)
                .FirstOrDefaultAsync(up => up.UserPrivilegeId == id);
        }

        public async Task<UserPrivilege?> GetByIdAsNoTrackingAsync(int id)
        {
            return await _context.UserPrivileges
                .Include(up => up.User)
                .Include(up => up.Privilege)
                .AsNoTracking()
                .FirstOrDefaultAsync(up => up.UserPrivilegeId == id);
        }

        public async Task<IEnumerable<UserPrivilege>> GetAllAsync()
        {
            return await _context.UserPrivileges
                .Include(up => up.User)
                .Include(up => up.Privilege)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<UserPrivilege>> FindAsync(Expression<Func<UserPrivilege, bool>> predicate)
        {
            return await _context.UserPrivileges
                .Include(up => up.User)
                .Include(up => up.Privilege)
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<UserPrivilege> AddAsync(UserPrivilege entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.UserPrivileges.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var userPrivilege = await _context.UserPrivileges.FindAsync(id);
            if (userPrivilege == null)
                return false;

            _context.UserPrivileges.Remove(userPrivilege);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Retorna todos os privil�gios de um usu�rio espec�fico.
        /// </summary>
        public async Task<IEnumerable<UserPrivilege>> GetByUserAsync(int userId)
        {
            return await _context.UserPrivileges
                .Include(up => up.Privilege)
                .Where(up => up.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna todos os usu�rios que possuem um privil�gio espec�fico.
        /// </summary>
        public async Task<IEnumerable<UserPrivilege>> GetByPrivilegeAsync(int privilegeId)
        {
            return await _context.UserPrivileges
                .Include(up => up.User)
                .Where(up => up.PrivilegeId == privilegeId)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna privil�gios detalhados de um usu�rio.
        /// </summary>
        public async Task<IEnumerable<Privilege>> GetUserPrivilegesDetailsAsync(int userId)
        {
            return await _context.UserPrivileges
                .Where(up => up.UserId == userId)
                .Select(up => up.Privilege)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region Verification Operations

        /// <summary>
        /// Verifica se um usu�rio possui um privil�gio espec�fico.
        /// </summary>
        public async Task<bool> UserHasPrivilegeAsync(int userId, int privilegeId)
        {
            return await _context.UserPrivileges
                .AnyAsync(up => up.UserId == userId && up.PrivilegeId == privilegeId);
        }

        /// <summary>
        /// Verifica se um usu�rio possui um privil�gio por c�digo.
        /// </summary>
        public async Task<bool> UserHasPrivilegeByCodeAsync(int userId, string privilegeCode)
        {
            if (string.IsNullOrWhiteSpace(privilegeCode))
                return false;

            return await _context.UserPrivileges
                .AnyAsync(up => up.UserId == userId && up.Privilege.Code == privilegeCode);
        }

        /// <summary>
        /// Verifica se o privil�gio j� foi concedido ao usu�rio.
        /// </summary>
        public async Task<bool> ExistsAsync(int userId, int privilegeId)
        {
            return await _context.UserPrivileges
                .AnyAsync(up => up.UserId == userId && up.PrivilegeId == privilegeId);
        }

        /// <summary>
        /// Busca um privil�gio de usu�rio espec�fico.
        /// </summary>
        public async Task<UserPrivilege?> FindByUserAndPrivilegeAsync(int userId, int privilegeId)
        {
            return await _context.UserPrivileges
                .FirstOrDefaultAsync(up => up.UserId == userId && up.PrivilegeId == privilegeId);
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// Concede m�ltiplos privil�gios a um usu�rio.
        /// </summary>
        public async Task AddRangeAsync(IEnumerable<UserPrivilege> userPrivileges)
        {
            if (userPrivileges == null || !userPrivileges.Any())
                throw new ArgumentException("Lista de privil�gios n�o pode ser vazia.", nameof(userPrivileges));

            await _context.UserPrivileges.AddRangeAsync(userPrivileges);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Revoga todos os privil�gios de um usu�rio.
        /// </summary>
        public async Task<int> DeleteAllFromUserAsync(int userId)
        {
            var userPrivileges = await _context.UserPrivileges
                .Where(up => up.UserId == userId)
                .ToListAsync();

            if (!userPrivileges.Any())
                return 0;

            _context.UserPrivileges.RemoveRange(userPrivileges);
            await _context.SaveChangesAsync();

            return userPrivileges.Count;
        }

        /// <summary>
        /// Revoga um privil�gio espec�fico de um usu�rio.
        /// </summary>
        public async Task<bool> DeleteByUserAndPrivilegeAsync(int userId, int privilegeId)
        {
            var userPrivilege = await FindByUserAndPrivilegeAsync(userId, privilegeId);

            if (userPrivilege == null)
                return false;

            _context.UserPrivileges.Remove(userPrivilege);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Retorna a quantidade de privil�gios de um usu�rio.
        /// </summary>
        public async Task<int> GetUserPrivilegeCountAsync(int userId)
        {
            return await _context.UserPrivileges
                .CountAsync(up => up.UserId == userId);
        }

        /// <summary>
        /// Retorna usu�rios com mais privil�gios.
        /// </summary>
        public async Task<IEnumerable<(int UserId, int PrivilegeCount)>> GetTopUsersWithMostPrivilegesAsync(int topCount = 10)
        {
            return await _context.UserPrivileges
                .GroupBy(up => up.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    PrivilegeCount = g.Count()
                })
                .OrderByDescending(x => x.PrivilegeCount)
                .Take(topCount)
                .AsNoTracking()
                .ToListAsync()
                .ContinueWith(task => task.Result.Select(x => (x.UserId, x.PrivilegeCount)));
        }

        #endregion
    }
}