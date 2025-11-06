using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço responsável por criar automaticamente registros de auditoria 
    /// das movimentações de estoque. Este serviço é interno e não deve ser 
    /// usado diretamente pela UI - apenas pelo StockRepository.
    /// </summary>
    public class StockAuditService
    {
        private readonly StockMovementRepository _stockMovementRepository;

        public StockAuditService(StockMovementRepository stockMovementRepository)
        {
            _stockMovementRepository = stockMovementRepository;
        }

        /// <summary>
        /// Registra automaticamente uma movimentação de estoque quando há criação inicial.
        /// </summary>
        public async Task LogStockCreationAsync(int productId, int quantity, int? userId, string? notes = null)
        {
            if (!userId.HasValue)
                throw new ArgumentException("UserId é obrigatório para registrar movimentação de estoque.", nameof(userId));

            try
            {
                var movement = new StockMovement
                {
                    ProductId = productId,
                    UserId = userId.Value,
                    Quantity = quantity,
                    PreviousQuantity = 0,
                    NewQuantity = quantity,
                    Date = DateTime.UtcNow,
                    Type = StockMovementType.Entry,
                    Notes = notes ?? "Criação inicial de estoque",
                    Reference = $"STOCK-INIT-{productId}-{DateTime.UtcNow:yyyyMMddHHmmss}"
                };

                await _stockMovementRepository.AddAsync(movement);
            }
            catch (Exception ex)
            {
                // Log do erro mas não falhar a operação principal
                System.Diagnostics.Debug.WriteLine($"Erro ao registrar auditoria de criação: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra automaticamente uma movimentação quando a quantidade de estoque é atualizada.
        /// Detecta automaticamente se é Entry (aumento) ou Exit (diminuição).
        /// </summary>
        public async Task LogQuantityChangeAsync(
            int productId, 
            int previousQuantity, 
            int newQuantity, 
            int? userId,
            StockMovementType type = StockMovementType.Adjustment,
            string? notes = null,
            string? reference = null)
        {
            System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] LogQuantityChangeAsync - INICIADO");
            System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] ProductId: {productId}");
            System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] Quantidade Anterior: {previousQuantity}");
            System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] Quantidade Nova: {newQuantity}");
            System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] UserId: {userId}");
            System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] Tipo: {type}");
            System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] Notes: {notes}");

            if (!userId.HasValue)
            {
                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] ERRO: UserId é NULL!");
                throw new ArgumentException("UserId é obrigatório para registrar movimentação de estoque.", nameof(userId));
            }

            try
            {
                var quantityDifference = newQuantity - previousQuantity;
                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] Diferença de quantidade: {quantityDifference}");
                
                // Se o tipo é Adjustment (padrão), detectar automaticamente Entry ou Exit
                StockMovementType movementType = type;
                if (type == StockMovementType.Adjustment)
                {
                    movementType = quantityDifference > 0 ? StockMovementType.Entry : 
                                   quantityDifference < 0 ? StockMovementType.Exit : 
                                   StockMovementType.Adjustment;
                }

                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] Tipo de movimentação detectado: {movementType}");

                // Determinar a referência baseada no tipo
                string defaultReference = movementType switch
                {
                    StockMovementType.Entry => $"STOCK-ENT-{productId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    StockMovementType.Exit => $"STOCK-EXT-{productId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    _ => $"STOCK-ADJ-{productId}-{DateTime.UtcNow:yyyyMMddHHmmss}"
                };

                // Determinar a nota padrão baseada no tipo
                string defaultNotes = movementType switch
                {
                    StockMovementType.Entry => $"Entrada de estoque: {Math.Abs(quantityDifference)} unidades adicionadas",
                    StockMovementType.Exit => $"Saída de estoque: {Math.Abs(quantityDifference)} unidades removidas",
                    _ => $"Ajuste de estoque: {previousQuantity} → {newQuantity}"
                };

                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] Referência: {reference ?? defaultReference}");
                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] Notas finais: {notes ?? defaultNotes}");

                var movement = new StockMovement
                {
                    ProductId = productId,
                    UserId = userId.Value,
                    Quantity = Math.Abs(quantityDifference),
                    PreviousQuantity = previousQuantity,
                    NewQuantity = newQuantity,
                    Date = DateTime.UtcNow,
                    Type = movementType,
                    Notes = notes ?? defaultNotes,
                    Reference = reference ?? defaultReference
                };

                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] Objeto StockMovement criado:");
                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] - ProductId: {movement.ProductId}");
                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] - UserId: {movement.UserId}");
                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] - Quantity: {movement.Quantity}");
                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] - Type: {movement.Type}");
                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] Chamando _stockMovementRepository.AddAsync...");

                await _stockMovementRepository.AddAsync(movement);
                
                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] _stockMovementRepository.AddAsync CONCLUÍDO!");
                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] LogQuantityChangeAsync - FINALIZADO COM SUCESSO");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] EXCEÇÃO: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[AUDIT SERVICE DEBUG] StackTrace: {ex.StackTrace}");
                // Log do erro mas não falhar a operação principal
                System.Diagnostics.Debug.WriteLine($"Erro ao registrar auditoria de mudança: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra uma reserva de quantidade de estoque.
        /// </summary>
        public async Task LogReserveAsync(
            int productId,
            int quantity,
            int availableBefore,
            int? userId,
            string? notes = null,
            string? reference = null)
        {
            if (!userId.HasValue)
                throw new ArgumentException("UserId é obrigatório para registrar movimentação de estoque.", nameof(userId));

            try
            {
                var movement = new StockMovement
                {
                    ProductId = productId,
                    UserId = userId.Value,
                    Quantity = quantity,
                    PreviousQuantity = availableBefore,
                    NewQuantity = availableBefore - quantity,
                    Date = DateTime.UtcNow,
                    Type = StockMovementType.Exit,
                    Notes = notes ?? $"Reserva de estoque: {quantity} unidades",
                    Reference = reference ?? $"STOCK-RSV-{productId}-{DateTime.UtcNow:yyyyMMddHHmmss}"
                };

                await _stockMovementRepository.AddAsync(movement);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao registrar auditoria de reserva: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra uma liberação de quantidade reservada.
        /// </summary>
        public async Task LogReleaseAsync(
            int productId,
            int quantity,
            int availableBefore,
            int? userId,
            string? notes = null,
            string? reference = null)
        {
            if (!userId.HasValue)
                throw new ArgumentException("UserId é obrigatório para registrar movimentação de estoque.", nameof(userId));

            try
            {
                var movement = new StockMovement
                {
                    ProductId = productId,
                    UserId = userId.Value,
                    Quantity = quantity,
                    PreviousQuantity = availableBefore,
                    NewQuantity = availableBefore + quantity,
                    Date = DateTime.UtcNow,
                    Type = StockMovementType.Return,
                    Notes = notes ?? $"Liberação de reserva: {quantity} unidades",
                    Reference = reference ?? $"STOCK-REL-{productId}-{DateTime.UtcNow:yyyyMMddHHmmss}"
                };

                await _stockMovementRepository.AddAsync(movement);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao registrar auditoria de liberação: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra uma movimentação de entrada de estoque.
        /// </summary>
        public async Task LogEntryAsync(
            int productId,
            int quantity,
            int previousQuantity,
            int? userId,
            decimal? unitCost = null,
            string? notes = null,
            string? reference = null)
        {
            if (!userId.HasValue)
                throw new ArgumentException("UserId é obrigatório para registrar movimentação de estoque.", nameof(userId));

            try
            {
                var movement = new StockMovement
                {
                    ProductId = productId,
                    UserId = userId.Value,
                    Quantity = quantity,
                    PreviousQuantity = previousQuantity,
                    NewQuantity = previousQuantity + quantity,
                    Date = DateTime.UtcNow,
                    Type = StockMovementType.Entry,
                    Notes = notes ?? $"Entrada de estoque: {quantity} unidades",
                    Reference = reference ?? $"STOCK-ENT-{productId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    UnitCost = unitCost,
                    TotalCost = unitCost.HasValue ? unitCost.Value * quantity : null
                };

                await _stockMovementRepository.AddAsync(movement);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao registrar auditoria de entrada: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra uma movimentação de saída de estoque.
        /// </summary>
        public async Task LogExitAsync(
            int productId,
            int quantity,
            int previousQuantity,
            int? userId,
            string? notes = null,
            string? reference = null)
        {
            if (!userId.HasValue)
                throw new ArgumentException("UserId é obrigatório para registrar movimentação de estoque.", nameof(userId));

            try
            {
                var movement = new StockMovement
                {
                    ProductId = productId,
                    UserId = userId.Value,
                    Quantity = quantity,
                    PreviousQuantity = previousQuantity,
                    NewQuantity = previousQuantity - quantity,
                    Date = DateTime.UtcNow,
                    Type = StockMovementType.Exit,
                    Notes = notes ?? $"Saída de estoque: {quantity} unidades",
                    Reference = reference ?? $"STOCK-EXT-{productId}-{DateTime.UtcNow:yyyyMMddHHmmss}"
                };

                await _stockMovementRepository.AddAsync(movement);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao registrar auditoria de saída: {ex.Message}");
            }
        }
    }
}
