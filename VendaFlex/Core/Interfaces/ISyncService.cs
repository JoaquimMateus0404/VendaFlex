namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Interface legada do serviço de sincronização.
    /// RECOMENDAÇÃO: Usar VendaFlex.Infrastructure.Sync.IAdvancedSyncService para novas implementações.
    /// Esta interface é mantida para compatibilidade com código existente.
    /// </summary>
    [Obsolete("Use VendaFlex.Infrastructure.Sync.IAdvancedSyncService para melhor controle e recursos avançados")]
    public interface ISyncService
    {
        /// <summary>
        /// Sincroniza dados locais (SQLite) para o servidor (SQL Server)
        /// </summary>
        /// <returns>True se bem-sucedido, False caso contrário</returns>
        Task<bool> SyncToSqlServerAsync();

        /// <summary>
        /// Sincroniza dados do servidor (SQL Server) para o banco local (SQLite)
        /// </summary>
        /// <returns>True se bem-sucedido, False caso contrário</returns>
        Task<bool> SyncToSqliteAsync();

        /// <summary>
        /// Verifica se existem mudanças pendentes de sincronização
        /// </summary>
        /// <returns>True se há mudanças pendentes, False caso contrário</returns>
        Task<bool> HasPendingChangesAsync();
    }
}
