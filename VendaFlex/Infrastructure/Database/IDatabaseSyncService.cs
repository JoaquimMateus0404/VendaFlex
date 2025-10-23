using System.Threading.Tasks;

namespace VendaFlex.Infrastructure.Database
{
    /// <summary>
    /// Interface para sincronização de dados entre SQL Server e SQLite (legado)
    /// Mantida por compatibilidade, mas não implementada ativamente
    /// </summary>
    public interface IDatabaseSyncService
    {
        /// <summary>
        /// Verifica se existem alterações pendentes para sincronização
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
