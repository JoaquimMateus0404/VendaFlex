using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Servi�o respons�vel por opera��es relacionadas a categorias de produtos.
    /// Define m�todos para CRUD, buscas, valida��es e consultas auxiliares.
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Recupera uma categoria pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador da categoria.</param>
        /// <returns>
        /// Um <see cref="OperationResult{CategoryDto}"/> contendo a categoria quando a opera��o for bem-sucedida;
        /// caso contr�rio, cont�m informa��es de erro.
        /// </returns>
        Task<OperationResult<CategoryDto>> GetByIdAsync(int id);

        /// <summary>
        /// Recupera todas as categorias.
        /// </summary>
        /// <returns>
        /// Um <see cref="OperationResult{IEnumerable{CategoryDto}}"/> com a lista de todas as categorias ou informa��es de erro.
        /// </returns>
        Task<OperationResult<IEnumerable<CategoryDto>>> GetAllAsync();

        /// <summary>
        /// Recupera apenas as categorias ativas.
        /// </summary>
        /// <returns>Lista de categorias que est�o marcadas como ativas.</returns>
        Task<OperationResult<IEnumerable<CategoryDto>>> GetActiveAsync();

        /// <summary>
        /// Recupera categorias que possuem um determinado identificador de categoria pai.
        /// </summary>
        /// <param name="parentId">Identificador da categoria pai. Pode ser nulo para categorias de n�vel raiz.</param>
        /// <returns>Lista de categorias filhas correspondentes.</returns>
        Task<IEnumerable<CategoryDto>> GetByParentIdAsync(int? parentId);

        /// <summary>
        /// Recupera uma categoria pelo seu c�digo �nico.
        /// </summary>
        /// <param name="code">C�digo da categoria.</param>
        /// <returns>A categoria correspondente ao c�digo ou nulo se n�o encontrada.</returns>
        Task<OperationResult<CategoryDto>> GetByCodeAsync(string code);

        /// <summary>
        /// Verifica se uma categoria existe pelo identificador.
        /// </summary>
        /// <param name="id">Identificador da categoria.</param>
        /// <returns>True se a categoria existir; caso contr�rio, false.</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Verifica se um c�digo de categoria j� existe, opcionalmente excluindo uma categoria espec�fica.
        /// </summary>
        /// <param name="code">C�digo a verificar.</param>
        /// <param name="excludeId">Identificador a ser exclu�do da verifica��o (�til em atualiza��es).</param>
        /// <returns>True se o c�digo existir em outra categoria; caso contr�rio, false.</returns>
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// Obt�m a quantidade de produtos associados a uma categoria.
        /// </summary>
        /// <param name="categoryId">Identificador da categoria.</param>
        /// <returns>N�mero de produtos na categoria.</returns>
        Task<int> GetProductCountAsync(int categoryId);

        /// <summary>
        /// Adiciona uma nova categoria ao sistema.
        /// </summary>
        /// <param name="category">DTO contendo os dados da nova categoria.</param>
        /// <returns>DTO da categoria criada, incluindo seu identificador.</returns>
        Task<OperationResult<CategoryDto>> AddAsync(CategoryDto category);

        /// <summary>
        /// Atualiza os dados de uma categoria existente.
        /// </summary>
        /// <param name="category">DTO com os dados atualizados da categoria (deve conter o identificador).</param>
        /// <returns>DTO da categoria atualizada.</returns>
        Task<OperationResult<CategoryDto>> UpdateAsync(CategoryDto category);

        /// <summary>
        /// Remove uma categoria pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador da categoria a ser removida.</param>
        /// <returns>True se a exclus�o foi bem-sucedida; caso contr�rio, false.</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Verifica se a categoria possui produtos vinculados.
        /// </summary>
        /// <param name="categoryId">Identificador da categoria.</param>
        /// <returns>True se existir ao menos um produto na categoria; caso contr�rio, false.</returns>
        Task<bool> HasProductsAsync(int categoryId);

        /// <summary>
        /// Verifica se a categoria possui subcategorias.
        /// </summary>
        /// <param name="categoryId">Identificador da categoria.</param>
        /// <returns>True se existir ao menos uma subcategoria; caso contr�rio, false.</returns>
        Task<bool> HasSubcategoriesAsync(int categoryId);

        /// <summary>
        /// Realiza busca por categorias com base em um termo (nome, c�digo, descri��o, etc.).
        /// </summary>
        /// <param name="searchTerm">Termo de busca parcial ou completo.</param>
        /// <returns>Lista de categorias que correspondem ao termo de busca.</returns>
        Task<OperationResult<IEnumerable<CategoryDto>>> SearchAsync(string searchTerm);

        /// <summary>
        /// Recupera uma p�gina de categorias usando pagina��o baseada em n�mero de p�gina e tamanho.
        /// </summary>
        /// <param name="pageNumber">N�mero da p�gina (1-based).</param>
        /// <param name="pageSize">Quantidade de itens por p�gina.</param>
        /// <returns>Lista de categorias correspondentes � p�gina solicitada.</returns>
        Task<OperationResult<IEnumerable<CategoryDto>>> GetPaginatedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Recupera o n�mero total de categorias existentes.
        /// </summary>
        /// <returns>Total de categorias cadastradas.</returns>
        Task<int> GetTotalCountAsync();
    }
}
