using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Interface para gerenciamento de sessão do usuário logado.
    /// Mantém informações do usuário atual durante a sessão da aplicação.
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// Usuário atualmente logado na aplicação.
        /// </summary>
        UserDto? CurrentUser { get; }

        /// <summary>
        /// Verifica se existe um usuário logado.
        /// </summary>
        bool IsLoggedIn { get; }

        /// <summary>
        /// Data e hora do login atual.
        /// </summary>
        DateTime? LoginTime { get; }

        /// <summary>
        /// Endereço IP do login (se disponível).
        /// </summary>
        string? LoginIpAddress { get; }

        /// <summary>
        /// Inicia uma sessão para o usuário.
        /// </summary>
        /// <param name="user">Dados do usuário que está fazendo login</param>
        /// <param name="ipAddress">Endereço IP opcional</param>
        void StartSession(UserDto user, string? ipAddress = null);

        /// <summary>
        /// Encerra a sessão atual do usuário.
        /// </summary>
        void EndSession();

        /// <summary>
        /// Atualiza as informações do usuário na sessão atual.
        /// </summary>
        /// <param name="user">Dados atualizados do usuário</param>
        void UpdateCurrentUser(UserDto user);

        /// <summary>
        /// Verifica se o usuário tem um privilégio específico.
        /// </summary>
        /// <param name="privilegeCode">Código do privilégio</param>
        /// <returns>True se o usuário tem o privilégio</returns>
        Task<bool> HasPrivilegeAsync(string privilegeCode);

        /// <summary>
        /// Verifica se o usuário é administrador.
        /// </summary>
        bool IsAdministrator { get; }

        /// <summary>
        /// Evento disparado quando a sessão é iniciada.
        /// </summary>
        event EventHandler<UserDto>? SessionStarted;

        /// <summary>
        /// Evento disparado quando a sessão é encerrada.
        /// </summary>
        event EventHandler? SessionEnded;
    }
}
