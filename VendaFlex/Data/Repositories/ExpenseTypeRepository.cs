using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CommerceHub.Data.Entities;

namespace CommerceHub.Data.Repositories
{
    /// <summary>
    /// Repositório para operações relacionadas aos tipos de despesas.
    /// </summary>
    public class ExpenseTypeRepository : IRepository<ExpenseType>
    {
        private readonly ApplicationDbContext _context;

        public ExpenseTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ExpenseType> GetByIdAsync(int id)
        {
            return await _context.ExpenseTypes.FindAsync(id);
        }

        public async Task<IEnumerable<ExpenseType>> GetAllAsync()
        {
            return await _context.ExpenseTypes.ToListAsync();
        }

        public async Task<IEnumerable<ExpenseType>> FindAsync(System.Linq.Expressions.Expression<System.Func<ExpenseType, bool>> predicate)
        {
            return await _context.ExpenseTypes.Where(predicate).ToListAsync();
        }

        public async Task<ExpenseType> AddAsync(ExpenseType entity)
        {
            _context.ExpenseTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ExpenseType> UpdateAsync(ExpenseType entity)
        {
            _context.ExpenseTypes.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var expenseType = await _context.ExpenseTypes.FindAsync(id);
            if (expenseType == null) return false;
            _context.ExpenseTypes.Remove(expenseType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ExpenseType>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.ExpenseTypes
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
