using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VendaFlex.Infrastructure.Database
{
    /// <summary>
    /// Implementa��o vazia do servi�o de sincroniza��o (legado)
    /// Mantido por compatibilidade, mas n�o realiza sincroniza��o real
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
            _logger.LogDebug("HasPendingChangesAsync: Sincroniza��o n�o implementada (apenas SQL Server)");
            return Task.FromResult(false);
        }

        public Task SyncToSqlServerAsync()
        {
            _logger.LogDebug("SyncToSqlServerAsync: Sincroniza��o n�o implementada (apenas SQL Server)");
            return Task.CompletedTask;
        }

        public Task SyncToSqliteAsync()
        {
            _logger.LogDebug("SyncToSqliteAsync: Sincroniza��o n�o implementada (apenas SQL Server)");
            return Task.CompletedTask;
        }
    }
}
