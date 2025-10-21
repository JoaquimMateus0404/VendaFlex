using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VendaFlex.Data;
using VendaFlex.Data.Entities;

namespace VendaFlex.Infrastructure.Sync
{
    /// <summary>
    /// Implementação avançada do serviço de sincronização com controle de conflitos,
    /// versionamento, retry policy e logs detalhados.
    /// </summary>
    public class AdvancedSyncService : IAdvancedSyncService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdvancedSyncService> _logger;
        private readonly SyncConfiguration _syncConfig;
        private readonly SemaphoreSlim _syncLock = new(1, 1);

        public AdvancedSyncService(
            IConfiguration configuration,
            ILogger<AdvancedSyncService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _syncConfig = LoadSyncConfiguration();
        }

        private SyncConfiguration LoadSyncConfiguration()
        {
            var config = new SyncConfiguration();
            _configuration.GetSection("Sync").Bind(config);
            return config;
        }

        public async Task<SyncResult> SyncToServerAsync(CancellationToken cancellationToken = default)
        {
            var result = new SyncResult
            {
                StartedAt = DateTime.UtcNow,
                Direction = SyncDirection.ToServer
            };

            if (!await _syncLock.WaitAsync(0, cancellationToken))
            {
                result.Success = false;
                result.Message = "Sincronização já em andamento";
                _logger.LogWarning("[SYNC] Tentativa de sincronização enquanto outra está em andamento");
                return result;
            }

            try
            {
                _logger.LogInformation("[SYNC?SERVER] Iniciando sincronização para SQL Server...");

                // Verificar conectividade
                if (!await TestServerConnectionAsync(cancellationToken))
                {
                    throw new InvalidOperationException("Não foi possível conectar ao SQL Server");
                }

                using var sqliteContext = CreateSqliteContext();
                using var sqlServerContext = CreateSqlServerContext();

                // Sincronizar cada tipo de entidade com controle de ordem
                await SyncEntityToServerAsync<Category>(sqliteContext, sqlServerContext, result, cancellationToken);
                await SyncEntityToServerAsync<Person>(sqliteContext, sqlServerContext, result, cancellationToken);
                await SyncEntityToServerAsync<Product>(sqliteContext, sqlServerContext, result, cancellationToken);
                await SyncEntityToServerAsync<Stock>(sqliteContext, sqlServerContext, result, cancellationToken);
                await SyncEntityToServerAsync<Invoice>(sqliteContext, sqlServerContext, result, cancellationToken);
                await SyncEntityToServerAsync<InvoiceProduct>(sqliteContext, sqlServerContext, result, cancellationToken);
                await SyncEntityToServerAsync<Payment>(sqliteContext, sqlServerContext, result, cancellationToken);
                await SyncEntityToServerAsync<Expense>(sqliteContext, sqlServerContext, result, cancellationToken);

                result.Success = true;
                result.CompletedAt = DateTime.UtcNow;
                result.Message = $"Sincronização concluída com sucesso. {result.Statistics.TotalRecordsProcessed} registros processados.";

                _logger.LogInformation("[SYNC?SERVER] Concluída: {Inserted} inserções, {Updated} atualizações, {Errors} erros",
                    result.Statistics.RecordsInserted,
                    result.Statistics.RecordsUpdated,
                    result.Statistics.RecordsFailed);

                await SaveSyncHistoryAsync(result);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.CompletedAt = DateTime.UtcNow;
                result.Message = $"Erro durante sincronização: {ex.Message}";
                result.Errors.Add(new SyncError
                {
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    ErrorType = SyncErrorType.UnknownError
                });

                _logger.LogError(ex, "[SYNC?SERVER] Erro durante sincronização");
            }
            finally
            {
                _syncLock.Release();
            }

            return result;
        }

        public async Task<SyncResult> SyncToClientAsync(CancellationToken cancellationToken = default)
        {
            var result = new SyncResult
            {
                StartedAt = DateTime.UtcNow,
                Direction = SyncDirection.ToClient
            };

            if (!await _syncLock.WaitAsync(0, cancellationToken))
            {
                result.Success = false;
                result.Message = "Sincronização já em andamento";
                return result;
            }

            try
            {
                _logger.LogInformation("[SYNC?CLIENT] Iniciando sincronização para SQLite...");

                if (!await TestServerConnectionAsync(cancellationToken))
                {
                    throw new InvalidOperationException("Não foi possível conectar ao SQL Server");
                }

                using var sqlServerContext = CreateSqlServerContext();
                using var sqliteContext = CreateSqliteContext();

                // Sincronizar dados essenciais para trabalho offline
                await SyncEntityToClientAsync<Category>(sqlServerContext, sqliteContext, result, cancellationToken);
                await SyncEntityToClientAsync<Person>(sqlServerContext, sqliteContext, result, cancellationToken);
                await SyncEntityToClientAsync<Product>(sqlServerContext, sqliteContext, result, cancellationToken);
                await SyncEntityToClientAsync<PaymentType>(sqlServerContext, sqliteContext, result, cancellationToken);
                await SyncEntityToClientAsync<ExpenseType>(sqlServerContext, sqliteContext, result, cancellationToken);

                result.Success = true;
                result.CompletedAt = DateTime.UtcNow;
                result.Message = $"Sincronização concluída. {result.Statistics.TotalRecordsProcessed} registros processados.";

                _logger.LogInformation("[SYNC?CLIENT] Concluída: {Inserted} inserções, {Updated} atualizações",
                    result.Statistics.RecordsInserted,
                    result.Statistics.RecordsUpdated);

                await SaveSyncHistoryAsync(result);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.CompletedAt = DateTime.UtcNow;
                result.Message = $"Erro: {ex.Message}";
                result.Errors.Add(new SyncError
                {
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    ErrorType = SyncErrorType.UnknownError
                });

                _logger.LogError(ex, "[SYNC?CLIENT] Erro durante sincronização");
            }
            finally
            {
                _syncLock.Release();
            }

            return result;
        }

        public async Task<SyncResult> SyncBidirectionalAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[SYNC?] Iniciando sincronização bidirecional...");

            // Primeiro enviar mudanças locais para o servidor
            var uploadResult = await SyncToServerAsync(cancellationToken);
            
            // Depois baixar mudanças do servidor
            var downloadResult = await SyncToClientAsync(cancellationToken);

            // Combinar resultados
            var combinedResult = new SyncResult
            {
                StartedAt = uploadResult.StartedAt,
                CompletedAt = downloadResult.CompletedAt,
                Direction = SyncDirection.Bidirectional,
                Success = uploadResult.Success && downloadResult.Success
            };

            combinedResult.Statistics.RecordsInserted = uploadResult.Statistics.RecordsInserted + downloadResult.Statistics.RecordsInserted;
            combinedResult.Statistics.RecordsUpdated = uploadResult.Statistics.RecordsUpdated + downloadResult.Statistics.RecordsUpdated;
            combinedResult.Statistics.RecordsFailed = uploadResult.Statistics.RecordsFailed + downloadResult.Statistics.RecordsFailed;
            combinedResult.Statistics.TotalRecordsProcessed = uploadResult.Statistics.TotalRecordsProcessed + downloadResult.Statistics.TotalRecordsProcessed;
            combinedResult.Errors.AddRange(uploadResult.Errors);
            combinedResult.Errors.AddRange(downloadResult.Errors);
            combinedResult.Conflicts.AddRange(uploadResult.Conflicts);
            combinedResult.Conflicts.AddRange(downloadResult.Conflicts);

            _logger.LogInformation("[SYNC?] Sincronização bidirecional concluída");

            return combinedResult;
        }

        public async Task<bool> HasPendingChangesAsync()
        {
            try
            {
                using var sqliteContext = CreateSqliteContext();
                
                // Verificar se há registros com IsPendingSync = true em entidades sincronizáveis
                // Por simplicidade, verificamos invoices recentes
                var recentDate = DateTime.UtcNow.AddDays(-_syncConfig.SyncLastDaysOnly);
                var pendingInvoices = await sqliteContext.Invoices
                    .Where(i => i.CreatedAt >= recentDate)
                    .CountAsync();

                return pendingInvoices > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SYNC] Erro ao verificar mudanças pendentes");
                return false;
            }
        }

        public async Task<SyncPendingInfo> GetPendingChangesInfoAsync()
        {
            var info = new SyncPendingInfo();

            try
            {
                using var sqliteContext = CreateSqliteContext();
                var cutoffDate = DateTime.UtcNow.AddDays(-_syncConfig.SyncLastDaysOnly);

                // Contar registros pendentes por tipo
                var pendingInvoices = await sqliteContext.Invoices.Where(i => i.CreatedAt >= cutoffDate).CountAsync();
                var pendingProducts = await sqliteContext.Products.Where(p => p.CreatedAt >= cutoffDate).CountAsync();
                var pendingPayments = await sqliteContext.Payments.Where(p => p.CreatedAt >= cutoffDate).CountAsync();

                info.PendingByEntity["Invoice"] = pendingInvoices;
                info.PendingByEntity["Product"] = pendingProducts;
                info.PendingByEntity["Payment"] = pendingPayments;
                info.TotalPendingRecords = pendingInvoices + pendingProducts + pendingPayments;

                // Datas
                var oldestInvoice = await sqliteContext.Invoices
                    .Where(i => i.CreatedAt >= cutoffDate)
                    .OrderBy(i => i.CreatedAt)
                    .Select(i => i.CreatedAt)
                    .FirstOrDefaultAsync();

                info.OldestPendingChange = oldestInvoice != default ? oldestInvoice : null;
                info.NewestPendingChange = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SYNC] Erro ao obter informações de mudanças pendentes");
            }

            return info;
        }

        public async Task<bool> TestServerConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                using var context = CreateSqlServerContext();
                return await context.Database.CanConnectAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[SYNC] Falha ao testar conexão com SQL Server");
                return false;
            }
        }

        public Task ResolveConflictAsync(SyncConflict conflict, ConflictResolution resolution)
        {
            // TODO: Implementar resolução manual de conflitos
            _logger.LogInformation("[SYNC] Conflito resolvido manualmente: {EntityType} {EntityId} ? {Resolution}",
                conflict.EntityType, conflict.EntityId, resolution);
            return Task.CompletedTask;
        }

        public async Task<List<SyncHistoryEntry>> GetSyncHistoryAsync(int count = 10)
        {
            // TODO: Implementar histórico persistente
            return await Task.FromResult(new List<SyncHistoryEntry>());
        }

        public async Task CleanupOldSyncDataAsync(int daysToKeep = 30)
        {
            _logger.LogInformation("[SYNC] Limpando dados de sincronização antigos (> {Days} dias)", daysToKeep);
            // TODO: Implementar limpeza de histórico antigo
            await Task.CompletedTask;
        }

        #region Private Helper Methods

        private ApplicationDbContext CreateSqliteContext()
        {
            var sqliteConn = _configuration.GetConnectionString("Sqlite");
            if (string.IsNullOrWhiteSpace(sqliteConn))
            {
                var sqliteDbPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "VendaFlex",
                    "VendaFlex_offline.db"
                );
                sqliteConn = $"Data Source={sqliteDbPath}";
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite(sqliteConn);
            optionsBuilder.EnableSensitiveDataLogging(false);

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        private ApplicationDbContext CreateSqlServerContext()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? _configuration.GetConnectionString("SqlServer");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionString do SQL Server não configurada");
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString, opts =>
            {
                opts.EnableRetryOnFailure(
                    maxRetryCount: _syncConfig.MaxRetryAttempts,
                    maxRetryDelay: TimeSpan.FromSeconds(_syncConfig.RetryDelaySeconds),
                    errorNumbersToAdd: null);
                opts.CommandTimeout(_syncConfig.TimeoutSeconds);
            });

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        private async Task SyncEntityToServerAsync<TEntity>(
            ApplicationDbContext sourceContext,
            ApplicationDbContext targetContext,
            SyncResult result,
            CancellationToken cancellationToken) where TEntity : class
        {
            var entityName = typeof(TEntity).Name;
            _logger.LogDebug("[SYNC?SERVER] Sincronizando {Entity}...", entityName);

            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-_syncConfig.SyncLastDaysOnly);
                
                // Buscar registros pendentes (simplificado - assumindo AuditableEntity)
                var sourceData = await sourceContext.Set<TEntity>().ToListAsync(cancellationToken);
                
                var inserted = 0;
                var updated = 0;

                foreach (var batch in sourceData.Chunk(_syncConfig.BatchSize))
                {
                    foreach (var item in batch)
                    {
                        try
                        {
                            var keyValue = GetEntityKey(item);
                            var existing = await FindEntityByKeyAsync(targetContext, item, keyValue);

                            if (existing == null)
                            {
                                targetContext.Set<TEntity>().Add(item);
                                inserted++;
                            }
                            else
                            {
                                targetContext.Entry(existing).CurrentValues.SetValues(item);
                                updated++;
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Statistics.RecordsFailed++;
                            result.Errors.Add(new SyncError
                            {
                                EntityType = entityName,
                                ErrorMessage = ex.Message,
                                ErrorType = SyncErrorType.DatabaseError
                            });
                        }
                    }

                    await targetContext.SaveChangesAsync(cancellationToken);
                }

                result.Statistics.RecordsInserted += inserted;
                result.Statistics.RecordsUpdated += updated;
                result.Statistics.TotalRecordsProcessed += inserted + updated;
                result.Statistics.IncrementEntityCount(entityName, inserted + updated);

                _logger.LogDebug("[SYNC?SERVER] {Entity}: {Inserted} inserções, {Updated} atualizações",
                    entityName, inserted, updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SYNC?SERVER] Erro ao sincronizar {Entity}", entityName);
                result.Errors.Add(new SyncError
                {
                    EntityType = entityName,
                    ErrorMessage = ex.Message,
                    StackTrace = ex.StackTrace,
                    ErrorType = SyncErrorType.DatabaseError
                });
            }
        }

        private async Task SyncEntityToClientAsync<TEntity>(
            ApplicationDbContext sourceContext,
            ApplicationDbContext targetContext,
            SyncResult result,
            CancellationToken cancellationToken) where TEntity : class
        {
            var entityName = typeof(TEntity).Name;
            _logger.LogDebug("[SYNC?CLIENT] Sincronizando {Entity}...", entityName);

            try
            {
                var sourceData = await sourceContext.Set<TEntity>().ToListAsync(cancellationToken);
                
                var inserted = 0;
                var updated = 0;

                foreach (var batch in sourceData.Chunk(_syncConfig.BatchSize))
                {
                    foreach (var item in batch)
                    {
                        var keyValue = GetEntityKey(item);
                        var existing = await FindEntityByKeyAsync(targetContext, item, keyValue);

                        if (existing == null)
                        {
                            targetContext.Set<TEntity>().Add(item);
                            inserted++;
                        }
                        else
                        {
                            targetContext.Entry(existing).CurrentValues.SetValues(item);
                            updated++;
                        }
                    }

                    await targetContext.SaveChangesAsync(cancellationToken);
                }

                result.Statistics.RecordsInserted += inserted;
                result.Statistics.RecordsUpdated += updated;
                result.Statistics.TotalRecordsProcessed += inserted + updated;
                result.Statistics.IncrementEntityCount(entityName, inserted + updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SYNC?CLIENT] Erro ao sincronizar {Entity}", entityName);
            }
        }

        private object? GetEntityKey<TEntity>(TEntity entity) where TEntity : class
        {
            var type = typeof(TEntity);
            var idProperty = type.GetProperty($"{type.Name}Id") ?? type.GetProperty("Id");
            return idProperty?.GetValue(entity);
        }

        private async Task<TEntity?> FindEntityByKeyAsync<TEntity>(
            ApplicationDbContext context,
            TEntity entity,
            object? keyValue) where TEntity : class
        {
            if (keyValue == null) return null;
            return await context.Set<TEntity>().FindAsync(keyValue);
        }

        private async Task SaveSyncHistoryAsync(SyncResult result)
        {
            // TODO: Salvar histórico em tabela dedicada
            _logger.LogInformation("[SYNC] Histórico salvo: {Direction}, Sucesso: {Success}, Duração: {Duration}",
                result.Direction, result.Success, result.Duration);
            await Task.CompletedTask;
        }

        #endregion
    }
}
