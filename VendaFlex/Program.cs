using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VendaFlex.Data;
using VendaFlex.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace VendaFlex
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            // Configurar diretório de dados da aplicação
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VendaFlex"
            );

            // Garantir que o diretório existe
            Directory.CreateDirectory(appDataPath);
            Directory.CreateDirectory(Path.Combine(appDataPath, "logs"));
            Directory.CreateDirectory(Path.Combine(appDataPath, "backups"));
            Directory.CreateDirectory(Path.Combine(appDataPath, "uploads"));

            // Configurar Serilog
            ConfigureSerilog(appDataPath);

            try
            {
                Log.Information("===========================================");
                Log.Information("VendaFlex iniciando...");
                Log.Information("Versão: {Version}", typeof(Program).Assembly.GetName().Version);
                Log.Information("Diretório de dados: {AppDataPath}", appDataPath);
                Log.Information("===========================================");

                // Criar configuração
                var configuration = BuildConfiguration(appDataPath);

                // Expandir variáveis de ambiente nas connection strings
                ExpandEnvironmentVariables(configuration, appDataPath);

                // Criar ServiceProvider
                var services = new ServiceCollection();
                ConfigureServices(services, configuration);
                var serviceProvider = services.BuildServiceProvider();

                Log.Information("Serviços registrados com sucesso");

                // Inicializar banco de dados
                InitializeDatabase(serviceProvider).GetAwaiter().GetResult();

                // Iniciar a aplicação WPF
                var app = new App();

                // Configurar o ServiceProvider no App antes de inicializar
                app.ServiceProvider = serviceProvider;

                Log.Information("Aplicação WPF inicializada com sucesso");

                app.Run();

                Log.Information("Aplicação encerrada normalmente");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Erro fatal ao iniciar a aplicação");
                MessageBox.Show(
                    $"Erro fatal ao iniciar o VendaFlex:\n\n{ex.Message}\n\nConsulte os logs para mais detalhes.",
                    "Erro Fatal",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureSerilog(string appDataPath)
        {
            var logPath = Path.Combine(appDataPath, "logs", "vendaflex-.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.File(
                    path: logPath,
                    rollingInterval: Serilog.RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760, // 10MB
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    shared: true
                )
                .CreateLogger();
        }

        private static IConfiguration BuildConfiguration(string appDataPath)
        {
            var configBuilder = new ConfigurationBuilder();

            // Adicionar appsettings.json do diretório da aplicação
            var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            if (File.Exists(appSettingsPath))
            {
                configBuilder.AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true);
                Log.Information("Carregado appsettings.json: {Path}", appSettingsPath);
            }
            else
            {
                Log.Warning("appsettings.json não encontrado em: {Path}", appSettingsPath);
            }

            // Adicionar configurações específicas do ambiente (se existir)
            var environment = Environment.GetEnvironmentVariable("VENDAFLEX_ENVIRONMENT") ?? "Production";
            var envSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"appsettings.{environment}.json");
            if (File.Exists(envSettingsPath))
            {
                configBuilder.AddJsonFile(envSettingsPath, optional: true, reloadOnChange: true);
                Log.Information("Carregado appsettings.{Environment}.json", environment);
            }

            // Adicionar variáveis de ambiente
            configBuilder.AddEnvironmentVariables(prefix: "VENDAFLEX_");

            return configBuilder.Build();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Registrar configuração
            services.AddSingleton(configuration);

            // Adicionar logging com Serilog
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog();
            });

            // Registrar todos os serviços do VendaFlex
            services.AddVendaFlex(configuration);
        }

        private static void ExpandEnvironmentVariables(IConfiguration configuration, string appDataPath)
        {
            // Expandir outras configurações que usam %LOCALAPPDATA%
            ExpandConfigValue(configuration, "FileStorage:RootPath", appDataPath);
            ExpandConfigValue(configuration, "Backup:BackupPath", appDataPath);
        }

        private static void ExpandConfigValue(IConfiguration config, string key, string appDataPath)
        {
            var value = config[key];
            if (!string.IsNullOrWhiteSpace(value) && value.Contains("%LOCALAPPDATA%"))
            {
                var expanded = value.Replace("%LOCALAPPDATA%", appDataPath);
                config[key] = expanded;
            }
        }

        private static async Task InitializeDatabase(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                Log.Information("Inicializando banco de dados SQL Server...");

                // Obter migrações
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();

                Log.Information("Migrações aplicadas: {Count}", appliedMigrations.Count());
                Log.Information("Migrações pendentes: {Count}", pendingMigrations.Count());

                if (pendingMigrations.Any())
                {
                    Log.Information("Aplicando migrações pendentes...");
                    await context.Database.MigrateAsync();
                    Log.Information("Migrações aplicadas com sucesso");
                }

                // Verificar se precisa de seed inicial
                await SeedInitialDataIfNeeded(context);

                Log.Information("Banco de dados inicializado com sucesso");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao inicializar banco de dados");
                throw;
            }
        }

        private static async Task SeedInitialDataIfNeeded(ApplicationDbContext context)
        {
            // Verificar se já existe CompanyConfig
            var hasConfig = await context.CompanyConfigs.AnyAsync();
            if (!hasConfig)
            {
                Log.Information("Criando configuração inicial da empresa...");

                var config = new VendaFlex.Data.Entities.CompanyConfig
                {
                    CompanyName = "Minha Empresa",
                    TaxId = "000000000",
                    Address = "Rua Principal, 123",
                    City = "Luanda",
                    Country = "Angola",
                    PhoneNumber = "+244 900 000 000",
                    Email = "contato@minhaempresa.ao",
                    Currency = "AOA",
                    CurrencySymbol = "Kz",
                    DefaultTaxRate = 14,
                    InvoicePrefix = "INV",
                    NextInvoiceNumber = 1,
                    InvoiceFormat = VendaFlex.Data.Entities.CompanyConfig.InvoiceFormatType.A4,
                    IncludeCustomerData = true,
                    AllowAnonymousInvoice = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.CompanyConfigs.Add(config);
                await context.SaveChangesAsync();

                Log.Information("Configuração inicial criada com sucesso");
            }

            // Criar usuário admin padrão se não existir
            var hasUsers = await context.Users.AnyAsync();
            if (!hasUsers)
            {
                Log.Information("Criando usuário administrador padrão...");

                // Criar pessoa para o admin
                var adminPerson = new VendaFlex.Data.Entities.Person
                {
                    Name = "Administrador do Sistema",
                    Type = VendaFlex.Data.Entities.PersonType.Employee,
                    Email = "admin@vendaflex.ao",
                    PhoneNumber = "+244 900 000 000",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Persons.Add(adminPerson);
                await context.SaveChangesAsync();

                // Criar usuário admin (senha: Admin@123)
                var adminUser = new VendaFlex.Data.Entities.User
                {
                    PersonId = adminPerson.PersonId,
                    Username = "admin",
                    PasswordHash = "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918", // Admin@123 em SHA256
                    Status = VendaFlex.Data.Entities.LoginStatus.Active,
                    LastLoginIp = string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();

                Log.Information("Usuário administrador criado com sucesso (Username: admin, Senha: Admin@123)");
                Log.Warning("IMPORTANTE: Altere a senha padrão após o primeiro login!");
            }
        }
    }
}