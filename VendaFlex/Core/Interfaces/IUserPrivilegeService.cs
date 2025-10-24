using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Interface para servi�os de gerenciamento de privil�gios de usu�rios.
    /// Define opera��es de concess�o e revoga��o de privil�gios.
    /// </summary>
    public interface IUserPrivilegeService
    {
        #region CRUD Operations

        /// <summary>
        /// Busca um privil�gio de usu�rio por ID.
        /// </summary>
        /// <param name="id">ID do privil�gio de usu�rio</param>
        /// <returns>Resultado com UserPrivilegeDto se encontrado</returns>
        Task<OperationResult<UserPrivilegeDto>> GetByIdAsync(int id);

        /// <summary>
        /// Retorna todos os privil�gios de usu�rios.
        /// </summary>
        /// <returns>Resultado com lista de privil�gios</returns>
        Task<OperationResult<IEnumerable<UserPrivilegeDto>>> GetAllAsync();

        #endregion

        #region Query Operations

        /// <summary>
        /// Retorna todos os privil�gios de um usu�rio espec�fico.
        /// </summary>
        /// <param name="userId">ID do usu�rio</param>
        /// <returns>Resultado com lista de privil�gios do usu�rio</returns>
        Task<OperationResult<IEnumerable<UserPrivilegeDto>>> GetByUserAsync(int userId);

        /// <summary>
        /// Retorna todos os usu�rios que possuem um privil�gio espec�fico.
        /// </summary>
        /// <param name="privilegeId">ID do privil�gio</param>
        /// <returns>Resultado com lista de privil�gios de usu�rios</returns>
        Task<OperationResult<IEnumerable<UserPrivilegeDto>>> GetByPrivilegeAsync(int privilegeId);

        /// <summary>
        /// Retorna privil�gios detalhados de um usu�rio (com nome do privil�gio).
        /// </summary>
        /// <param name="userId">ID do usu�rio</param>
        /// <returns>Resultado com lista de privil�gios com detalhes</returns>
        Task<OperationResult<IEnumerable<PrivilegeDto>>> GetUserPrivilegesDetailsAsync(int userId);

        #endregion

        #region Grant & Revoke Operations

        /// <summary>
        /// Concede um privil�gio a um usu�rio.
        /// </summary>
        /// <param name="dto">Dados do privil�gio a ser concedido</param>
        /// <returns>Resultado com UserPrivilegeDto criado</returns>
        Task<OperationResult<UserPrivilegeDto>> GrantAsync(UserPrivilegeDto dto);

        /// <summary>
        /// Concede m�ltiplos privil�gios a um usu�rio de uma vez.
        /// </summary>
        /// <param name="userId">ID do usu�rio</param>
        /// <param name="privilegeIds">IDs dos privil�gios</param>
        /// <param name="grantedByUserId">ID do usu�rio que est� concedendo</param>
        /// <returns>Resultado da opera��o</returns>
        Task<OperationResult> GrantMultipleAsync(int userId, IEnumerable<int> privilegeIds, int? grantedByUserId = null);

        /// <summary>
        /// Revoga um privil�gio de usu�rio espec�fico.
        /// </summary>
        /// <param name="userPrivilegeId">ID do privil�gio de usu�rio</param>
        /// <returns>Resultado da opera��o</returns>
        Task<OperationResult> RevokeAsync(int userPrivilegeId);

        /// <summary>
        /// Revoga um privil�gio espec�fico de um usu�rio.
        /// </summary>
        /// <param name="userId">ID do usu�rio</param>
        /// <param name="privilegeId">ID do privil�gio</param>
        /// <returns>Resultado da opera��o</returns>
        Task<OperationResult> RevokeByUserAndPrivilegeAsync(int userId, int privilegeId);

        /// <summary>
        /// Revoga todos os privil�gios de um usu�rio.
        /// </summary>
        /// <param name="userId">ID do usu�rio</param>
        /// <returns>Resultado da opera��o</returns>
        Task<OperationResult> RevokeAllFromUserAsync(int userId);

        #endregion

        #region Verification Operations

        /// <summary>
        /// Verifica se um usu�rio possui um privil�gio espec�fico.
        /// </summary>
        /// <param name="userId">ID do usu�rio</param>
        /// <param name="privilegeId">ID do privil�gio</param>
        /// <returns>True se o usu�rio possui o privil�gio</returns>
        Task<bool> UserHasPrivilegeAsync(int userId, int privilegeId);

        /// <summary>
        /// Verifica se um usu�rio possui um privil�gio por c�digo.
        /// </summary>
        /// <param name="userId">ID do usu�rio</param>
        /// <param name="privilegeCode">C�digo do privil�gio</param>
        /// <returns>True se o usu�rio possui o privil�gio</returns>
        Task<bool> UserHasPrivilegeByCodeAsync(int userId, string privilegeCode);

        /// <summary>
        /// Verifica se o privil�gio j� foi concedido ao usu�rio.
        /// </summary>
        /// <param name="userId">ID do usu�rio</param>
        /// <param name="privilegeId">ID do privil�gio</param>
        /// <returns>True se j� existe</returns>
        Task<bool> ExistsAsync(int userId, int privilegeId);

        #endregion

        #region Statistics

        /// <summary>
        /// Retorna a quantidade de privil�gios de um usu�rio.
        /// </summary>
        /// <param name="userId">ID do usu�rio</param>
        /// <returns>Quantidade de privil�gios</returns>
        Task<int> GetUserPrivilegeCountAsync(int userId);

        /// <summary>
        /// Retorna usu�rios com mais privil�gios.
        /// </summary>
        /// <param name="topCount">Quantidade de usu�rios a retornar</param>
        /// <returns>Resultado com lista de usu�rios e contagem</returns>
        Task<OperationResult<IEnumerable<(int UserId, int PrivilegeCount)>>> GetTopUsersWithMostPrivilegesAsync(int topCount = 10);

        #endregion
    }
}