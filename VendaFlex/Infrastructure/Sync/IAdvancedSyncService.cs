using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VendaFlex.Data.Entities;

namespace VendaFlex.Infrastructure.Sync
{
    /// <summary>
    /// Interface para o servi�o avan�ado de sincroniza��o entre SQL Server e SQLite
    /// </summary>
    public interface IAdvancedSyncService
    {
        /// <summary>
        /// Sincroniza dados do SQLite para o SQL Server
        /// </summary>
        Task<SyncResult> SyncToServerAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sincroniza dados do SQL Server para o SQLite
        /// </summary>
        Task<SyncResult> SyncToClientAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Sincroniza��o bidirecional completa
        /// </summary>
        Task<SyncResult> SyncBidirectionalAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica se h� mudan�as pendentes para sincroniza��o
        /// </summary>
        Task<bool> HasPendingChangesAsync();

        /// <summary>
        /// Obt�m estat�sticas sobre dados pendentes de sincroniza��o
        /// </summary>
        Task<SyncPendingInfo> GetPendingChangesInfoAsync();

        /// <summary>
        /// Verifica a conectividade com o SQL Server
        /// </summary>
        Task<bool> TestServerConnectionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolve conflito manualmente
        /// </summary>
        Task ResolveConflictAsync(SyncConflict conflict, ConflictResolution resolution);

        /// <summary>
        /// Obt�m hist�rico de sincroniza��es
        /// </summary>
        Task<List<SyncHistoryEntry>> GetSyncHistoryAsync(int count = 10);

        /// <summary>
        /// Limpa dados antigos do hist�rico de sincroniza��o
        /// </summary>
        Task CleanupOldSyncDataAsync(int daysToKeep = 30);
    }

    /// <summary>
    /// Informa��es sobre mudan�as pendentes de sincroniza��o
    /// </summary>
    public class SyncPendingInfo
    {
        public int TotalPendingRecords { get; set; }
        public Dictionary<string, int> PendingByEntity { get; set; } = new();
        public DateTime? OldestPendingChange { get; set; }
        public DateTime? NewestPendingChange { get; set; }
        public long EstimatedSizeBytes { get; set; }
    }

    /// <summary>
    /// Entrada do hist�rico de sincroniza��o
    /// </summary>
    public class SyncHistoryEntry
    {
        public int Id { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan? Duration { get; set; }
        public SyncDirection Direction { get; set; }
        public bool Success { get; set; }
        public int RecordsProcessed { get; set; }
        public int RecordsSynced { get; set; }
        public int Conflicts { get; set; }
        public int Errors { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
