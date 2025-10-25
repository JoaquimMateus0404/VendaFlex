namespace VendaFlex.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface para gerenciamento seguro de credenciais usando Windows Credential Manager.
    /// </summary>
    public interface ICredentialManager
    {
        /// <summary>
        /// Salva o nome de usu�rio de forma segura.
        /// </summary>
        /// <param name="username">Nome de usu�rio a ser salvo</param>
        /// <returns>True se salvou com sucesso</returns>
        bool SaveRememberedUsername(string username);

        /// <summary>
        /// Recupera o nome de usu�rio salvo.
        /// </summary>
        /// <returns>Nome de usu�rio salvo ou null se n�o existir</returns>
        string? GetRememberedUsername();

        /// <summary>
        /// Remove o nome de usu�rio salvo.
        /// </summary>
        /// <returns>True se removeu com sucesso</returns>
        bool ClearRememberedUsername();

        /// <summary>
        /// Verifica se existe um nome de usu�rio salvo.
        /// </summary>
        /// <returns>True se existe credencial salva</returns>
        bool HasRememberedUsername();
    }
}
