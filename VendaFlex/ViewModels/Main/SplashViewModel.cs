using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using VendaFlex.Core.Interfaces;
using VendaFlex.Infrastructure.Database;
using VendaFlex.Infrastructure.Navigation;
using VendaFlex.ViewModels.Base;

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
        private readonly StringBuilder _logBuffer = new();

        private string _statusMessage = "Inicializando...";
        private string _progressText = string.Empty;
        private bool _isLoading = true;

        public SplashViewModel(
            ICompanyConfigService companyConfigService,
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

            _minSplashMs = configuration.GetValue("UI:SplashMinMilliseconds", 3000);
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

        #region Inicialização
        private async Task InitializeApplicationAsync()
        {
            var sw = Stopwatch.StartNew();

            try
            {
                await MostrarStatusAsync("Iniciando VendaFlex", delay: 2000);

                await VerificarBancosAsync();
                await SincronizarDadosAsync();
                await VerificarConfiguracoesAsync();

                await GarantirTempoMinimoAsync(sw.ElapsedMilliseconds);

                NavegarConformeStatus();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante inicialização.");
                await GarantirTempoMinimoAsync(sw.ElapsedMilliseconds);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    StatusMessage = "Erro durante inicialização";
                    IsLoading = false;
                    _navigationService.NavigateToSetup();
                });
            }
        }
        #endregion

        #region Etapas do processo

        private async Task VerificarBancosAsync()
        {
            StatusMessage = "Verificando bancos de dados...";
            await MostrarStatusAsync("Verificando status dos bancos de dados...", 2000);

            await _dbStatus.RefreshStatusAsync();
            ProgressText = BuildStatusText();
            await Task.Delay(3000);
        }

        private async Task SincronizarDadosAsync()
        {
            StatusMessage = "Sincronizando dados...";
            await MostrarStatusAsync("Verificando sincronização de dados...", 2000);

            if (await _syncService.HasPendingChangesAsync())
            {
                await MostrarStatusAsync("Enviando alterações locais para o servidor...", 2000);
                await _syncService.SyncToSqlServerAsync();
            }
            else
            {
                await MostrarStatusAsync("Nenhuma alteração pendente. Sincronizando SQLite...", 2000);
                await _syncService.SyncToSqliteAsync();
            }
        }

        private async Task VerificarConfiguracoesAsync()
        {
            StatusMessage = "Verificando configuração...";
            await MostrarStatusAsync("Carregando configuração da empresa...", 2000);

            var isConfig = await _companyConfigService.IsConfiguredAsync();

            StatusMessage = "Verificando usuários Admin...";
            await MostrarStatusAsync("Verificando existência de usuários Administradores...", 2000);    
            var hasAdmins = await _userService.HasAdminsAsync();


            bool precisaSetup = !isConfig || !hasAdmins;

            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusMessage = precisaSetup
                    ? "Configuração inicial necessária"
                    : "Sistema configurado";

                ProgressText = precisaSetup
                    ? "Navegando para Setup..."
                    : "Navegando para Login...";
            });

            // Garante que o binding atualiza antes de navegar
            await Task.Delay(3000);

            if (precisaSetup)
                _navigationService.NavigateToSetup();
            else
                _navigationService.NavigateToLogin();

        }

        #endregion

        #region Utilidades

        private async Task MostrarStatusAsync(string message, int delay = 0)
        {
            ProgressText = message;
            if (delay > 0)
                await Task.Delay(delay);
        }

        private async Task GarantirTempoMinimoAsync(long elapsedMs)
        {
            var remaining = _minSplashMs - (int)elapsedMs;
            if (remaining > 0)
                await Task.Delay(remaining);
        }

        private void NavegarConformeStatus()
        {
            // (Já tratado dentro de VerificarConfiguracoesAsync, mas separado se quiser lógica adicional depois)
        }

        #endregion

        #region Logs

        private void ConfigureLogCapture()
        {
            var logSink = new LogEventSink(logEvent =>
            {
                Application.Current?.Dispatcher.InvokeAsync(() =>
                {
                    if (logEvent.Level < LogEventLevel.Information) return;

                    var message = $"[{logEvent.Timestamp:HH:mm:ss}] {logEvent.RenderMessage()}";
                    AtualizarBufferDeLog(message);
                });
            });
            // O sink será adicionado via configuração global do Serilog (Program.cs)
        }

        private void AtualizarBufferDeLog(string message)
        {
            _logBuffer.AppendLine(message);

            var lines = _logBuffer
                .ToString()
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .TakeLast(10);

            _logBuffer.Clear();
            _logBuffer.AppendLine(string.Join("\n", lines));
            ProgressText = _logBuffer.ToString().TrimEnd();
        }

        private class LogEventSink : ILogEventSink
        {
            private readonly Action<LogEvent> _onLogEvent;
            public LogEventSink(Action<LogEvent> onLogEvent) => _onLogEvent = onLogEvent;
            public void Emit(LogEvent logEvent) => _onLogEvent?.Invoke(logEvent);
        }

        #endregion

        #region Status
        private string BuildStatusText()
        {
            var sb = new StringBuilder();

            sb.AppendLine(_dbStatus.SqlServerConfigured
                ? $"SQL Server: {(_dbStatus.SqlServerConnected ? "Conectado" : "Falha")} | " +
                  $"Aplicadas: {_dbStatus.SqlServerAppliedMigrations} | Pendentes: {_dbStatus.SqlServerPendingMigrations}"
                : "SQL Server: não configurado");

            sb.AppendLine(_dbStatus.SqliteAvailable
                ? $"SQLite: OK ({_dbStatus.SqlitePath}) | " +
                  $"Aplicadas: {_dbStatus.SqliteAppliedMigrations} | Pendentes: {_dbStatus.SqlitePendingMigrations}"
                : "SQLite: falha");

            if (!string.IsNullOrWhiteSpace(_dbStatus.SqlServerError))
                sb.AppendLine($"Erro SQL: {_dbStatus.SqlServerError}");
            if (!string.IsNullOrWhiteSpace(_dbStatus.SqliteError))
                sb.AppendLine($"Erro SQLite: {_dbStatus.SqliteError}");

            return sb.ToString();
        }
        #endregion
    }
}
