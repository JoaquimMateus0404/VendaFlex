using VendaFlex.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace VendaFlex.Infrastructure
{
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
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
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

        /// <summary>
        /// Configura o DbContext com SQL Server
        /// </summary>
        public static void ConfigureDbContext(IServiceCollection services, IConfiguration configuration)
        {
            var migrationsAssembly = typeof(ApplicationDbContext).Assembly.GetName().Name;
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada no appsettings.json");
            }

            // Obter configurações de retry do appsettings
            var maxRetryCount = configuration.GetValue<int>("Database:MaxRetryCount", 5);
            var maxRetryDelay = configuration.GetValue<int>("Database:MaxRetryDelay", 30);
            var commandTimeout = configuration.GetValue<int>("Database:CommandTimeout", 60);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(migrationsAssembly);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: maxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelay),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(commandTimeout);
                });

                // Configurações comuns
                options.EnableSensitiveDataLogging(false);
                options.EnableDetailedErrors(true);
                
                // Tratar PendingModelChangesWarning como log (não erro)
                options.ConfigureWarnings(w =>
                {
                    w.Log(RelationalEventId.PendingModelChangesWarning);
                });
            });
        }
    }
}
