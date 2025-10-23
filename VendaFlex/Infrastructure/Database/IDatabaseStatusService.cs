using System;

namespace VendaFlex.Infrastructure.Database
{
    /// <summary>
    /// Interface para obter informações sobre o status do banco de dados
    /// </summary>
    public interface IDatabaseStatusService
    {
        /// <summary>
        /// Indica se o SQL Server está configurado
        /// </summary>
        bool SqlServerConfigured { get; }

        /// <summary>
        /// Indica se o SQL Server está conectado
        /// </summary>
        bool SqlServerConnected { get; }

        /// <summary>
        /// Número de migrações aplicadas no SQL Server
        /// </summary>
        int SqlServerAppliedMigrations { get; }

        /// <summary>
        /// Número de migrações pendentes no SQL Server
        /// </summary>
        int SqlServerPendingMigrations { get; }

        /// <summary>
        /// Mensagem de erro do SQL Server, se houver
        /// </summary>
        string? SqlServerError { get; }

        /// <summary>
        /// Indica se o SQLite está disponível (legado)
        /// </summary>
        bool SqliteAvailable { get; }

        /// <summary>
        /// Caminho do arquivo SQLite (legado)
        /// </summary>
        string? SqlitePath { get; }

        /// <summary>
        /// Número de migrações aplicadas no SQLite (legado)
        /// </summary>
        int SqliteAppliedMigrations { get; }

        /// <summary>
        /// Número de migrações pendentes no SQLite (legado)
        /// </summary>
        int SqlitePendingMigrations { get; }

        /// <summary>
        /// Mensagem de erro do SQLite, se houver (legado)
        /// </summary>
        string? SqliteError { get; }

        /// <summary>
        /// Atualiza o status do banco de dados
        /// </summary>
        System.Threading.Tasks.Task RefreshStatusAsync();
    }
}
