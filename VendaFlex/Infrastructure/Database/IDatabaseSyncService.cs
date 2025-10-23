using System.Threading.Tasks;

namespace VendaFlex.Infrastructure.Database
{
    /// <summary>
    /// Interface para sincroniza��o de dados entre SQL Server e SQLite (legado)
    /// Mantida por compatibilidade, mas n�o implementada ativamente
    /// </summary>
    public interface IDatabaseSyncService
    {
        /// <summary>
        /// Verifica se existem altera��es pendentes para sincroniza��o
        /// </summary>
        Task<bool> HasPendingChangesAsync();

        /// <summary>
        /// Sincroniza dados do SQLite para o SQL Server
        /// </summary>
        Task SyncToSqlServerAsync();

        /// <summary>
        /// Sincroniza dados do SQL Server para o SQLite
        /// </summary>
        Task SyncToSqliteAsync();
    }
}
