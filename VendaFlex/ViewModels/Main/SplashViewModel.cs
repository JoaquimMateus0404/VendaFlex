using VendaFlex.Core.Interfaces;
using VendaFlex.Infrastructure.Navigation;
using VendaFlex.Infrastructure.Database;
using VendaFlex.ViewModels.Base;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Text;

namespace VendaFlex.ViewModels.Main
{
    public class SplashViewModel : BaseViewModel
    {
        private readonly ICompanyConfigService _companyConfigService;
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;
        private readonly IDatabaseStatusService _dbStatus;
        private readonly IDatabaseSyncService _syncService;
        private readonly ILogger<SplashViewModel> _logger;
        private readonly int _minSplashMs;

        private string _statusMessage = "Inicializando...";
        private string _progressText = string.Empty;
        private bool _isLoading = true;
        private readonly StringBuilder _logBuffer = new StringBuilder();

        public SplashViewModel(ICompanyConfigService companyConfigService,
                               IUserService userService,
                               INavigationService navigationService,
                               IDatabaseStatusService dbStatus,
                               IDatabaseSyncService syncService,
                               IConfiguration configuration,
                               ILogger<SplashViewModel> logger)
        {
            _companyConfigService = companyConfigService;
            _userService = userService;
            _navigationService = navigationService;
            _dbStatus = dbStatus;
            _syncService = syncService;
            _logger = logger;

            _minSplashMs = configuration.GetValue<int?>("UI:SplashMinMilliseconds") ?? 3000;

            // Capturar logs do Serilog
            ConfigureLogCapture();

            _ = InitializeApplicationAsync();
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        public string ProgressText
        {
            get => _progressText;
            set => Set(ref _progressText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private void ConfigureLogCapture()
        {
            // Criar um sink personalizado para capturar logs
            var logSink = new LogEventSink(logEvent =>
            {
                Application.Current?.Dispatcher.InvokeAsync(() =>
                {
                    var message = logEvent.RenderMessage();
                    
                    // Filtrar logs muito verbosos
                    if (logEvent.Level >= LogEventLevel.Information)
                    {
                        _logBuffer.AppendLine($"[{logEvent.Timestamp:HH:mm:ss}] {message}");
                        
                        // Manter apenas as últimas 10 linhas
                        var lines = _logBuffer.ToString().Split('\n')
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .TakeLast(10);
                        
                        _logBuffer.Clear();
                        _logBuffer.AppendLine(string.Join("\n", lines));
                        
                        ProgressText = _logBuffer.ToString().TrimEnd();
                    }
                });
            });

            // O sink já será usado pelos logs existentes, não reconfiguramos o Serilog aqui
            // pois ele já foi configurado no Program.cs
        }

        private async Task InitializeApplicationAsync()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await Task.Delay(300); // efeito visual

                ProgressText =  "=== Iniciando VendaFlex ===";
                await Task.Delay(300);

                StatusMessage = "Verificando bancos de dados...";
                ProgressText = ("Verificando status dos bancos de dados...");
                
                await _dbStatus.RefreshStatusAsync();
                ProgressText = BuildStatusText();
                await Task.Delay(300);

                // Sincronização inicial se aplicável
                StatusMessage = "Sincronizando dados...";
                ProgressText = "Verificando sincronização de dados...";
                await Task.Delay(300);
                
                if (await _syncService.HasPendingChangesAsync())
                {
                    ProgressText = "Enviando alterações locais para o servidor...";
                    await _syncService.SyncToSqlServerAsync();
                    await Task.Delay(300);
                }
                else
                {
                    ProgressText = "Nenhuma alteração pendente para sincronização";
                    await _syncService.SyncToSqliteAsync();
                    await Task.Delay(300);
                }

                StatusMessage = "Verificando configuração...";
                ProgressText = "Conectando ao banco de dados...";
                await Task.Delay(250);

                // Verificar se a empresa está configurada
                ProgressText = "Carregando configuração da empresa...";
                var config = await _companyConfigService.GetAsync();
                await Task.Delay(300);

                StatusMessage = "Verificando usuários...";
                ProgressText = "Verificando usuários cadastrados...";
                await Task.Delay(300);

                // Garantir tempo mínimo de exibição
                var remaining = _minSplashMs - (int)sw.ElapsedMilliseconds;
                if (remaining > 0)
                {
                    await Task.Delay(remaining);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsLoading = false;

                    if (config == null)
                    {
                        StatusMessage = "Configuração inicial necessária";
                        ProgressText = "Configuração da empresa não encontrada";
                        ProgressText = "Navegando para Setup...";
                        
                        _navigationService.NavigateToSetup();
                    }
                    else
                    {
                        StatusMessage = "Sistema configurado";

                        ProgressText = "Navegando para Login...";
                        
                        _navigationService.NavigateToLogin();
                    }
                });
            }
            catch (System.Exception ex)
            {
                ProgressText = ex+ "Erro durante inicialização";
                
                // Garantir tempo mínimo de exibição, mesmo em erro
                var remaining = _minSplashMs - (int)sw.ElapsedMilliseconds;
                if (remaining > 0)
                    await Task.Delay(remaining);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = "Erro durante inicialização";
                    IsLoading = false;
                    
                    _logger.LogWarning("Redirecionando para Setup devido ao erro");
                    _navigationService.NavigateToSetup();
                });
            }
        }

        // Classe auxiliar para capturar logs
        private class LogEventSink : Serilog.Core.ILogEventSink
        {
            private readonly System.Action<LogEvent> _onLogEvent;

            public LogEventSink(System.Action<LogEvent> onLogEvent)
            {
                _onLogEvent = onLogEvent;
            }

            public void Emit(LogEvent logEvent)
            {
                _onLogEvent?.Invoke(logEvent);
            }
        }

        private string BuildStatusText()
        {
            var lines = new System.Collections.Generic.List<string>();
            lines.Add(_dbStatus.SqlServerConfigured
                ? $"SQL Server: {(_dbStatus.SqlServerConnected ? "Conectado" : "Falha")} | Aplicadas: {_dbStatus.SqlServerAppliedMigrations} | Pendentes: {_dbStatus.SqlServerPendingMigrations}"
                : "SQL Server: não configurado");

            var sqliteLine = _dbStatus.SqliteAvailable
                ? $"SQLite: OK ({_dbStatus.SqlitePath}) | Aplicadas: {_dbStatus.SqliteAppliedMigrations} | Pendentes: {_dbStatus.SqlitePendingMigrations}"
                : "SQLite: falha";
            lines.Add(sqliteLine);

            if (!string.IsNullOrWhiteSpace(_dbStatus.SqlServerError))
                lines.Add($"Erro SQL: {_dbStatus.SqlServerError}");
            if (!string.IsNullOrWhiteSpace(_dbStatus.SqliteError))
                lines.Add($"Erro SQLite: {_dbStatus.SqliteError}");

            return string.Join("\n", lines);
        }
    }
}
