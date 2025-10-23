using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VendaFlex.Infrastructure.Database
{
    /// <summary>
    /// Implementação vazia do serviço de sincronização (legado)
    /// Mantido por compatibilidade, mas não realiza sincronização real
    /// </summary>
    public class DatabaseSyncService : IDatabaseSyncService
    {
        private readonly ILogger<DatabaseSyncService> _logger;

        public DatabaseSyncService(ILogger<DatabaseSyncService> logger)
        {
            _logger = logger;
        }

        public Task<bool> HasPendingChangesAsync()
        {
            _logger.LogDebug("HasPendingChangesAsync: Sincronização não implementada (apenas SQL Server)");
            return Task.FromResult(false);
        }

        public Task SyncToSqlServerAsync()
        {
            _logger.LogDebug("SyncToSqlServerAsync: Sincronização não implementada (apenas SQL Server)");
            return Task.CompletedTask;
        }

        public Task SyncToSqliteAsync()
        {
            _logger.LogDebug("SyncToSqliteAsync: Sincronização não implementada (apenas SQL Server)");
            return Task.CompletedTask;
        }
    }
}
