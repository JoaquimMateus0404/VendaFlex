using VendaFlex.Data;
using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VendaFlex.Infrastructure.Sync;

namespace VendaFlex.Infrastructure
{
    public enum DatabaseProvider
    {
        SqlServer,
        Sqlite
    }

    public class DatabaseConfiguration
    {
        private readonly IConfiguration _configuration;

        public DatabaseConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Verifica se há conexão com SQL Server
        /// </summary>
        public bool CanConnectToSqlServer()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection")
                    ?? _configuration.GetConnectionString("SqlServer");
                if (string.IsNullOrWhiteSpace(connectionString))
                    return false;

                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                using var context = new ApplicationDbContext(optionsBuilder.Options);
                return context.Database.CanConnect();
            }
            catch
            {
                return false;
            }
        }

        private DatabaseProvider GetConfiguredOrActiveProvider()
        {
            var configured = _configuration["Database:Provider"];
            if (!string.IsNullOrWhiteSpace(configured))
            {
                if (string.Equals(configured, "SqlServer", StringComparison.OrdinalIgnoreCase))
                    return DatabaseProvider.SqlServer;
                if (string.Equals(configured, "Sqlite", StringComparison.OrdinalIgnoreCase))
                    return DatabaseProvider.Sqlite;
            }

            // Auto-detect (default)
            if (CanConnectToSqlServer())
            {
                return DatabaseProvider.SqlServer;
            }
            return DatabaseProvider.Sqlite;
        }

        /// <summary>
        /// Configura o DbContext com o provider adequado, respeitando appsettings (Database:Provider)
        /// </summary>
        public static void ConfigureDbContext(IServiceCollection services, IConfiguration configuration)
        {
            var dbConfig = new DatabaseConfiguration(configuration);
            var provider = dbConfig.GetConfiguredOrActiveProvider();
            var migrationsAssembly = typeof(ApplicationDbContext).Assembly.GetName().Name;

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (provider == DatabaseProvider.SqlServer)
                {
                    var connectionString = configuration.GetConnectionString("DefaultConnection")
                        ?? configuration.GetConnectionString("SqlServer");
                    if (string.IsNullOrWhiteSpace(connectionString))
                    {
                        throw new InvalidOperationException("ConnectionStrings:SqlServer/DefaultConnection não configurada");
                    }

                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(migrationsAssembly);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
                }
                else
                {
                    // Preferir a connection string do appsettings se existir
                    var sqliteConn = configuration.GetConnectionString("Sqlite");
                    if (string.IsNullOrWhiteSpace(sqliteConn))
                    {
                        var sqliteDbPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "VendaFlex",
                            "VendaFlex_offline.db"
                        );

                        // Garantir que o diretório existe
                        var directory = Path.GetDirectoryName(sqliteDbPath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        sqliteConn = $"Data Source={sqliteDbPath}";
                    }
                    else
                    {
                        // Se o caminho no connstring for relativo, movê-lo para %LOCALAPPDATA%/VendaFlex
                        const string dataSourcePrefix = "Data Source=";
                        if (sqliteConn.StartsWith(dataSourcePrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            var path = sqliteConn.Substring(dataSourcePrefix.Length).Trim();
                            if (!Path.IsPathRooted(path))
                            {
                                var full = Path.Combine(
                                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                    "VendaFlex",
                                    path);
                                var dir = Path.GetDirectoryName(full);
                                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                                    Directory.CreateDirectory(dir);
                                sqliteConn = $"{dataSourcePrefix}{full}";
                            }
                            else
                            {
                                var dir = Path.GetDirectoryName(path);
                                if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
                                    Directory.CreateDirectory(dir);
                            }
                        }
                    }

                    options.UseSqlite(sqliteConn, sqliteOptions =>
                    {
                        sqliteOptions.MigrationsAssembly(migrationsAssembly);
                    });
                }

                // Configurações comuns
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(true);
                // Tratar PendingModelChangesWarning como log (não erro)
                options.ConfigureWarnings(w =>
                {
                    w.Log(RelationalEventId.PendingModelChangesWarning);
                });
            });

            // Registrar o provider ativo como singleton (usar overload não genérico para permitir enum)
            services.AddSingleton(typeof(DatabaseProvider), provider);
        }
    }

    /// <summary>
    /// Serviço de sincronização entre SQL Server e SQLite (Implementação legada)
    /// RECOMENDAÇÃO: Usar IAdvancedSyncService para novas implementações
    /// </summary>
    public interface IDatabaseSyncService
    {
        Task SyncToSqlServerAsync();
        Task SyncToSqliteAsync();
        Task<bool> HasPendingChangesAsync();
    }

    /// <summary>
    /// Implementação legada do serviço de sincronização.
    /// Esta classe é mantida para compatibilidade, mas delega para AdvancedSyncService.
    /// </summary>
    public class DatabaseSyncService : IDatabaseSyncService
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseProvider _currentProvider;
        private readonly IAdvancedSyncService? _advancedSyncService;

        public DatabaseSyncService(
            IConfiguration configuration,
            DatabaseProvider currentProvider,
            IAdvancedSyncService? advancedSyncService = null)
        {
            _configuration = configuration;
            _currentProvider = currentProvider;
            _advancedSyncService = advancedSyncService;
        }

        public async Task<bool> HasPendingChangesAsync()
        {
            // Se temos o serviço avançado disponível, usá-lo
            if (_advancedSyncService != null)
            {
                return await _advancedSyncService.HasPendingChangesAsync();
            }

            // Implementação legada
            if (_currentProvider == DatabaseProvider.Sqlite)
            {
                var sqliteDbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "VendaFlex",
                    "VendaFlex_offline.db"
                );

                if (!File.Exists(sqliteDbPath))
                    return false;

                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseSqlite($"Data Source={sqliteDbPath}");

                using var context = new ApplicationDbContext(optionsBuilder.Options);

                // Verificar registros recentes
                var pendingInvoices = await context.Invoices
                    .Where(i => i.CreatedAt > DateTime.UtcNow.AddDays(-7))
                    .CountAsync();

                return pendingInvoices > 0;
            }

            return false;
        }

        public async Task SyncToSqlServerAsync()
        {
            // Se temos o serviço avançado, delegar para ele
            if (_advancedSyncService != null)
            {
                var result = await _advancedSyncService.SyncToServerAsync();
                if (!result.Success)
                {
                    throw new InvalidOperationException($"Sincronização falhou: {result.Message}");
                }
                return;
            }

            // Implementação legada (simplificada)
            if (_currentProvider != DatabaseProvider.Sqlite)
                return;

            var dbConfig = new DatabaseConfiguration(_configuration);
            if (!dbConfig.CanConnectToSqlServer())
                throw new InvalidOperationException("Não foi possível conectar ao SQL Server");

            var sqliteDbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VendaFlex",
                "VendaFlex_offline.db"
            );

            var sqliteOptionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            sqliteOptionsBuilder.UseSqlite($"Data Source={sqliteDbPath}");

            var sqlServerOptionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            sqlServerOptionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")
                ?? _configuration.GetConnectionString("SqlServer"));

            using var sqliteContext = new ApplicationDbContext(sqliteOptionsBuilder.Options);
            using var sqlServerContext = new ApplicationDbContext(sqlServerOptionsBuilder.Options);

            // Sincronizar faturas recentes
            var pendingInvoices = await sqliteContext.Invoices
                .Include(i => i.InvoiceProducts)
                .Include(i => i.Payments)
                .Where(i => i.CreatedAt > DateTime.UtcNow.AddDays(-7))
                .ToListAsync();

            foreach (var invoice in pendingInvoices)
            {
                var existingInvoice = await sqlServerContext.Invoices
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId);

                if (existingInvoice == null)
                {
                    sqlServerContext.Invoices.Add(invoice);
                }
                else
                {
                    sqlServerContext.Entry(existingInvoice).CurrentValues.SetValues(invoice);
                }
            }

            await sqlServerContext.SaveChangesAsync();
        }

        public async Task SyncToSqliteAsync()
        {
            // Se temos o serviço avançado, delegar para ele
            if (_advancedSyncService != null)
            {
                var result = await _advancedSyncService.SyncToClientAsync();
                if (!result.Success)
                {
                    throw new InvalidOperationException($"Sincronização falhou: {result.Message}");
                }
                return;
            }

            // Implementação legada
            if (_currentProvider != DatabaseProvider.SqlServer)
                return;

            var sqliteDbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VendaFlex",
                "VendaFlex_offline.db"
            );

            var sqliteOptionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            sqliteOptionsBuilder.UseSqlite($"Data Source={sqliteDbPath}");

            var sqlServerOptionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            sqlServerOptionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection")
                ?? _configuration.GetConnectionString("SqlServer"));

            using var sqlServerContext = new ApplicationDbContext(sqlServerOptionsBuilder.Options);
            using var sqliteContext = new ApplicationDbContext(sqliteOptionsBuilder.Options);

            // Garantir que o banco SQLite está criado
            await sqliteContext.Database.EnsureCreatedAsync();

            // Sincronizar produtos ativos
            var recentProducts = await sqlServerContext.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .Where(p => p.Status == ProductStatus.Active)
                .Take(1000)
                .ToListAsync();

            foreach (var product in recentProducts)
            {
                var existingProduct = await sqliteContext.Products
                    .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);

                if (existingProduct == null)
                {
                    sqliteContext.Products.Add(product);
                }
                else
                {
                    sqliteContext.Entry(existingProduct).CurrentValues.SetValues(product);
                }
            }

            await sqliteContext.SaveChangesAsync();
        }
    }
}
