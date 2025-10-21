using System;
using System.Collections.Generic;

namespace VendaFlex.Infrastructure.Sync
{
    /// <summary>
    /// Configurações para o serviço de sincronização
    /// </summary>
    public class SyncConfiguration
    {
        /// <summary>
        /// Habilita sincronização automática ao iniciar a aplicação
        /// </summary>
        public bool EnableAutoSync { get; set; } = true;

        /// <summary>
        /// Intervalo em minutos para sincronização automática periódica (0 = desabilitado)
        /// </summary>
        public int AutoSyncIntervalMinutes { get; set; } = 0;

        /// <summary>
        /// Número máximo de tentativas em caso de falha
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Delay em segundos entre tentativas (exponencial backoff)
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 5;

        /// <summary>
        /// Tamanho do lote para sincronização (número de registros por vez)
        /// </summary>
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// Timeout em segundos para operações de sincronização
        /// </summary>
        public int TimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Tipos de entidades que devem ser sincronizadas (null = todas)
        /// </summary>
        public List<string>? EntitiesToSync { get; set; }

        /// <summary>
        /// Estratégia de resolução de conflitos
        /// </summary>
        public ConflictResolutionStrategy ConflictResolution { get; set; } = ConflictResolutionStrategy.ServerWins;

        /// <summary>
        /// Sincronizar apenas dados dos últimos N dias (0 = todos)
        /// </summary>
        public int SyncLastDaysOnly { get; set; } = 30;

        /// <summary>
        /// Habilitar compressão de dados durante a sincronização
        /// </summary>
        public bool EnableCompression { get; set; } = false;

        /// <summary>
        /// Sincronizar anexos e arquivos relacionados
        /// </summary>
        public bool SyncAttachments { get; set; } = true;

        /// <summary>
        /// Modo de sincronização preferido
        /// </summary>
        public SyncMode Mode { get; set; } = SyncMode.Bidirectional;
    }

    /// <summary>
    /// Estratégias para resolução de conflitos de sincronização
    /// </summary>
    public enum ConflictResolutionStrategy
    {
        /// <summary>
        /// O servidor (SQL Server) sempre prevalece
        /// </summary>
        ServerWins,

        /// <summary>
        /// O cliente (SQLite) sempre prevalece
        /// </summary>
        ClientWins,

        /// <summary>
        /// A modificação mais recente prevalece (baseado em LastModifiedUtc)
        /// </summary>
        LastWriteWins,

        /// <summary>
        /// A versão com maior número de versão prevalece
        /// </summary>
        HighestVersionWins,

        /// <summary>
        /// Requer intervenção manual do usuário
        /// </summary>
        ManualResolution
    }

    /// <summary>
    /// Modo de sincronização
    /// </summary>
    public enum SyncMode
    {
        /// <summary>
        /// Sincronização bidirecional completa
        /// </summary>
        Bidirectional,

        /// <summary>
        /// Apenas enviar dados locais para o servidor
        /// </summary>
        UploadOnly,

        /// <summary>
        /// Apenas baixar dados do servidor
        /// </summary>
        DownloadOnly
    }
}
