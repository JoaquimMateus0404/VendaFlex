using VendaFlex.Data;
using VendaFlex.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
    /// Serviço de sincronização entre SQL Server e SQLite
    /// </summary>
    public interface IDatabaseSyncService
    {
        Task SyncToSqlServerAsync();
        Task SyncToSqliteAsync();
        Task<bool> HasPendingChangesAsync();
    }

    public class DatabaseSyncService : IDatabaseSyncService
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseProvider _currentProvider;

        public DatabaseSyncService(IConfiguration configuration, DatabaseProvider currentProvider)
        {
            _configuration = configuration;
            _currentProvider = currentProvider;
        }

        public async Task<bool> HasPendingChangesAsync()
        {
            if (_currentProvider == DatabaseProvider.Sqlite)
            {
                // Verificar se há alterações no SQLite que precisam ser sincronizadas
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

                // Verificar registros com flag de sincronização pendente (exemplo)
                var pendingInvoices = await context.Invoices
                    .Where(i => i.CreatedAt > DateTime.UtcNow.AddDays(-7))
                    .CountAsync();

                return pendingInvoices > 0;
            }

            return false;
        }

        public async Task SyncToSqlServerAsync()
        {
            if (_currentProvider != DatabaseProvider.Sqlite)
                return;

            var dbConfig = new DatabaseConfiguration(_configuration);
            if (!dbConfig.CanConnectToSqlServer())
                throw new InvalidOperationException("Não foi possível conectar ao SQL Server");

            // Implementar lógica de sincronização do SQLite para SQL Server
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

            // Sincronizar dados (exemplo com Invoices)
            var pendingInvoices = await sqliteContext.Invoices
                .Include(i => i.InvoiceProducts)
                .Include(i => i.Payments)
                .Where(i => i.CreatedAt > DateTime.UtcNow.AddDays(-7))
                .ToListAsync();

            foreach (var invoice in pendingInvoices)
            {
                // Verificar se já existe no SQL Server
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
            if (_currentProvider != DatabaseProvider.SqlServer)
                return;

            // Implementar lógica de sincronização do SQL Server para SQLite
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

            // Sincronizar dados essenciais para trabalho offline
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
