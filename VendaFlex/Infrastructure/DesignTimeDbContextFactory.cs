using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using VendaFlex.Data;

namespace VendaFlex.Infrastructure
{
    /// <summary>
    /// Factory para criar o ApplicationDbContext em tempo de design (migrations, scaffolding, etc.)
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Construir configuração
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            // Obter connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                // Fallback para LocalDB se não houver connection string configurada
                connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=VendaFlexDB;Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=true;";
                Console.WriteLine($"[WARNING] Connection string não encontrada no appsettings.json. Usando LocalDB: {connectionString}");
            }

            // Criar options
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

            optionsBuilder.EnableSensitiveDataLogging(false);
            optionsBuilder.EnableDetailedErrors(true);
            
            // Configurar warnings - tratar PendingModelChangesWarning como log em vez de erro
            optionsBuilder.ConfigureWarnings(warnings =>
            {
                warnings.Log(RelationalEventId.PendingModelChangesWarning);
            });

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
