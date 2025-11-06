using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;
using VendaFlex.Core.Interfaces;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Repositório para operações relacionadas ao estoque dos produtos.
    /// </summary>
    public class StockRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly Lazy<VendaFlex.Core.Services.StockAuditService> _auditService;
        private readonly ICurrentUserContext _currentUserContext;

        public StockRepository(
            ApplicationDbContext context,
            Lazy<VendaFlex.Core.Services.StockAuditService> auditService,
            ICurrentUserContext currentUserContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));
        }

        #region Basic CRUD

        /// <summary>
        /// Busca estoque por ID do produto.
        /// </summary>
        public async Task<Stock?> GetByProductIdAsync(int productId)
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.ProductId == productId);
        }

        /// <summary>
        /// Busca estoque por ID do produto sem tracking.
        /// </summary>
        public async Task<Stock?> GetByProductIdAsNoTrackingAsync(int productId)
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ProductId == productId);
        }

        /// <summary>
        /// Retorna todos os registros de estoque.
        /// </summary>
        public async Task<IEnumerable<Stock>> GetAllAsync()
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca estoques usando um predicado customizado.
        /// </summary>
        public async Task<IEnumerable<Stock>> FindAsync(Expression<Func<Stock, bool>> predicate)
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Adiciona um novo registro de estoque.
        /// </summary>
        public async Task<Stock> AddAsync(Stock entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var currentUserId = _currentUserContext.UserId; // Sempre usar o userId do contexto atual
            entity.LastStockUpdateByUserId = currentUserId;

            await _context.Stocks.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Registrar auditoria de criação
            await _auditService.Value.LogStockCreationAsync(
                entity.ProductId,
                entity.Quantity,
                currentUserId,
                "Criação inicial de estoque");

            return entity;
        }

        /// <summary>
        /// Atualiza um registro de estoque existente.
        /// </summary>
        public async Task<Stock> UpdateAsync(Stock entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Buscar estado anterior para auditoria
            var previousStock = await _context.Stocks
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ProductId == entity.ProductId);

            var currentUserId = _currentUserContext.UserId; // Sempre usar o userId do contexto atual

            entity.LastStockUpdate = DateTime.UtcNow;
            entity.LastStockUpdateByUserId = currentUserId;
            _context.Stocks.Update(entity);
            await _context.SaveChangesAsync();

            // Registrar auditoria de mudança
            if (previousStock != null && previousStock.Quantity != entity.Quantity)
            {
                await _auditService.Value.LogQuantityChangeAsync(
                    entity.ProductId,
                    previousStock.Quantity,
                    entity.Quantity,
                    currentUserId,
                    VendaFlex.Data.Entities.StockMovementType.Adjustment,
                    "Atualização de estoque");
            }

            return entity;
        }

        /// <summary>
        /// Remove um registro de estoque do banco de dados.
        /// </summary>
        public async Task<bool> DeleteAsync(int productId)
        {
            var stock = await _context.Stocks.FindAsync(productId);
            if (stock == null)
                return false;

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Stock Operations

        /// <summary>
        /// Atualiza a quantidade de estoque de um produto.
        /// </summary>
        public async Task<bool> UpdateQuantityAsync(int productId, int quantity, int? userId = null)
        {
            var stock = await _context.Stocks.FindAsync(productId);
            if (stock == null)
                return false;

            var previousQuantity = stock.Quantity;
            var currentUserId = _currentUserContext.UserId; // Sempre usar o userId do contexto atual

            stock.Quantity = quantity;
            stock.LastStockUpdate = DateTime.UtcNow;
            stock.LastStockUpdateByUserId = currentUserId;

            await _context.SaveChangesAsync();

            // Registrar auditoria (tipo será determinado automaticamente)
            await _auditService.Value.LogQuantityChangeAsync(
                productId,
                previousQuantity,
                quantity,
                currentUserId,
                VendaFlex.Data.Entities.StockMovementType.Adjustment,
                "Atualização de quantidade");

            return true;
        }

        /// <summary>
        /// Atualiza a quantidade de estoque de um produto com nota personalizada.
        /// </summary>
        public async Task<bool> UpdateQuantityAsync(int productId, int quantity, int? userId, string? notes)
        {
            System.Diagnostics.Debug.WriteLine($"[REPOSITORY DEBUG] UpdateQuantityAsync - INICIADO");
            System.Diagnostics.Debug.WriteLine($"[REPOSITORY DEBUG] ProductId: {productId}");
            System.Diagnostics.Debug.WriteLine($"[REPOSITORY DEBUG] Nova Quantidade: {quantity}");
            System.Diagnostics.Debug.WriteLine($"[REPOSITORY DEBUG] UserId recebido: {userId}");
            System.Diagnostics.Debug.WriteLine($"[REPOSITORY DEBUG] Notes: {notes}");

            var stock = await _context.Stocks.FindAsync(productId);
            if (stock == null)
            {
                System.Diagnostics.Debug.WriteLine($"[REPOSITORY DEBUG] ERRO: Stock não encontrado!");
                return false;
            }

            var previousQuantity = stock.Quantity;
            var currentUserId = _currentUserContext.UserId; // Sempre usar o userId do contexto atual

            System.Diagnostics.Debug.WriteLine($"[REPOSITORY DEBUG] Quantidade anterior: {previousQuantity}");
            System.Diagnostics.Debug.WriteLine($"[REPOSITORY DEBUG] UserId do contexto: {currentUserId}");

            stock.Quantity = quantity;
            stock.LastStockUpdate = DateTime.UtcNow;
            stock.LastStockUpdateByUserId = currentUserId;

            await _context.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine($"[REPOSITORY DEBUG] SaveChanges CONCLUÍDO");

            // Registrar auditoria (tipo será determinado automaticamente)
            System.Diagnostics.Debug.WriteLine($"[REPOSITORY DEBUG] Chamando LogQuantityChangeAsync...");
            System.Diagnostics.Debug.WriteLine($"[REPOSITORY DEBUG] Diferença: {quantity - previousQuantity}");
            
            await _auditService.Value.LogQuantityChangeAsync(
                productId,
                previousQuantity,
                quantity,
                currentUserId,
                VendaFlex.Data.Entities.StockMovementType.Adjustment,
                notes);

            System.Diagnostics.Debug.WriteLine($"[REPOSITORY DEBUG] LogQuantityChangeAsync CONCLUÍDO");
            System.Diagnostics.Debug.WriteLine($"[REPOSITORY DEBUG] UpdateQuantityAsync - FINALIZADO COM SUCESSO");

            return true;
        }

        /// <summary>
        /// Reserva uma quantidade de estoque.
        /// </summary>
        public async Task<bool> ReserveQuantityAsync(int productId, int quantity)
        {
            var stock = await _context.Stocks.FindAsync(productId);
            if (stock == null)
                return false;

            if (stock.AvailableQuantity < quantity)
                return false;

            var availableBefore = stock.AvailableQuantity;
            var currentUserId = _currentUserContext.UserId; // Sempre usar o userId do contexto atual

            stock.ReservedQuantity = (stock.ReservedQuantity ?? 0) + quantity;
            stock.LastStockUpdate = DateTime.UtcNow;
            stock.LastStockUpdateByUserId = currentUserId;

            await _context.SaveChangesAsync();

            // Registrar auditoria
            await _auditService.Value.LogReserveAsync(
                productId,
                quantity,
                availableBefore,
                currentUserId,
                $"Reserva de {quantity} unidades");

            return true;
        }

        /// <summary>
        /// Libera uma quantidade reservada de estoque.
        /// </summary>
        public async Task<bool> ReleaseReservedQuantityAsync(int productId, int quantity)
        {
            var stock = await _context.Stocks.FindAsync(productId);
            if (stock == null)
                return false;

            var currentReserved = stock.ReservedQuantity ?? 0;
            if (currentReserved < quantity)
                return false;

            var availableBefore = stock.AvailableQuantity;
            var currentUserId = _currentUserContext.UserId; // Sempre usar o userId do contexto atual

            stock.ReservedQuantity = currentReserved - quantity;
            stock.LastStockUpdate = DateTime.UtcNow;
            stock.LastStockUpdateByUserId = currentUserId;

            await _context.SaveChangesAsync();

            // Registrar auditoria
            await _auditService.Value.LogReleaseAsync(
                productId,
                quantity,
                availableBefore,
                currentUserId,
                $"Liberação de {quantity} unidades reservadas");

            return true;
        }

        /// <summary>
        /// Retorna a quantidade disponível de um produto.
        /// </summary>
        public async Task<int> GetAvailableQuantityAsync(int productId)
        {
            var stock = await _context.Stocks
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ProductId == productId);

            return stock?.AvailableQuantity ?? 0;
        }

        #endregion

        #region Specific Queries

        /// <summary>
        /// Retorna produtos com estoque baixo.
        /// </summary>
        public async Task<IEnumerable<Stock>> GetLowStockAsync()
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .Where(s => s.Product.ControlsStock && 
                           s.Product.MinimumStock.HasValue && 
                           s.Quantity <= s.Product.MinimumStock.Value)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna produtos sem estoque.
        /// </summary>
        public async Task<IEnumerable<Stock>> GetOutOfStockAsync()
        {
            return await _context.Stocks
                .Include(s => s.Product)
                .Where(s => s.Product.ControlsStock && s.Quantity <= 0)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region Validation

        /// <summary>
        /// Verifica se um estoque existe para um produto.
        /// </summary>
        public async Task<bool> ExistsAsync(int productId)
        {
            return await _context.Stocks.AnyAsync(s => s.ProductId == productId);
        }

        #endregion

        #region Pagination

        /// <summary>
        /// Retorna estoques paginados.
        /// </summary>
        public async Task<IEnumerable<Stock>> GetPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Página deve ser maior ou igual a 1.", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Tamanho da página deve ser maior que 0.", nameof(pageSize));

            return await _context.Stocks
                .Include(s => s.Product)
                .OrderBy(s => s.Product.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna o total de registros de estoque.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Stocks.CountAsync();
        }

        /// <summary>
        /// Retorna o total de estoques que atendem a um predicado.
        /// </summary>
        public async Task<int> GetCountAsync(Expression<Func<Stock, bool>> predicate)
        {
            return await _context.Stocks.CountAsync(predicate);
        }

        #endregion
    }
}
