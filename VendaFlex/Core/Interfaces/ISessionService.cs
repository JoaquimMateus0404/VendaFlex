using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Interface para gerenciamento de sess�o do usu�rio logado.
    /// Mant�m informa��es do usu�rio atual durante a sess�o da aplica��o.
    /// </summary>
    public interface ISessionService
    {
        /// <summary>
        /// Usu�rio atualmente logado na aplica��o.
        /// </summary>
        UserDto? CurrentUser { get; }

        /// <summary>
        /// Verifica se existe um usu�rio logado.
        /// </summary>
        bool IsLoggedIn { get; }

        /// <summary>
        /// Data e hora do login atual.
        /// </summary>
        DateTime? LoginTime { get; }

        /// <summary>
        /// Endere�o IP do login (se dispon�vel).
        /// </summary>
        string? LoginIpAddress { get; }

        /// <summary>
        /// Inicia uma sess�o para o usu�rio.
        /// </summary>
        /// <param name="user">Dados do usu�rio que est� fazendo login</param>
        /// <param name="ipAddress">Endere�o IP opcional</param>
        void StartSession(UserDto user, string? ipAddress = null);

        /// <summary>
        /// Encerra a sess�o atual do usu�rio.
        /// </summary>
        void EndSession();

        /// <summary>
        /// Atualiza as informa��es do usu�rio na sess�o atual.
        /// </summary>
        /// <param name="user">Dados atualizados do usu�rio</param>
        void UpdateCurrentUser(UserDto user);

        /// <summary>
        /// Verifica se o usu�rio tem um privil�gio espec�fico.
        /// </summary>
        /// <param name="privilegeCode">C�digo do privil�gio</param>
        /// <returns>True se o usu�rio tem o privil�gio</returns>
        Task<bool> HasPrivilegeAsync(string privilegeCode);

        /// <summary>
        /// Verifica se o usu�rio � administrador.
        /// </summary>
        bool IsAdministrator { get; }

        /// <summary>
        /// Evento disparado quando a sess�o � iniciada.
        /// </summary>
        event EventHandler<UserDto>? SessionStarted;

        /// <summary>
        /// Evento disparado quando a sess�o � encerrada.
        /// </summary>
        event EventHandler? SessionEnded;
    }
}
