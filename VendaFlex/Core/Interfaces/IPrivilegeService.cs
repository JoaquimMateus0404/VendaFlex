using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Interface para serviços de gerenciamento de privilégios do sistema.
    /// Define operações CRUD e consultas de privilégios.
    /// </summary>
    public interface IPrivilegeService
    {
        #region CRUD Operations

        /// <summary>
        /// Busca um privilégio por ID.
        /// </summary>
        /// <param name="id">ID do privilégio</param>
        /// <returns>Resultado com PrivilegeDto se encontrado</returns>
        Task<OperationResult<PrivilegeDto>> GetByIdAsync(int id);

        /// <summary>
        /// Retorna todos os privilégios cadastrados.
        /// </summary>
        /// <returns>Resultado com lista de privilégios</returns>
        Task<OperationResult<IEnumerable<PrivilegeDto>>> GetAllAsync();

        /// <summary>
        /// Cria um novo privilégio.
        /// </summary>
        /// <param name="dto">Dados do privilégio</param>
        /// <returns>Resultado com PrivilegeDto criado</returns>
        Task<OperationResult<PrivilegeDto>> CreateAsync(PrivilegeDto dto);

        /// <summary>
        /// Atualiza um privilégio existente.
        /// </summary>
        /// <param name="dto">Dados atualizados</param>
        /// <returns>Resultado com PrivilegeDto atualizado</returns>
        Task<OperationResult<PrivilegeDto>> UpdateAsync(PrivilegeDto dto);

        /// <summary>
        /// Remove um privilégio do sistema.
        /// </summary>
        /// <param name="id">ID do privilégio</param>
        /// <returns>Resultado indicando sucesso ou falha</returns>
        Task<OperationResult> DeleteAsync(int id);

        #endregion

        #region Query Operations

        /// <summary>
        /// Retorna apenas privilégios ativos.
        /// </summary>
        /// <returns>Resultado com lista de privilégios ativos</returns>
        Task<OperationResult<IEnumerable<PrivilegeDto>>> GetActiveAsync();

        /// <summary>
        /// Retorna apenas privilégios inativos.
        /// </summary>
        /// <returns>Resultado com lista de privilégios inativos</returns>
        Task<OperationResult<IEnumerable<PrivilegeDto>>> GetInactiveAsync();

        /// <summary>
        /// Busca privilégios por termo (nome ou código).
        /// </summary>
        /// <param name="term">Termo de busca</param>
        /// <returns>Resultado com lista de privilégios encontrados</returns>
        Task<OperationResult<IEnumerable<PrivilegeDto>>> SearchAsync(string term);

        /// <summary>
        /// Busca privilégio por código.
        /// </summary>
        /// <param name="code">Código do privilégio</param>
        /// <returns>Resultado com PrivilegeDto se encontrado</returns>
        Task<OperationResult<PrivilegeDto>> GetByCodeAsync(string code);

        #endregion

        #region Pagination

        /// <summary>
        /// Retorna privilégios paginados.
        /// </summary>
        /// <param name="page">Número da página (inicia em 1)</param>
        /// <param name="pageSize">Quantidade de itens por página</param>
        /// <returns>Resultado com lista paginada</returns>
        Task<OperationResult<IEnumerable<PrivilegeDto>>> GetPagedAsync(int page, int pageSize);

        /// <summary>
        /// Retorna o total de privilégios cadastrados.
        /// </summary>
        /// <returns>Quantidade total</returns>
        Task<int> GetTotalCountAsync();

        #endregion

        #region Status Operations

        /// <summary>
        /// Ativa um privilégio.
        /// </summary>
        /// <param name="id">ID do privilégio</param>
        /// <returns>Resultado da operação</returns>
        Task<OperationResult> ActivateAsync(int id);

        /// <summary>
        /// Desativa um privilégio.
        /// </summary>
        /// <param name="id">ID do privilégio</param>
        /// <returns>Resultado da operação</returns>
        Task<OperationResult> DeactivateAsync(int id);

        #endregion

        #region Validation Operations

        /// <summary>
        /// Verifica se um código de privilégio já está em uso.
        /// </summary>
        /// <param name="code">Código a verificar</param>
        /// <param name="excludePrivilegeId">ID do privilégio a excluir da verificação</param>
        /// <returns>True se o código já existe</returns>
        Task<bool> CodeExistsAsync(string code, int? excludePrivilegeId = null);

        #endregion
    }
}