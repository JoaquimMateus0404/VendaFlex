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
    /// Repositório para operações relacionadas aos tipos de despesas.
    /// </summary>
    public class ExpenseTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public ExpenseTypeRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Basic CRUD

        /// <summary>
        /// Busca um tipo de despesa por ID.
        /// </summary>
        public async Task<ExpenseType?> GetByIdAsync(int id)
        {
            return await _context.ExpenseTypes.FindAsync(id);
        }

        /// <summary>
        /// Busca um tipo de despesa por ID sem tracking.
        /// </summary>
        public async Task<ExpenseType?> GetByIdAsNoTrackingAsync(int id)
        {
            return await _context.ExpenseTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(et => et.ExpenseTypeId == id);
        }

        /// <summary>
        /// Retorna todos os tipos de despesas.
        /// </summary>
        public async Task<IEnumerable<ExpenseType>> GetAllAsync()
        {
            return await _context.ExpenseTypes
                .AsNoTracking()
                .OrderBy(et => et.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Busca tipos de despesas usando um predicado customizado.
        /// </summary>
        public async Task<IEnumerable<ExpenseType>> FindAsync(Expression<Func<ExpenseType, bool>> predicate)
        {
            return await _context.ExpenseTypes
                .Where(predicate)
                .AsNoTracking()
                .OrderBy(et => et.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Adiciona um novo tipo de despesa.
        /// </summary>
        public async Task<ExpenseType> AddAsync(ExpenseType entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.ExpenseTypes.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Atualiza um tipo de despesa existente.
        /// </summary>
        public async Task<ExpenseType> UpdateAsync(ExpenseType entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.ExpenseTypes.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Remove um tipo de despesa do banco de dados.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var expenseType = await _context.ExpenseTypes.FindAsync(id);
            if (expenseType == null)
                return false;

            _context.ExpenseTypes.Remove(expenseType);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Specific Queries

        /// <summary>
        /// Verifica se um tipo de despesa existe.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ExpenseTypes.AnyAsync(et => et.ExpenseTypeId == id);
        }

        /// <summary>
        /// Retorna apenas tipos de despesas ativos.
        /// </summary>
        public async Task<IEnumerable<ExpenseType>> GetActiveAsync()
        {
            return await _context.ExpenseTypes
                .Where(et => et.IsActive)
                .AsNoTracking()
                .OrderBy(et => et.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Busca um tipo de despesa pelo nome.
        /// </summary>
        public async Task<ExpenseType?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await _context.ExpenseTypes
                .FirstOrDefaultAsync(et => et.Name == name);
        }

        /// <summary>
        /// Verifica se já existe um tipo de despesa com o nome informado.
        /// </summary>
        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (excludeId.HasValue)
            {
                return await _context.ExpenseTypes
                    .AnyAsync(et => et.Name == name && et.ExpenseTypeId != excludeId.Value);
            }

            return await _context.ExpenseTypes.AnyAsync(et => et.Name == name);
        }

        /// <summary>
        /// Retorna a contagem de despesas associadas a um tipo.
        /// </summary>
        public async Task<int> GetExpenseCountAsync(int expenseTypeId)
        {
            return await _context.Expenses
                .Where(e => e.ExpenseTypeId == expenseTypeId)
                .CountAsync();
        }

        /// <summary>
        /// Verifica se um tipo de despesa possui despesas associadas.
        /// </summary>
        public async Task<bool> HasExpensesAsync(int expenseTypeId)
        {
            return await _context.Expenses
                .AnyAsync(e => e.ExpenseTypeId == expenseTypeId);
        }

        /// <summary>
        /// Retorna o número total de tipos de despesas.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.ExpenseTypes.CountAsync();
        }

        /// <summary>
        /// Busca tipos de despesas por termo de pesquisa.
        /// </summary>
        public async Task<IEnumerable<ExpenseType>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var term = searchTerm.ToLower();
            return await _context.ExpenseTypes
                .Where(et => (et.Name != null && et.Name.ToLower().Contains(term)) ||
                            (et.Description != null && et.Description.ToLower().Contains(term)))
                .AsNoTracking()
                .OrderBy(et => et.Name)
                .ToListAsync();
        }

        #endregion
    }
}
