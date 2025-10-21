using System;

namespace VendaFlex.Data.Entities
{
    /// <summary>
    /// Interface para entidades que suportam sincroniza��o entre SQL Server e SQLite.
    /// Implementa controle de vers�o, timestamp e origem dos dados.
    /// </summary>
    public interface ISyncableEntity
    {
        /// <summary>
        /// Timestamp da �ltima modifica��o (UTC). Usado para resolu��o de conflitos.
        /// </summary>
        DateTime LastModifiedUtc { get; set; }

        /// <summary>
        /// Vers�o da entidade. Incrementada a cada modifica��o.
        /// Usado para detec��o de conflitos (Optimistic Concurrency).
        /// </summary>
        int Version { get; set; }

        /// <summary>
        /// Indica se a entidade foi criada/modificada localmente (SQLite) e precisa ser sincronizada.
        /// </summary>
        bool IsPendingSync { get; set; }

        /// <summary>
        /// Timestamp da �ltima sincroniza��o bem-sucedida (UTC).
        /// Null se nunca foi sincronizada.
        /// </summary>
        DateTime? LastSyncedUtc { get; set; }

        /// <summary>
        /// Identificador �nico global (GUID) para rastreamento entre bancos.
        /// Garante unicidade mesmo quando criado offline.
        /// </summary>
        Guid SyncGuid { get; set; }

        /// <summary>
        /// Indica a origem da �ltima modifica��o (SqlServer, Sqlite).
        /// </summary>
        DataSource DataSource { get; set; }

        /// <summary>
        /// Hash dos dados da entidade. Usado para detec��o r�pida de mudan�as.
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
