namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Interface legada do servi�o de sincroniza��o.
    /// RECOMENDA��O: Usar VendaFlex.Infrastructure.Sync.IAdvancedSyncService para novas implementa��es.
    /// Esta interface � mantida para compatibilidade com c�digo existente.
    /// </summary>
    [Obsolete("Use VendaFlex.Infrastructure.Sync.IAdvancedSyncService para melhor controle e recursos avan�ados")]
    public interface ISyncService
    {
        /// <summary>
        /// Sincroniza dados locais (SQLite) para o servidor (SQL Server)
        /// </summary>
        /// <returns>True se bem-sucedido, False caso contr�rio</returns>
        Task<bool> SyncToSqlServerAsync();

        /// <summary>
        /// Sincroniza dados do servidor (SQL Server) para o banco local (SQLite)
        /// </summary>
        /// <returns>True se bem-sucedido, False caso contr�rio</returns>
        Task<bool> SyncToSqliteAsync();

        /// <summary>
        /// Verifica se existem mudan�as pendentes de sincroniza��o
        /// </summary>
        /// <returns>True se h� mudan�as pendentes, False caso contr�rio</returns>
        Task<bool> HasPendingChangesAsync();
    }
}
