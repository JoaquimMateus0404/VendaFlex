using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

                // Criar e configurar o host
                var host = CreateHostBuilder(args, appDataPath).Build();

                // Inicializar banco de dados
                InitializeDatabase(host.Services).GetAwaiter().GetResult();

                // Executar sincronização inicial se configurado
                PerformInitialSync(host.Services).GetAwaiter().GetResult();

                // Iniciar a aplicação WPF
                var app = new App();

                // Configurar o ServiceProvider no App antes de inicializar
                app.ServiceProvider = host.Services;

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

        private static IHostBuilder CreateHostBuilder(string[] args, string appDataPath)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Limpar configurações padrão
                    config.Sources.Clear();

                    // Adicionar appsettings.json do diretório da aplicação
                    var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                    if (File.Exists(appSettingsPath))
                    {
                        config.AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true);
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
                        config.AddJsonFile(envSettingsPath, optional: true, reloadOnChange: true);
                        Log.Information("Carregado appsettings.{Environment}.json", environment);
                    }

                    // Adicionar variáveis de ambiente
                    config.AddEnvironmentVariables(prefix: "VENDAFLEX_");

                    // Expandir variáveis de ambiente nas connection strings
                    var tempConfig = config.Build();
                    ExpandEnvironmentVariables(tempConfig, appDataPath);
                })
                .ConfigureServices((context, services) =>
                {
                    // Registrar configuração
                    services.AddSingleton(context.Configuration);

                    // Registrar todos os serviços do VendaFlex
                    services.AddVendaFlex(context.Configuration);

                    Log.Information("Serviços registrados com sucesso");
                });
        }

        private static void ExpandEnvironmentVariables(IConfiguration configuration, string appDataPath)
        {
            // Expandir %LOCALAPPDATA% nas connection strings
            var sqliteConn = configuration.GetConnectionString("Sqlite");
            if (!string.IsNullOrWhiteSpace(sqliteConn) && sqliteConn.Contains("%LOCALAPPDATA%"))
            {
                var expanded = sqliteConn.Replace(
                    "%LOCALAPPDATA%",
                    Path.Combine(appDataPath)
                );
                configuration["ConnectionStrings:Sqlite"] = expanded;
                Log.Information("Connection string SQLite expandida: {Conn}", expanded);
            }

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
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var statusService = scope.ServiceProvider.GetRequiredService<IDatabaseStatusService>();

            try
            {
                Log.Information("Inicializando banco de dados...");

                // Verificar qual provider está ativo
                var provider = scope.ServiceProvider.GetRequiredService<DatabaseProvider>();
                Log.Information("Provider de banco de dados ativo: {Provider}", provider);

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

                // Atualizar status do banco
                if (provider == DatabaseProvider.SqlServer)
                {
                    statusService.UpdateSqlServerStatus(
                        configured: true,
                        connected: true,
                        applied: appliedMigrations.Count(),
                        pending: 0
                    );
                }
                else
                {
                    var sqliteConn = configuration.GetConnectionString("Sqlite") ?? "";
                    var dbPath = ExtractDbPath(sqliteConn);
                    statusService.UpdateSqliteStatus(
                        available: true,
                        applied: appliedMigrations.Count(),
                        pending: 0,
                        path: dbPath
                    );
                }

                // Verificar se precisa de seed inicial
                await SeedInitialDataIfNeeded(context);

                Log.Information("Banco de dados inicializado com sucesso");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erro ao inicializar banco de dados");

                // Atualizar status com erro
                var provider = scope.ServiceProvider.GetRequiredService<DatabaseProvider>();
                if (provider == DatabaseProvider.SqlServer)
                {
                    statusService.UpdateSqlServerStatus(false, false, 0, 0, ex.Message);
                }
                else
                {
                    statusService.UpdateSqliteStatus(false, 0, 0, "", ex.Message);
                }

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

        private static async Task PerformInitialSync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var syncService = scope.ServiceProvider.GetRequiredService<IDatabaseSyncService>();

            var syncOnStartup = configuration.GetValue<bool>("Sync:SyncOnStartup", true);
            if (!syncOnStartup)
            {
                Log.Information("Sincronização automática desabilitada");
                return;
            }

            try
            {
                Log.Information("Verificando necessidade de sincronização...");

                var hasPendingChanges = await syncService.HasPendingChangesAsync();
                if (hasPendingChanges)
                {
                    Log.Information("Sincronizando dados pendentes...");
                    await syncService.SyncToSqlServerAsync();
                    Log.Information("Sincronização concluída com sucesso");
                }
                else
                {
                    Log.Information("Nenhuma sincronização necessária");
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Erro ao realizar sincronização inicial (continuando normalmente)");
            }
        }

        private static string ExtractDbPath(string connectionString)
        {
            const string dataSourcePrefix = "Data Source=";
            var index = connectionString.IndexOf(dataSourcePrefix, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var path = connectionString.Substring(index + dataSourcePrefix.Length);
                var endIndex = path.IndexOf(';');
                if (endIndex > 0)
                {
                    path = path.Substring(0, endIndex);
                }
                return path.Trim();
            }
            return connectionString;
        }
    }
}