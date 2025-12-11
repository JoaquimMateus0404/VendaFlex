using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Repositório para operações relacionadas a despesas.
    /// </summary>
    public class ExpenseRepository 
    {
        private readonly ApplicationDbContext _context;

        public ExpenseRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Basic CRUD

        /// <summary>
        /// Busca uma despesa por ID.
        /// </summary>
        public async Task<Expense?> GetByIdAsync(int id)
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.ExpenseId == id);
        }

        /// <summary>
        /// Busca uma despesa por ID sem tracking.
        /// </summary>
        public async Task<Expense?> GetByIdAsNoTrackingAsync(int id)
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Include(e => e.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ExpenseId == id);
        }

        /// <summary>
        /// Retorna todas as despesas.
        /// </summary>
        public async Task<IEnumerable<Expense>> GetAllAsync()
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Include(e => e.User)
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Busca despesas usando um predicado customizado.
        /// </summary>
        public async Task<IEnumerable<Expense>> FindAsync(Expression<Func<Expense, bool>> predicate)
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Include(e => e.User)
                .Where(predicate)
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Adiciona uma nova despesa.
        /// </summary>
        public async Task<Expense> AddAsync(Expense entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.Expenses.AddAsync(entity);
            await _context.SaveChangesAsync();
            
            // Recarregar com as relações
            return await GetByIdAsync(entity.ExpenseId) ?? entity;
        }

        /// <summary>
        /// Atualiza uma despesa existente.
        /// </summary>
        public async Task<Expense> UpdateAsync(Expense entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Expenses.Update(entity);
            await _context.SaveChangesAsync();
            
            // Recarregar com as relações
            return await GetByIdAsync(entity.ExpenseId) ?? entity;
        }

        /// <summary>
        /// Remove uma despesa do banco de dados.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null)
                return false;

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Specific Queries

        /// <summary>
        /// Verifica se uma despesa existe.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Expenses.AnyAsync(e => e.ExpenseId == id);
        }

        /// <summary>
        /// Retorna despesas por tipo.
        /// </summary>
        public async Task<IEnumerable<Expense>> GetByExpenseTypeAsync(int expenseTypeId)
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Include(e => e.User)
                .Where(e => e.ExpenseTypeId == expenseTypeId)
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna despesas por usuário.
        /// </summary>
        public async Task<IEnumerable<Expense>> GetByUserAsync(int userId)
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Include(e => e.User)
                .Where(e => e.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna despesas pagas.
        /// </summary>
        public async Task<IEnumerable<Expense>> GetPaidAsync()
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Include(e => e.User)
                .Where(e => e.IsPaid)
                .AsNoTracking()
                .OrderByDescending(e => e.PaidDate)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna despesas não pagas (pendentes).
        /// </summary>
        public async Task<IEnumerable<Expense>> GetUnpaidAsync()
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Include(e => e.User)
                .Where(e => !e.IsPaid)
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna despesas por intervalo de datas.
        /// </summary>
        public async Task<IEnumerable<Expense>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Include(e => e.User)
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna despesas por referência.
        /// </summary>
        public async Task<IEnumerable<Expense>> GetByReferenceAsync(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
                return Enumerable.Empty<Expense>();

            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Include(e => e.User)
                .Where(e => e.Reference != null && e.Reference.Contains(reference))
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Retorna o total de despesas.
        /// </summary>
        public async Task<decimal> GetTotalAmountAsync()
        {
            return await _context.Expenses.SumAsync(e => e.Value);
        }

        /// <summary>
        /// Retorna o total de despesas pagas.
        /// </summary>
        public async Task<decimal> GetTotalPaidAmountAsync()
        {
            return await _context.Expenses
                .Where(e => e.IsPaid)
                .SumAsync(e => e.Value);
        }

        /// <summary>
        /// Retorna o total de despesas não pagas.
        /// </summary>
        public async Task<decimal> GetTotalUnpaidAmountAsync()
        {
            return await _context.Expenses
                .Where(e => !e.IsPaid)
                .SumAsync(e => e.Value);
        }

        /// <summary>
        /// Retorna o total de despesas por período.
        /// </summary>
        public async Task<decimal> GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Expenses
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .SumAsync(e => e.Value);
        }

        /// <summary>
        /// Retorna o total de despesas por tipo.
        /// </summary>
        public async Task<decimal> GetTotalAmountByTypeAsync(int expenseTypeId)
        {
            return await _context.Expenses
                .Where(e => e.ExpenseTypeId == expenseTypeId)
                .SumAsync(e => e.Value);
        }

        /// <summary>
        /// Retorna o número total de despesas.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Expenses.CountAsync();
        }

        /// <summary>
        /// Retorna uma lista paginada de despesas.
        /// </summary>
        public async Task<IEnumerable<Expense>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Include(e => e.User)
                .OrderByDescending(e => e.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca despesas por título ou notas.
        /// </summary>
        public async Task<IEnumerable<Expense>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var term = searchTerm.ToLower();
            return await _context.Expenses
                .Include(e => e.ExpenseType)
                .Include(e => e.User)
                .Where(e => (e.Title != null && e.Title.ToLower().Contains(term)) ||
                           (e.Notes != null && e.Notes.ToLower().Contains(term)) ||
                           (e.Reference != null && e.Reference.ToLower().Contains(term)))
                .AsNoTracking()
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        #endregion
    }
}
