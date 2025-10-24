using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Interface para serviços de gerenciamento de privilégios de usuários.
    /// Define operações de concessão e revogação de privilégios.
    /// </summary>
    public interface IUserPrivilegeService
    {
        #region CRUD Operations

        /// <summary>
        /// Busca um privilégio de usuário por ID.
        /// </summary>
        /// <param name="id">ID do privilégio de usuário</param>
        /// <returns>Resultado com UserPrivilegeDto se encontrado</returns>
        Task<OperationResult<UserPrivilegeDto>> GetByIdAsync(int id);

        /// <summary>
        /// Retorna todos os privilégios de usuários.
        /// </summary>
        /// <returns>Resultado com lista de privilégios</returns>
        Task<OperationResult<IEnumerable<UserPrivilegeDto>>> GetAllAsync();

        #endregion

        #region Query Operations

        /// <summary>
        /// Retorna todos os privilégios de um usuário específico.
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <returns>Resultado com lista de privilégios do usuário</returns>
        Task<OperationResult<IEnumerable<UserPrivilegeDto>>> GetByUserAsync(int userId);

        /// <summary>
        /// Retorna todos os usuários que possuem um privilégio específico.
        /// </summary>
        /// <param name="privilegeId">ID do privilégio</param>
        /// <returns>Resultado com lista de privilégios de usuários</returns>
        Task<OperationResult<IEnumerable<UserPrivilegeDto>>> GetByPrivilegeAsync(int privilegeId);

        /// <summary>
        /// Retorna privilégios detalhados de um usuário (com nome do privilégio).
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <returns>Resultado com lista de privilégios com detalhes</returns>
        Task<OperationResult<IEnumerable<PrivilegeDto>>> GetUserPrivilegesDetailsAsync(int userId);

        #endregion

        #region Grant & Revoke Operations

        /// <summary>
        /// Concede um privilégio a um usuário.
        /// </summary>
        /// <param name="dto">Dados do privilégio a ser concedido</param>
        /// <returns>Resultado com UserPrivilegeDto criado</returns>
        Task<OperationResult<UserPrivilegeDto>> GrantAsync(UserPrivilegeDto dto);

        /// <summary>
        /// Concede múltiplos privilégios a um usuário de uma vez.
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="privilegeIds">IDs dos privilégios</param>
        /// <param name="grantedByUserId">ID do usuário que está concedendo</param>
        /// <returns>Resultado da operação</returns>
        Task<OperationResult> GrantMultipleAsync(int userId, IEnumerable<int> privilegeIds, int? grantedByUserId = null);

        /// <summary>
        /// Revoga um privilégio de usuário específico.
        /// </summary>
        /// <param name="userPrivilegeId">ID do privilégio de usuário</param>
        /// <returns>Resultado da operação</returns>
        Task<OperationResult> RevokeAsync(int userPrivilegeId);

        /// <summary>
        /// Revoga um privilégio específico de um usuário.
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="privilegeId">ID do privilégio</param>
        /// <returns>Resultado da operação</returns>
        Task<OperationResult> RevokeByUserAndPrivilegeAsync(int userId, int privilegeId);

        /// <summary>
        /// Revoga todos os privilégios de um usuário.
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <returns>Resultado da operação</returns>
        Task<OperationResult> RevokeAllFromUserAsync(int userId);

        #endregion

        #region Verification Operations

        /// <summary>
        /// Verifica se um usuário possui um privilégio específico.
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="privilegeId">ID do privilégio</param>
        /// <returns>True se o usuário possui o privilégio</returns>
        Task<bool> UserHasPrivilegeAsync(int userId, int privilegeId);

        /// <summary>
        /// Verifica se um usuário possui um privilégio por código.
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="privilegeCode">Código do privilégio</param>
        /// <returns>True se o usuário possui o privilégio</returns>
        Task<bool> UserHasPrivilegeByCodeAsync(int userId, string privilegeCode);

        /// <summary>
        /// Verifica se o privilégio já foi concedido ao usuário.
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="privilegeId">ID do privilégio</param>
        /// <returns>True se já existe</returns>
        Task<bool> ExistsAsync(int userId, int privilegeId);

        #endregion

        #region Statistics

        /// <summary>
        /// Retorna a quantidade de privilégios de um usuário.
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <returns>Quantidade de privilégios</returns>
        Task<int> GetUserPrivilegeCountAsync(int userId);

        /// <summary>
        /// Retorna usuários com mais privilégios.
        /// </summary>
        /// <param name="topCount">Quantidade de usuários a retornar</param>
        /// <returns>Resultado com lista de usuários e contagem</returns>
        Task<OperationResult<IEnumerable<(int UserId, int PrivilegeCount)>>> GetTopUsersWithMostPrivilegesAsync(int topCount = 10);

        #endregion
    }
}