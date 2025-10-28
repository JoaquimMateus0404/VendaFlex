using Microsoft.Extensions.Logging;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Implementação do serviço de gerenciamento de sessão.
    /// Mantém o estado do usuário logado durante a execução da aplicação.
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
                _logger.LogError("Tentativa de iniciar sessão com usuário nulo");
                throw new ArgumentNullException(nameof(user));
            }

            _currentUser = user;
            _loginTime = DateTime.UtcNow;
            _loginIpAddress = ipAddress;

            // Atualiza contexto de usuário atual
            _currentUserContext.UserId = user.UserId;

            _logger.LogInformation(
                "Sessão iniciada para usuário {Username} (ID: {UserId}) às {LoginTime}",
                user.Username,
                user.UserId,
                _loginTime);

            // Dispara evento de sessão iniciada
            SessionStarted?.Invoke(this, user);
        }

        /// <inheritdoc/>
        public void EndSession()
        {
            if (!IsLoggedIn)
            {
                _logger.LogWarning("Tentativa de encerrar sessão sem usuário logado");
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

            // Limpa contexto de usuário atual
            _currentUserContext.UserId = null;

            _logger.LogInformation(
                "Sessão encerrada para usuário {Username} (ID: {UserId}). Duração: {Duration}",
                username,
                userId,
                sessionDuration);

            // Dispara evento de sessão encerrada
            SessionEnded?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc/>
        public void UpdateCurrentUser(UserDto user)
        {
            if (user == null)
            {
                _logger.LogError("Tentativa de atualizar usuário atual com valor nulo");
                throw new ArgumentNullException(nameof(user));
            }

            if (!IsLoggedIn)
            {
                _logger.LogWarning("Tentativa de atualizar usuário sem sessão ativa");
                return;
            }

            if (_currentUser!.UserId != user.UserId)
            {
                _logger.LogWarning(
                    "Tentativa de atualizar sessão com usuário diferente. Atual: {CurrentId}, Novo: {NewId}",
                    _currentUser.UserId,
                    user.UserId);
                return;
            }

            _currentUser = user;

            // Mantém contexto sincronizado
            _currentUserContext.UserId = user.UserId;

            _logger.LogInformation(
                "Informações do usuário {Username} (ID: {UserId}) atualizadas na sessão",
                user.Username,
                user.UserId);
        }

        /// <inheritdoc/>
        public async Task<bool> HasPrivilegeAsync(string privilegeCode)
        {
            if (!IsLoggedIn)
            {
                _logger.LogDebug("Verificação de privilégio '{PrivilegeCode}' falhou: nenhum usuário logado", privilegeCode);
                return false;
            }

            if (IsAdministrator)
            {
                _logger.LogDebug(
                    "Usuário {Username} é administrador, privilégio '{PrivilegeCode}' concedido automaticamente",
                    _currentUser!.Username,
                    privilegeCode);
                return true;
            }

            try
            {
                var privilegeService = _serviceProvider.GetService(typeof(IUserPrivilegeService)) as IUserPrivilegeService;
                if (privilegeService == null)
                {
                    _logger.LogWarning("IUserPrivilegeService não está registrado");
                    return false;
                }

                var privilegesResult = await privilegeService.GetUserPrivilegesDetailsAsync(_currentUser!.UserId);

                if (!privilegesResult.Success || privilegesResult.Data == null)
                {
                    _logger.LogWarning(
                        "Falha ao buscar privilégios do usuário {Username}: {Message}",
                        _currentUser.Username,
                        privilegesResult.Message);
                    return false;
                }

                var hasPrivilege = privilegesResult.Data.Any(p =>
                    p.Code?.Equals(privilegeCode, StringComparison.OrdinalIgnoreCase) == true);

                _logger.LogDebug(
                    "Verificação de privilégio '{PrivilegeCode}' para usuário {Username}: {Result}",
                    privilegeCode,
                    _currentUser.Username,
                    hasPrivilege);

                return hasPrivilege;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao verificar privilégio '{PrivilegeCode}' para usuário {Username}",
                    privilegeCode,
                    _currentUser!.Username);
                return false;
            }
        }
    }
}
