using System;
using System.IO;
using System.Linq;
using VendaFlex.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VendaFlex.Infrastructure
{
    /// <summary>
    /// Inicializa e garante a existência/migração dos bancos SQL Server e SQLite.
    /// </summary>
    public static class DatabaseInitializer
    {
        public static void InitializeDatabases(IConfiguration configuration, ILogger? logger = null, IDatabaseStatusService? status = null)
        {
            var migrationsAssembly = typeof(ApplicationDbContext).Assembly.GetName().Name;

            // 1) SQL Server: tentar aplicar migrations (cria DB se não existir)
            var sqlServerConn = configuration.GetConnectionString("DefaultConnection")
                                ?? configuration.GetConnectionString("SqlServer");
            if (!string.IsNullOrWhiteSpace(sqlServerConn))
            {
                try
                {
                    logger?.LogInformation("[DB-INIT] Preparando SQL Server...");
                    var sqlOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseSqlServer(sqlServerConn, opts =>
                        {
                            opts.MigrationsAssembly(migrationsAssembly);
                            opts.EnableRetryOnFailure();
                        })
                        .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning))
                        .Options;

                    using var sqlCtx = new ApplicationDbContext(sqlOptions);

                    // Aplicar migrations incondicionalmente. Se não houver, EnsureCreated
                    var totalMigrations = sqlCtx.Database.GetMigrations().ToList();
                    if (totalMigrations.Any())
                    {
                        logger?.LogInformation("[DB-INIT] Aplicando {Count} migrations no SQL Server...", totalMigrations.Count);
                        sqlCtx.Database.Migrate();
                        var appliedAfter = sqlCtx.Database.GetAppliedMigrations().Count();
                        var pendingAfter = sqlCtx.Database.GetPendingMigrations().Count();
                        status?.UpdateSqlServerStatus(configured: true, connected: true, applied: appliedAfter, pending: pendingAfter);
                        logger?.LogInformation("[DB-INIT] SQL Server OK. Aplicadas: {Applied}, Pendentes: {Pending}", appliedAfter, pendingAfter);
                    }
                    else
                    {
                        logger?.LogWarning("[DB-INIT] Nenhuma migration encontrada para SQL Server. Executando EnsureCreated.");
                        sqlCtx.Database.EnsureCreated();
                        status?.UpdateSqlServerStatus(configured: true, connected: true, applied: 0, pending: 0);
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "[DB-INIT] Falha ao inicializar SQL Server. Continuando com SQLite.");
                    status?.UpdateSqlServerStatus(configured: true, connected: false, applied: 0, pending: 0, error: ex.Message);
                }
            }
            else
            {
                logger?.LogInformation("[DB-INIT] ConnectionString do SQL Server não configurada. Ignorando SQL Server.");
                status?.UpdateSqlServerStatus(configured: false, connected: false, applied: 0, pending: 0, error: "Não configurado");
            }

            // 2) SQLite: construir caminho e migrar/criar
            var sqliteConn = configuration.GetConnectionString("Sqlite");
            string sqlitePath;
            if (string.IsNullOrWhiteSpace(sqliteConn))
            {
                sqlitePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "VendaFlex",
                    "VendaFlex_offline.db");
                Directory.CreateDirectory(Path.GetDirectoryName(sqlitePath)!);
                sqliteConn = $"Data Source={sqlitePath}";
                logger?.LogInformation("[DB-INIT] Usando SQLite em: {Path}", sqlitePath);
            }
            else
            {
                const string prefix = "Data Source=";
                if (sqliteConn.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    var path = sqliteConn.Substring(prefix.Length).Trim();
                    if (!Path.IsPathRooted(path))
                    {
                        var full = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "VendaFlex",
                            path);
                        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
                        sqliteConn = $"{prefix}{full}";
                        sqlitePath = full;
                        logger?.LogInformation("[DB-INIT] Normalizando caminho relativo do SQLite para: {Path}", full);
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                        sqlitePath = path;
                        logger?.LogInformation("[DB-INIT] Usando SQLite em: {Path}", path);
                    }
                }
                else
                {
                    // Tentar extrair caminho
                    sqlitePath = sqliteConn;
                }
            }

            var sqliteOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(sqliteConn!, opts => opts.MigrationsAssembly(migrationsAssembly))
                .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning))
                .Options;
            using var sqliteCtx = new ApplicationDbContext(sqliteOptions);

            try
            {
                var totalMigrations = sqliteCtx.Database.GetMigrations().ToList();
                if (totalMigrations.Any())
                {
                    logger?.LogInformation("[DB-INIT] Aplicando {Count} migrations no SQLite...", totalMigrations.Count);
                    sqliteCtx.Database.Migrate();
                    var appliedAfter = sqliteCtx.Database.GetAppliedMigrations().Count();
                    var pendingAfter = sqliteCtx.Database.GetPendingMigrations().Count();
                    status?.UpdateSqliteStatus(available: true, applied: appliedAfter, pending: pendingAfter, path: sqlitePath);
                    logger?.LogInformation("[DB-INIT] SQLite OK. Aplicadas: {Applied}, Pendentes: {Pending}", appliedAfter, pendingAfter);
                }
                else
                {
                    logger?.LogWarning("[DB-INIT] Nenhuma migration encontrada para SQLite. Executando EnsureCreated.");
                    sqliteCtx.Database.EnsureCreated();
                    status?.UpdateSqliteStatus(available: true, applied: 0, pending: 0, path: sqlitePath);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "[DB-INIT] Falha ao inicializar SQLite.");
                status?.UpdateSqliteStatus(available: false, applied: 0, pending: 0, path: sqlitePath, error: ex.Message);
            }
        }
    }
}
