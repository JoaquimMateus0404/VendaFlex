using Microsoft.Extensions.Logging;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Implementa��o do servi�o de gerenciamento de sess�o.
    /// Mant�m o estado do usu�rio logado durante a execu��o da aplica��o.
    /// </summary>
    public class SessionService : ISessionService
    {
        private readonly ILogger<SessionService> _logger;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly IServiceProvider _serviceProvider;
        private UserDto? _currentUser;
        private DateTime? _loginTime;
        private string? _loginIpAddress;

        public SessionService(
            ILogger<SessionService> logger,
            ICurrentUserContext currentUserContext,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _currentUserContext = currentUserContext;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public UserDto? CurrentUser => _currentUser;

        /// <inheritdoc/>
        public bool IsLoggedIn => _currentUser != null;

        /// <inheritdoc/>
        public DateTime? LoginTime => _loginTime;

        /// <inheritdoc/>
        public string? LoginIpAddress => _loginIpAddress;

        /// <inheritdoc/>
        public bool IsAdministrator
        {
            get
            {
                if (!IsLoggedIn)
                    return false;

                return _currentUser!.UserId == 1 && _currentUser.Status == LoginStatus.Active;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<UserDto>? SessionStarted;

        /// <inheritdoc/>
        public event EventHandler? SessionEnded;

        /// <inheritdoc/>
        public void StartSession(UserDto user, string? ipAddress = null)
        {
            if (user == null)
            {
                _logger.LogError("Tentativa de iniciar sess�o com usu�rio nulo");
                throw new ArgumentNullException(nameof(user));
            }

            _currentUser = user;
            _loginTime = DateTime.UtcNow;
            _loginIpAddress = ipAddress;

            // Atualiza contexto de usu�rio atual
            _currentUserContext.UserId = user.UserId;

            _logger.LogInformation(
                "Sess�o iniciada para usu�rio {Username} (ID: {UserId}) �s {LoginTime}",
                user.Username,
                user.UserId,
                _loginTime);

            // Dispara evento de sess�o iniciada
            SessionStarted?.Invoke(this, user);
        }

        /// <inheritdoc/>
        public void EndSession()
        {
            if (!IsLoggedIn)
            {
                _logger.LogWarning("Tentativa de encerrar sess�o sem usu�rio logado");
                return;
            }

            var username = _currentUser!.Username;
            var userId = _currentUser.UserId;
            var sessionDuration = _loginTime.HasValue
                ? DateTime.UtcNow - _loginTime.Value
                : TimeSpan.Zero;

            _currentUser = null;
            _loginTime = null;
            _loginIpAddress = null;

            // Limpa contexto de usu�rio atual
            _currentUserContext.UserId = null;

            _logger.LogInformation(
                "Sess�o encerrada para usu�rio {Username} (ID: {UserId}). Dura��o: {Duration}",
                username,
                userId,
                sessionDuration);

            // Dispara evento de sess�o encerrada
            SessionEnded?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void UpdateCurrentUser(UserDto user)
        {
            if (user == null)
            {
                _logger.LogError("Tentativa de atualizar usu�rio atual com valor nulo");
                throw new ArgumentNullException(nameof(user));
            }

            if (!IsLoggedIn)
            {
                _logger.LogWarning("Tentativa de atualizar usu�rio sem sess�o ativa");
                return;
            }

            if (_currentUser!.UserId != user.UserId)
            {
                _logger.LogWarning(
                    "Tentativa de atualizar sess�o com usu�rio diferente. Atual: {CurrentId}, Novo: {NewId}",
                    _currentUser.UserId,
                    user.UserId);
                return;
            }

            _currentUser = user;

            // Mant�m contexto sincronizado
            _currentUserContext.UserId = user.UserId;

            _logger.LogInformation(
                "Informa��es do usu�rio {Username} (ID: {UserId}) atualizadas na sess�o",
                user.Username,
                user.UserId);
        }

        /// <inheritdoc/>
        public async Task<bool> HasPrivilegeAsync(string privilegeCode)
        {
            if (!IsLoggedIn)
            {
                _logger.LogDebug("Verifica��o de privil�gio '{PrivilegeCode}' falhou: nenhum usu�rio logado", privilegeCode);
                return false;
            }

            if (IsAdministrator)
            {
                _logger.LogDebug(
                    "Usu�rio {Username} � administrador, privil�gio '{PrivilegeCode}' concedido automaticamente",
                    _currentUser!.Username,
                    privilegeCode);
                return true;
            }

            try
            {
                var privilegeService = _serviceProvider.GetService(typeof(IUserPrivilegeService)) as IUserPrivilegeService;
                if (privilegeService == null)
                {
                    _logger.LogWarning("IUserPrivilegeService n�o est� registrado");
                    return false;
                }

                var privilegesResult = await privilegeService.GetUserPrivilegesDetailsAsync(_currentUser!.UserId);

                if (!privilegesResult.Success || privilegesResult.Data == null)
                {
                    _logger.LogWarning(
                        "Falha ao buscar privil�gios do usu�rio {Username}: {Message}",
                        _currentUser.Username,
                        privilegesResult.Message);
                    return false;
                }

                var hasPrivilege = privilegesResult.Data.Any(p =>
                    p.Code?.Equals(privilegeCode, StringComparison.OrdinalIgnoreCase) == true);

                _logger.LogDebug(
                    "Verifica��o de privil�gio '{PrivilegeCode}' para usu�rio {Username}: {Result}",
                    privilegeCode,
                    _currentUser.Username,
                    hasPrivilege);

                return hasPrivilege;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao verificar privil�gio '{PrivilegeCode}' para usu�rio {Username}",
                    privilegeCode,
                    _currentUser!.Username);
                return false;
            }
        }
    }
}
