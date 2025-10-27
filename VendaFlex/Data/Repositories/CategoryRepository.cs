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
    /// Repositório para operações relacionadas a categorias de produtos.
    /// </summary>
    public class CategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Basic CRUD

        /// <summary>
        /// Busca uma categoria por ID.
        /// </summary>
        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        /// <summary>
        /// Busca uma categoria por ID sem tracking.
        /// </summary>
        public async Task<Category?> GetByIdAsNoTrackingAsync(int id)
        {
            return await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        /// <summary>
        /// Retorna todas as categorias.
        /// </summary>
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        /// <summary>
        /// Busca categorias usando um predicado customizado.
        /// </summary>
        public async Task<IEnumerable<Category>> FindAsync(Expression<Func<Category, bool>> predicate)
        {
            return await _context.Categories
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Adiciona uma nova categoria.
        /// </summary>
        public async Task<Category> AddAsync(Category entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.Categories.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Atualiza uma categoria existente.
        /// </summary>
        public async Task<Category> UpdateAsync(Category entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Categories.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Remove uma categoria do banco de dados.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Specific Queries

        /// <summary>
        /// Retorna apenas categorias ativas.
        /// </summary>
        public async Task<IEnumerable<Category>> GetActiveAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca categoria por código.
        /// </summary>
        public async Task<Category?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Code == code);
        }

        /// <summary>
        /// Busca categorias filhas de uma categoria pai.
        /// </summary>
        public async Task<IEnumerable<Category>> GetByParentIdAsync(int? parentId)
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == parentId)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca categorias raiz (sem pai).
        /// </summary>
        public async Task<IEnumerable<Category>> GetRootCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region Search and Validation

        /// <summary>
        /// Busca categorias por termo (nome, descrição, código).
        /// </summary>
        public async Task<IEnumerable<Category>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Enumerable.Empty<Category>();

            term = term.ToLower();

            return await _context.Categories
                .Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    (c.Description != null && c.Description.ToLower().Contains(term)) ||
                    (c.Code != null && c.Code.ToLower().Contains(term)))
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Verifica se uma categoria existe.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories.AnyAsync(c => c.CategoryId == id);
        }

        /// <summary>
        /// Verifica se código já existe.
        /// </summary>
        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            if (excludeId.HasValue)
            {
                return await _context.Categories
                    .AnyAsync(c => c.Code == code && c.CategoryId != excludeId.Value);
            }

            return await _context.Categories.AnyAsync(c => c.Code == code);
        }

        /// <summary>
        /// Verifica se a categoria possui produtos.
        /// </summary>
        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products.AnyAsync(p => p.CategoryId == categoryId);
        }

        /// <summary>
        /// Retorna a quantidade de produtos na categoria.
        /// </summary>
        public async Task<int> GetProductCountAsync(int categoryId)
        {
            return await _context.Products.CountAsync(p => p.CategoryId == categoryId);
        }

        /// <summary>
        /// Verifica se a categoria possui subcategorias.
        /// </summary>
        public async Task<bool> HasChildCategoriesAsync(int categoryId)
        {
            return await _context.Categories.AnyAsync(c => c.ParentCategoryId == categoryId);
        }

        #endregion

        #region Pagination

        /// <summary>
        /// Retorna categorias paginadas.
        /// </summary>
        public async Task<IEnumerable<Category>> GetPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Página deve ser maior ou igual a 1.", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Tamanho da página deve ser maior que 0.", nameof(pageSize));

            return await _context.Categories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna o total de categorias cadastradas.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Categories.CountAsync();
        }

        /// <summary>
        /// Retorna o total de categorias que atendem a um predicado.
        /// </summary>
        public async Task<int> GetCountAsync(Expression<Func<Category, bool>> predicate)
        {
            return await _context.Categories.CountAsync(predicate);
        }

        #endregion
    }
}
