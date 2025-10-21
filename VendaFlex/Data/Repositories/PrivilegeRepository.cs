using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    public class PrivilegeRepository : IRepository<Privilege>
    {
        private readonly ApplicationDbContext _context;
        public PrivilegeRepository(ApplicationDbContext context) { _context = context; }

        public async Task<Privilege> GetByIdAsync(int id) => await _context.Privileges.FindAsync(id);
        public async Task<IEnumerable<Privilege>> GetAllAsync() => await _context.Privileges.ToListAsync();
        public async Task<IEnumerable<Privilege>> FindAsync(System.Linq.Expressions.Expression<System.Func<Privilege, bool>> predicate)
            => await _context.Privileges.Where(predicate).ToListAsync();
        public async Task<Privilege> AddAsync(Privilege entity) { _context.Privileges.Add(entity); await _context.SaveChangesAsync(); return entity; }
        public async Task<Privilege> UpdateAsync(Privilege entity) { _context.Privileges.Update(entity); await _context.SaveChangesAsync(); return entity; }
        public async Task<bool> DeleteAsync(int id) { var e = await _context.Privileges.FindAsync(id); if (e==null) return false; _context.Privileges.Remove(e); await _context.SaveChangesAsync(); return true; }
        public async Task<IEnumerable<Privilege>> GetPagedAsync(int page, int pageSize)
            => await _context.Privileges.Skip((page-1)*pageSize).Take(pageSize).ToListAsync();
    }
}
