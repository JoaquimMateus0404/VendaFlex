using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VendaFlex.Data;

namespace VendaFlex.Infrastructure.Database
{
    public class DatabaseStatusService : IDatabaseStatusService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseStatusService> _logger;

        public bool SqlServerConfigured { get; private set; }
        public bool SqlServerConnected { get; private set; }
        public int SqlServerAppliedMigrations { get; private set; }
        public int SqlServerPendingMigrations { get; private set; }
        public string? SqlServerError { get; private set; }

        // Propriedades legado (SQLite) - mantidas por compatibilidade
        public bool SqliteAvailable { get; private set; }
        public string? SqlitePath { get; private set; }
        public int SqliteAppliedMigrations { get; private set; }
        public int SqlitePendingMigrations { get; private set; }
        public string? SqliteError { get; private set; }

        public DatabaseStatusService(ApplicationDbContext context, ILogger<DatabaseStatusService> logger)
        {
            _context = context;
            _logger = logger;

            // SQLite não é mais usado
            SqliteAvailable = false;
            SqlitePath = "N/A - SQL Server apenas";
            SqliteAppliedMigrations = 0;
            SqlitePendingMigrations = 0;
            SqliteError = null;
        }

        public async Task RefreshStatusAsync()
        {
            _logger.LogInformation("Atualizando status do banco de dados...");

            try
            {
                SqlServerConfigured = true;

                // Testar conexão
                var canConnect = await _context.Database.CanConnectAsync();
                SqlServerConnected = canConnect;

                if (canConnect)
                {
                    _logger.LogInformation("SQL Server: Conectado com sucesso");

                    // Obter migrações
                    var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
                    var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();

                    SqlServerAppliedMigrations = appliedMigrations.Count();
                    SqlServerPendingMigrations = pendingMigrations.Count();

                    _logger.LogInformation(
                        "SQL Server - Migrações aplicadas: {Applied}, Pendentes: {Pending}",
                        SqlServerAppliedMigrations,
                        SqlServerPendingMigrations
                    );

                    SqlServerError = null;
                }
                else
                {
                    _logger.LogWarning("SQL Server: Não foi possível conectar");
                    SqlServerError = "Não foi possível conectar ao banco de dados";
                    SqlServerAppliedMigrations = 0;
                    SqlServerPendingMigrations = 0;
                }
            }
            catch (Exception ex)
            {
                SqlServerConnected = false;
                SqlServerError = ex.Message;
                SqlServerAppliedMigrations = 0;
                SqlServerPendingMigrations = 0;

                _logger.LogError(ex, "Erro ao verificar status do SQL Server");
            }
        }
    }
}
