using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Interface para servi�os de gerenciamento de privil�gios do sistema.
    /// Define opera��es CRUD e consultas de privil�gios.
    /// </summary>
    public interface IPrivilegeService
    {
        #region CRUD Operations

        /// <summary>
        /// Busca um privil�gio por ID.
        /// </summary>
        /// <param name="id">ID do privil�gio</param>
        /// <returns>Resultado com PrivilegeDto se encontrado</returns>
        Task<OperationResult<PrivilegeDto>> GetByIdAsync(int id);

        /// <summary>
        /// Retorna todos os privil�gios cadastrados.
        /// </summary>
        /// <returns>Resultado com lista de privil�gios</returns>
        Task<OperationResult<IEnumerable<PrivilegeDto>>> GetAllAsync();

        /// <summary>
        /// Cria um novo privil�gio.
        /// </summary>
        /// <param name="dto">Dados do privil�gio</param>
        /// <returns>Resultado com PrivilegeDto criado</returns>
        Task<OperationResult<PrivilegeDto>> CreateAsync(PrivilegeDto dto);

        /// <summary>
        /// Atualiza um privil�gio existente.
        /// </summary>
        /// <param name="dto">Dados atualizados</param>
        /// <returns>Resultado com PrivilegeDto atualizado</returns>
        Task<OperationResult<PrivilegeDto>> UpdateAsync(PrivilegeDto dto);

        /// <summary>
        /// Remove um privil�gio do sistema.
        /// </summary>
        /// <param name="id">ID do privil�gio</param>
        /// <returns>Resultado indicando sucesso ou falha</returns>
        Task<OperationResult> DeleteAsync(int id);

        #endregion

        #region Query Operations

        /// <summary>
        /// Retorna apenas privil�gios ativos.
        /// </summary>
        /// <returns>Resultado com lista de privil�gios ativos</returns>
        Task<OperationResult<IEnumerable<PrivilegeDto>>> GetActiveAsync();

        /// <summary>
        /// Retorna apenas privil�gios inativos.
        /// </summary>
        /// <returns>Resultado com lista de privil�gios inativos</returns>
        Task<OperationResult<IEnumerable<PrivilegeDto>>> GetInactiveAsync();

        /// <summary>
        /// Busca privil�gios por termo (nome ou c�digo).
        /// </summary>
        /// <param name="term">Termo de busca</param>
        /// <returns>Resultado com lista de privil�gios encontrados</returns>
        Task<OperationResult<IEnumerable<PrivilegeDto>>> SearchAsync(string term);

        /// <summary>
        /// Busca privil�gio por c�digo.
        /// </summary>
        /// <param name="code">C�digo do privil�gio</param>
        /// <returns>Resultado com PrivilegeDto se encontrado</returns>
        Task<OperationResult<PrivilegeDto>> GetByCodeAsync(string code);

        #endregion

        #region Pagination

        /// <summary>
        /// Retorna privil�gios paginados.
        /// </summary>
        /// <param name="page">N�mero da p�gina (inicia em 1)</param>
        /// <param name="pageSize">Quantidade de itens por p�gina</param>
        /// <returns>Resultado com lista paginada</returns>
        Task<OperationResult<IEnumerable<PrivilegeDto>>> GetPagedAsync(int page, int pageSize);

        /// <summary>
        /// Retorna o total de privil�gios cadastrados.
        /// </summary>
        /// <returns>Quantidade total</returns>
        Task<int> GetTotalCountAsync();

        #endregion

        #region Status Operations

        /// <summary>
        /// Ativa um privil�gio.
        /// </summary>
        /// <param name="id">ID do privil�gio</param>
        /// <returns>Resultado da opera��o</returns>
        Task<OperationResult> ActivateAsync(int id);

        /// <summary>
        /// Desativa um privil�gio.
        /// </summary>
        /// <param name="id">ID do privil�gio</param>
        /// <returns>Resultado da opera��o</returns>
        Task<OperationResult> DeactivateAsync(int id);

        #endregion

        #region Validation Operations

        /// <summary>
        /// Verifica se um c�digo de privil�gio j� est� em uso.
        /// </summary>
        /// <param name="code">C�digo a verificar</param>
        /// <param name="excludePrivilegeId">ID do privil�gio a excluir da verifica��o</param>
        /// <returns>True se o c�digo j� existe</returns>
        Task<bool> CodeExistsAsync(string code, int? excludePrivilegeId = null);

        #endregion
    }
}