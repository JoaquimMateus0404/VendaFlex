namespace VendaFlex.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface para gerenciamento seguro de credenciais usando Windows Credential Manager.
    /// </summary>
    public interface ICredentialManager
    {
        /// <summary>
        /// Salva o nome de usuário de forma segura.
        /// </summary>
        /// <param name="username">Nome de usuário a ser salvo</param>
        /// <returns>True se salvou com sucesso</returns>
        bool SaveRememberedUsername(string username);

        /// <summary>
        /// Recupera o nome de usuário salvo.
        /// </summary>
        /// <returns>Nome de usuário salvo ou null se não existir</returns>
        string? GetRememberedUsername();

        /// <summary>
        /// Remove o nome de usuário salvo.
        /// </summary>
        /// <returns>True se removeu com sucesso</returns>
        bool ClearRememberedUsername();

        /// <summary>
        /// Verifica se existe um nome de usuário salvo.
        /// </summary>
        /// <returns>True se existe credencial salva</returns>
        bool HasRememberedUsername();
    }
}
