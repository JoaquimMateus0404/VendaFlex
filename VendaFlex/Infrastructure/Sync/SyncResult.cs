using System;
using System.Collections.Generic;

namespace VendaFlex.Infrastructure.Sync
{
    /// <summary>
    /// Resultado de uma operação de sincronização
    /// </summary>
    public class SyncResult
    {
        public bool Success { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public TimeSpan Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : TimeSpan.Zero;
        
        public SyncDirection Direction { get; set; }
        public SyncStatistics Statistics { get; set; } = new();
        public List<SyncError> Errors { get; set; } = new();
        public List<SyncConflict> Conflicts { get; set; } = new();
        public string? Message { get; set; }

        public bool HasErrors => Errors.Count > 0;
        public bool HasConflicts => Conflicts.Count > 0;
    }

    /// <summary>
    /// Direção da sincronização
    /// </summary>
    public enum SyncDirection
    {
        ToServer,       // SQLite ? SQL Server
        ToClient,       // SQL Server ? SQLite
        Bidirectional   // Ambas as direções
    }

    /// <summary>
    /// Estatísticas de sincronização
    /// </summary>
    public class SyncStatistics
    {
        public int TotalRecordsProcessed { get; set; }
        public int RecordsInserted { get; set; }
        public int RecordsUpdated { get; set; }
        public int RecordsDeleted { get; set; }
        public int RecordsSkipped { get; set; }
        public int RecordsFailed { get; set; }
        public int ConflictsResolved { get; set; }
        public long BytesTransferred { get; set; }
        
        public Dictionary<string, int> EntitiesProcessed { get; set; } = new();

        public void IncrementEntityCount(string entityType, int count = 1)
        {
            if (!EntitiesProcessed.ContainsKey(entityType))
                EntitiesProcessed[entityType] = 0;
            
            EntitiesProcessed[entityType] += count;
        }
    }

    /// <summary>
    /// Erro ocorrido durante sincronização
    /// </summary>
    public class SyncError
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public SyncErrorType ErrorType { get; set; }
        public bool IsRetryable { get; set; }
    }

    /// <summary>
    /// Tipo de erro de sincronização
    /// </summary>
    public enum SyncErrorType
    {
        NetworkError,
        DatabaseError,
        ValidationError,
        ConflictError,
        TimeoutError,
        AuthenticationError,
        UnknownError
    }

    /// <summary>
    /// Conflito detectado durante sincronização
    /// </summary>
    public class SyncConflict
    {
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public ConflictType Type { get; set; }
        public ConflictResolution Resolution { get; set; }
        public string? LocalVersion { get; set; }
        public string? ServerVersion { get; set; }
        public DateTime? LocalModifiedUtc { get; set; }
        public DateTime? ServerModifiedUtc { get; set; }
    }

    /// <summary>
    /// Tipo de conflito
    /// </summary>
    public enum ConflictType
    {
        ModifiedBoth,      // Modificado em ambos os lados
        DeletedLocal,      // Deletado localmente, modificado no servidor
        DeletedServer,     // Deletado no servidor, modificado localmente
        VersionMismatch,   // Versões incompatíveis
        DataCorruption     // Dados corrompidos/inconsistentes
    }

    /// <summary>
    /// Resolução aplicada ao conflito
    /// </summary>
    public enum ConflictResolution
    {
        ServerWon,
        ClientWon,
        Merged,
        Skipped,
        ManualRequired
    }
}
