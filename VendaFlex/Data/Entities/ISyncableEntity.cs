using System;

namespace VendaFlex.Data.Entities
{
    /// <summary>
    /// Interface para entidades que suportam sincronização entre SQL Server e SQLite.
    /// Implementa controle de versão, timestamp e origem dos dados.
    /// </summary>
    public interface ISyncableEntity
    {
        /// <summary>
        /// Timestamp da última modificação (UTC). Usado para resolução de conflitos.
        /// </summary>
        DateTime LastModifiedUtc { get; set; }

        /// <summary>
        /// Versão da entidade. Incrementada a cada modificação.
        /// Usado para detecção de conflitos (Optimistic Concurrency).
        /// </summary>
        int Version { get; set; }

        /// <summary>
        /// Indica se a entidade foi criada/modificada localmente (SQLite) e precisa ser sincronizada.
        /// </summary>
        bool IsPendingSync { get; set; }

        /// <summary>
        /// Timestamp da última sincronização bem-sucedida (UTC).
        /// Null se nunca foi sincronizada.
        /// </summary>
        DateTime? LastSyncedUtc { get; set; }

        /// <summary>
        /// Identificador único global (GUID) para rastreamento entre bancos.
        /// Garante unicidade mesmo quando criado offline.
        /// </summary>
        Guid SyncGuid { get; set; }

        /// <summary>
        /// Indica a origem da última modificação (SqlServer, Sqlite).
        /// </summary>
        DataSource DataSource { get; set; }

        /// <summary>
        /// Hash dos dados da entidade. Usado para detecção rápida de mudanças.
        /// </summary>
        string? DataHash { get; set; }
    }

    /// <summary>
    /// Origem dos dados da entidade
    /// </summary>
    public enum DataSource
    {
        SqlServer = 0,
        Sqlite = 1,
        Unknown = 2
    }
}
