using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Reposit�rio para opera��es relacionadas a produtos.
    /// </summary>
    public class ProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Basic CRUD

        /// <summary>
        /// Busca um produto por ID.
        /// </summary>
        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        /// <summary>
        /// Busca um produto por ID sem tracking.
        /// </summary>
        public async Task<Product?> GetByIdAsNoTrackingAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        /// <summary>
        /// Retorna todos os produtos.
        /// </summary>
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca produtos usando um predicado customizado.
        /// </summary>
        public async Task<IEnumerable<Product>> FindAsync(Expression<Func<Product, bool>> predicate)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Adiciona um novo produto.
        /// </summary>
        public async Task<Product> AddAsync(Product entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                Debug.WriteLine($"[PRODUCT REPOSITORY] Adicionando produto: {entity.Name}");
                Debug.WriteLine($"[PRODUCT REPOSITORY] CategoryId: {entity.CategoryId}");
                Debug.WriteLine($"[PRODUCT REPOSITORY] SupplierId: {entity.SupplierId}");

                // Verificar se a categoria existe no banco
                var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == entity.CategoryId);
                Debug.WriteLine($"[PRODUCT REPOSITORY] Categoria existe no banco? {categoryExists}");

                // Verificar se o fornecedor existe no banco
                var supplierExists = await _context.Persons.AnyAsync(p => p.PersonId == entity.SupplierId);
                Debug.WriteLine($"[PRODUCT REPOSITORY] Fornecedor existe no banco? {supplierExists}");

                // SOLUÇÃO: Desanexar completamente as entidades de navegação do contexto
                // Isso evita conflitos de tracking do EF Core
                if (entity.Category != null)
                {
                    Debug.WriteLine($"[PRODUCT REPOSITORY] Category.CategoryId: {entity.Category.CategoryId}");
                    Debug.WriteLine($"[PRODUCT REPOSITORY] Desanexando Category do contexto...");
                    _context.Entry(entity.Category).State = EntityState.Detached;
                }
                if (entity.Supplier != null)
                {
                    Debug.WriteLine($"[PRODUCT REPOSITORY] Supplier.PersonId: {entity.Supplier.PersonId}");
                    Debug.WriteLine($"[PRODUCT REPOSITORY] Desanexando Supplier do contexto...");
                    _context.Entry(entity.Supplier).State = EntityState.Detached;
                }
                if (entity.Stock != null)
                {
                    Debug.WriteLine($"[PRODUCT REPOSITORY] Desanexando Stock do contexto...");
                    _context.Entry(entity.Stock).State = EntityState.Detached;
                }
                
                Debug.WriteLine($"[PRODUCT REPOSITORY] Verificando FKs antes de AddAsync:");
                Debug.WriteLine($"[PRODUCT REPOSITORY]   - CategoryId: {entity.CategoryId}");
                Debug.WriteLine($"[PRODUCT REPOSITORY]   - SupplierId: {entity.SupplierId}");

                Debug.WriteLine($"[PRODUCT REPOSITORY] Chamando AddAsync...");
                await _context.Products.AddAsync(entity);
                
                Debug.WriteLine($"[PRODUCT REPOSITORY] Chamando SaveChangesAsync...");
                await _context.SaveChangesAsync();
                
                Debug.WriteLine($"[PRODUCT REPOSITORY] Produto adicionado com sucesso! ProductId: {entity.ProductId}");
                return entity;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PRODUCT REPOSITORY] ERRO: {ex.GetType().Name}");
                Debug.WriteLine($"[PRODUCT REPOSITORY] Mensagem: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"[PRODUCT REPOSITORY] InnerException: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        /// <summary>
        /// Atualiza um produto existente.
        /// </summary>
        public async Task<Product> UpdateAsync(Product entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Marcar entidades relacionadas como Unchanged para não serem atualizadas
            // Isso evita que o EF tente atualizar Category, Supplier ou Stock
            if (entity.Category != null)
            {
                _context.Entry(entity.Category).State = EntityState.Unchanged;
            }
            if (entity.Supplier != null)
            {
                _context.Entry(entity.Supplier).State = EntityState.Unchanged;
            }
            if (entity.Stock != null)
            {
                _context.Entry(entity.Stock).State = EntityState.Unchanged;
            }

            // Usar Entry em vez de Update para ter mais controle
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _context.Products.Attach(entity);
                entry.State = EntityState.Modified;
            }
            else
            {
                entry.State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Remove um produto do banco de dados.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Specific Queries

        /// <summary>
        /// Retorna apenas produtos ativos.
        /// </summary>
        public async Task<IEnumerable<Product>> GetActiveAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .Where(p => p.Status == ProductStatus.Active)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca produtos por categoria.
        /// </summary>
        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .Where(p => p.CategoryId == categoryId)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca produtos por fornecedor.
        /// </summary>
        public async Task<IEnumerable<Product>> GetBySupplierIdAsync(int supplierId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .Where(p => p.SupplierId == supplierId)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca produto por código de barras.
        /// </summary>
        public async Task<Product?> GetByBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return null;

            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .FirstOrDefaultAsync(p => p.Barcode == barcode);
        }

        /// <summary>
        /// Busca produto por SKU.
        /// </summary>
        public async Task<Product?> GetBySKUAsync(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return null;

            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .FirstOrDefaultAsync(p => p.SKU == sku);
        }

        /// <summary>
        /// Busca produto por código.
        /// </summary>
        public async Task<Product?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .FirstOrDefaultAsync(p => p.Barcode == code);
        }

        /// <summary>
        /// Busca produtos por faixa de preço.
        /// </summary>
        public async Task<IEnumerable<Product>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .Where(p => p.SalePrice >= minPrice && p.SalePrice <= maxPrice)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca produtos por status.
        /// </summary>
        public async Task<IEnumerable<Product>> GetByStatusAsync(ProductStatus status)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .Where(p => p.Status == status)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna produtos com estoque baixo.
        /// </summary>
        public async Task<IEnumerable<Product>> GetLowStockAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .Where(p => p.ControlsStock && p.MinimumStock.HasValue && 
                           p.Stock != null && p.Stock.Quantity <= p.MinimumStock.Value)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna produtos sem estoque.
        /// </summary>
        public async Task<IEnumerable<Product>> GetOutOfStockAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .Where(p => p.ControlsStock && (p.Stock == null || p.Stock.Quantity <= 0))
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna produtos em destaque.
        /// </summary>
        public async Task<IEnumerable<Product>> GetFeaturedAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .Where(p => p.IsFeatured)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna produtos com controle de validade.
        /// </summary>
        public async Task<IEnumerable<Product>> GetWithExpirationAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .Where(p => p.HasExpirationDate)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region Search and Validation

        /// <summary>
        /// Busca produtos por termo (nome, descrição, código de barras, SKU).
        /// </summary>
        public async Task<IEnumerable<Product>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Enumerable.Empty<Product>();

            term = term.ToLower();

            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Description != null && p.Description.ToLower().Contains(term)) ||
                    (p.Barcode != null && p.Barcode.Contains(term)) ||
                    (p.SKU != null && p.SKU.ToLower().Contains(term)) ||
                    (p.Barcode != null && p.Barcode.ToLower().Contains(term)))
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Verifica se um produto existe.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.ProductId == id);
        }

        /// <summary>
        /// Verifica se código de barras já existe.
        /// </summary>
        public async Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return false;

            if (excludeId.HasValue)
            {
                return await _context.Products
                    .AnyAsync(p => p.Barcode == barcode && p.ProductId != excludeId.Value);
            }

            return await _context.Products.AnyAsync(p => p.Barcode == barcode);
        }

        /// <summary>
        /// Verifica se SKU já existe.
        /// </summary>
        public async Task<bool> SKUExistsAsync(string sku, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return false;

            if (excludeId.HasValue)
            {
                return await _context.Products
                    .AnyAsync(p => p.SKU == sku && p.ProductId != excludeId.Value);
            }

            return await _context.Products.AnyAsync(p => p.SKU == sku);
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
                return await _context.Products
                    .AnyAsync(p => p.Barcode == code && p.ProductId != excludeId.Value);
            }

            return await _context.Products.AnyAsync(p => p.Barcode == code);
        }

        #endregion

        #region Pagination

        /// <summary>
        /// Retorna produtos paginados.
        /// </summary>
        public async Task<IEnumerable<Product>> GetPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Página deve ser maior ou igual a 1.", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Tamanho da página deve ser maior que 0.", nameof(pageSize));

            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna o total de produtos cadastrados.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Products.CountAsync();
        }

        /// <summary>
        /// Retorna o total de produtos que atendem a um predicado.
        /// </summary>
        public async Task<int> GetCountAsync(Expression<Func<Product, bool>> predicate)
        {
            return await _context.Products.CountAsync(predicate);
        }

        #endregion
    }
}
