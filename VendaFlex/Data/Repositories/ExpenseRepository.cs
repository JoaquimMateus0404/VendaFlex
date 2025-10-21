using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Repositório para operações relacionadas a despesas.
    /// </summary>
    public class ExpenseRepository : IRepository<Expense>
    {
        private readonly ApplicationDbContext _context;

        public ExpenseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Expense> GetByIdAsync(int id)
        {
            return await _context.Expenses.FindAsync(id);
        }

        public async Task<IEnumerable<Expense>> GetAllAsync()
        {
            return await _context.Expenses.ToListAsync();
        }

        public async Task<IEnumerable<Expense>> FindAsync(System.Linq.Expressions.Expression<System.Func<Expense, bool>> predicate)
        {
            return await _context.Expenses.Where(predicate).ToListAsync();
        }

        public async Task<Expense> AddAsync(Expense entity)
        {
            _context.Expenses.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Expense> UpdateAsync(Expense entity)
        {
            _context.Expenses.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return false;
            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Expense>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.Expenses
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Obtém todos os tipos de despesa.
        /// </summary>
        public async Task<IEnumerable<ExpenseType>> GetExpenseTypesAsync()
        {
            return await _context.ExpenseTypes.ToListAsync();
        }
    }
}
