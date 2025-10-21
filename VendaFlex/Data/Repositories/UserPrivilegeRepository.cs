using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Data.Entities;

namespace CommerceHub.Data.Repositories
{
    public class UserPrivilegeRepository : IRepository<UserPrivilege>
    {
        private readonly ApplicationDbContext _context;
        public UserPrivilegeRepository(ApplicationDbContext context) { _context = context; }

        public async Task<UserPrivilege> GetByIdAsync(int id) => await _context.UserPrivileges.FindAsync(id);
        public async Task<IEnumerable<UserPrivilege>> GetAllAsync() => await _context.UserPrivileges.ToListAsync();
        public async Task<IEnumerable<UserPrivilege>> FindAsync(System.Linq.Expressions.Expression<System.Func<UserPrivilege, bool>> predicate)
            => await _context.UserPrivileges.Where(predicate).ToListAsync();
        public async Task<UserPrivilege> AddAsync(UserPrivilege entity) { _context.UserPrivileges.Add(entity); await _context.SaveChangesAsync(); return entity; }
        public async Task<UserPrivilege> UpdateAsync(UserPrivilege entity) { _context.UserPrivileges.Update(entity); await _context.SaveChangesAsync(); return entity; }
        public async Task<bool> DeleteAsync(int id) { var e = await _context.UserPrivileges.FindAsync(id); if (e==null) return false; _context.UserPrivileges.Remove(e); await _context.SaveChangesAsync(); return true; }
        public async Task<IEnumerable<UserPrivilege>> GetPagedAsync(int page, int pageSize)
            => await _context.UserPrivileges.Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
    }
}
