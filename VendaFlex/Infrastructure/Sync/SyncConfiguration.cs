using System;
using System.Collections.Generic;

namespace VendaFlex.Infrastructure.Sync
{
    /// <summary>
    /// Configura��es para o servi�o de sincroniza��o
    /// </summary>
    public class SyncConfiguration
    {
        /// <summary>
        /// Habilita sincroniza��o autom�tica ao iniciar a aplica��o
        /// </summary>
        public bool EnableAutoSync { get; set; } = true;

        /// <summary>
        /// Intervalo em minutos para sincroniza��o autom�tica peri�dica (0 = desabilitado)
        /// </summary>
        public int AutoSyncIntervalMinutes { get; set; } = 0;

        /// <summary>
        /// N�mero m�ximo de tentativas em caso de falha
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Delay em segundos entre tentativas (exponencial backoff)
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 5;

        /// <summary>
        /// Tamanho do lote para sincroniza��o (n�mero de registros por vez)
        /// </summary>
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// Timeout em segundos para opera��es de sincroniza��o
        /// </summary>
        public int TimeoutSeconds { get; set; } = 300;

        /// <summary>
        /// Tipos de entidades que devem ser sincronizadas (null = todas)
        /// </summary>
        public List<string>? EntitiesToSync { get; set; }

        /// <summary>
        /// Estrat�gia de resolu��o de conflitos
        /// </summary>
        public ConflictResolutionStrategy ConflictResolution { get; set; } = ConflictResolutionStrategy.ServerWins;

        /// <summary>
        /// Sincronizar apenas dados dos �ltimos N dias (0 = todos)
        /// </summary>
        public int SyncLastDaysOnly { get; set; } = 30;

        /// <summary>
        /// Habilitar compress�o de dados durante a sincroniza��o
        /// </summary>
        public bool EnableCompression { get; set; } = false;

        /// <summary>
        /// Sincronizar anexos e arquivos relacionados
        /// </summary>
        public bool SyncAttachments { get; set; } = true;

        /// <summary>
        /// Modo de sincroniza��o preferido
        /// </summary>
        public SyncMode Mode { get; set; } = SyncMode.Bidirectional;
    }

    /// <summary>
    /// Estrat�gias para resolu��o de conflitos de sincroniza��o
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
        /// A modifica��o mais recente prevalece (baseado em LastModifiedUtc)
        /// </summary>
        LastWriteWins,

        /// <summary>
        /// A vers�o com maior n�mero de vers�o prevalece
        /// </summary>
        HighestVersionWins,

        /// <summary>
        /// Requer interven��o manual do usu�rio
        /// </summary>
        ManualResolution
    }

    /// <summary>
    /// Modo de sincroniza��o
    /// </summary>
    public enum SyncMode
    {
        /// <summary>
        /// Sincroniza��o bidirecional completa
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
