using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Entities;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Servi�o respons�vel por opera��es relacionadas a produtos.
    /// Define m�todos para consulta, valida��o, CRUD, buscas e consultas auxiliares.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Recupera um produto pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador do produto.</param>
        /// <returns>
        /// Um <see cref="OperationResult{ProductDto}"/> contendo o produto quando a opera��o for bem-sucedida;
        /// em caso de falha (n�o encontrado, erro de neg�cio) cont�m informa��es de erro.
        /// </returns>
        Task<OperationResult<ProductDto>> GetByIdAsync(int id);

        /// <summary>
        /// Recupera todos os produtos.
        /// </summary>
        /// <returns>
        /// Um <see cref="OperationResult{IEnumerable{ProductDto}}"/> com a cole��o de produtos ou informa��es de erro.
        /// </returns>
        Task<OperationResult<IEnumerable<ProductDto>>> GetAllAsync();

        /// <summary>
        /// Recupera apenas os produtos com status ou sinaliza��o de ativos.
        /// </summary>
        /// <returns>Lista de produtos ativos.</returns>
        Task<OperationResult<IEnumerable<ProductDto>>> GetActiveAsync();

        /// <summary>
        /// Recupera produtos pertencentes a uma categoria espec�fica.
        /// </summary>
        /// <param name="categoryId">Identificador da categoria.</param>
        /// <returns>Cole��o de produtos da categoria.</returns>
        Task<OperationResult<IEnumerable<ProductDto>>> GetByCategoryIdAsync(int categoryId);

        /// <summary>
        /// Recupera produtos fornecidos por um fornecedor espec�fico.
        /// </summary>
        /// <param name="supplierId">Identificador do fornecedor.</param>
        /// <returns>Cole��o de produtos do fornecedor.</returns>
        Task<OperationResult<IEnumerable<ProductDto>>> GetBySupplierIdAsync(int supplierId);

        /// <summary>
        /// Recupera um produto pelo seu c�digo de barras.
        /// </summary>
        /// <param name="barcode">C�digo de barras do produto.</param>
        /// <returns>
        /// Um <see cref="OperationResult{ProductDto}"/> com o produto correspondente ou informa��es de erro se n�o encontrado.
        /// </returns>
        Task<OperationResult<ProductDto>> GetByBarcodeAsync(string barcode);

        /// <summary>
        /// Recupera um produto pelo seu SKU.
        /// </summary>
        /// <param name="sku">SKU do produto.</param>
        /// <returns>
        /// Um <see cref="OperationResult{ProductDto}"/> com o produto correspondente ou informa��es de erro se n�o encontrado.
        /// </returns>
        Task<OperationResult<ProductDto>> GetBySKUAsync(string sku);

        /// <summary>
        /// Recupera um produto pelo seu c�digo �nico.
        /// </summary>
        /// <param name="code">C�digo do produto.</param>
        /// <returns>
        /// Um <see cref="OperationResult{ProductDto}"/> com o produto correspondente ou informa��es de erro se n�o encontrado.
        /// </returns>
        Task<OperationResult<ProductDto>> GetByCodeAsync(string code);

        /// <summary>
        /// Recupera produtos com estoque baixo, conforme regras de controle de estoque (por exemplo, abaixo do m�nimo).
        /// </summary>
        /// <returns>Cole��o de produtos com baixo estoque.</returns>
        Task<OperationResult<IEnumerable<ProductDto>>> GetLowStockAsync();

        /// <summary>
        /// Recupera produtos que est�o sem estoque.
        /// </summary>
        /// <returns>Cole��o de produtos sem estoque.</returns>
        Task<OperationResult<IEnumerable<ProductDto>>> GetOutOfStockAsync();

        /// <summary>
        /// Recupera produtos marcados como em destaque (featured).
        /// </summary>
        /// <returns>Cole��o de produtos em destaque.</returns>
        Task<OperationResult<IEnumerable<ProductDto>>> GetFeaturedAsync();

        /// <summary>
        /// Recupera produtos que possuem controle de validade/expira��o.
        /// </summary>
        /// <returns>Cole��o de produtos que possuem data de expira��o.</returns>
        Task<OperationResult<IEnumerable<ProductDto>>> GetWithExpirationAsync();

        /// <summary>
        /// Verifica se um produto existe pelo identificador.
        /// </summary>
        /// <param name="id">Identificador do produto.</param>
        /// <returns>True se o produto existir; caso contr�rio, false.</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Verifica se um c�digo de barras j� est� em uso por outro produto.
        /// </summary>
        /// <param name="barcode">C�digo de barras a verificar.</param>
        /// <param name="excludeId">Id opcional para excluir da verifica��o (�til em atualiza��es).</param>
        /// <returns>True se o c�digo de barras existir em outro produto; caso contr�rio, false.</returns>
        Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null);

        /// <summary>
        /// Verifica se um SKU j� est� em uso por outro produto.
        /// </summary>
        /// <param name="sku">SKU a verificar.</param>
        /// <param name="excludeId">Id opcional para excluir da verifica��o (�til em atualiza��es).</param>
        /// <returns>True se o SKU existir em outro produto; caso contr�rio, false.</returns>
        Task<bool> SKUExistsAsync(string sku, int? excludeId = null);

        /// <summary>
        /// Verifica se um c�digo de produto j� est� em uso por outro produto.
        /// </summary>
        /// <param name="code">C�digo a verificar.</param>
        /// <param name="excludeId">Id opcional para excluir da verifica��o (�til em atualiza��es).</param>
        /// <returns>True se o c�digo existir em outro produto; caso contr�rio, false.</returns>
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// Adiciona um novo produto ao sistema.
        /// </summary>
        /// <param name="product">Inst�ncia de <see cref="ProductDto"/> com os dados do novo produto.</param>
        /// <returns>
        /// Um <see cref="OperationResult{ProductDto}"/> com o produto criado (incluindo id) ou erros de valida��o.
        /// </returns>
        Task<OperationResult<ProductDto>> AddAsync(ProductDto product);

        /// <summary>
        /// Atualiza os dados de um produto existente.
        /// </summary>
        /// <param name="product">Inst�ncia de <see cref="ProductDto"/> com os dados atualizados (deve conter o id).</param>
        /// <returns>
        /// Um <see cref="OperationResult{ProductDto}"/> com o produto atualizado ou informa��es de erro.
        /// </returns>
        Task<OperationResult<ProductDto>> UpdateAsync(ProductDto product);

        /// <summary>
        /// Remove um produto pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador do produto a remover.</param>
        /// <returns>
        /// Um <see cref="OperationResult{bool}"/> onde o valor indica sucesso da exclus�o e erros descrevem falhas.
        /// </returns>
        Task<OperationResult<bool>> DeleteAsync(int id);

        /// <summary>
        /// Realiza busca por produtos usando um termo (nome, c�digo, SKU, descri��o, etc.).
        /// </summary>
        /// <param name="searchTerm">Termo de busca parcial ou completo.</param>
        /// <returns>Cole��o de produtos que correspondem ao termo.</returns>
        Task<OperationResult<IEnumerable<ProductDto>>> SearchAsync(string searchTerm);

        /// <summary>
        /// Recupera uma p�gina de produtos usando pagina��o baseada em n�mero de p�gina e tamanho.
        /// </summary>
        /// <param name="pageNumber">N�mero da p�gina (1-based).</param>
        /// <param name="pageSize">Quantidade de itens por p�gina.</param>
        /// <returns>Cole��o de produtos correspondentes � p�gina solicitada.</returns>
        Task<OperationResult<IEnumerable<ProductDto>>> GetPaginatedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Recupera o n�mero total de produtos existentes.
        /// </summary>
        /// <returns>Total de produtos cadastrados.</returns>
        Task<int> GetTotalCountAsync();

        /// <summary>
        /// Recupera produtos filtrados por status.
        /// </summary>
        /// <param name="status">Status do produto (<see cref="ProductStatus"/>).</param>
        /// <returns>Cole��o de produtos com o status informado.</returns>
        Task<OperationResult<IEnumerable<ProductDto>>> GetByStatusAsync(ProductStatus status);

        /// <summary>
        /// Recupera produtos cujo pre�o est� dentro de um intervalo informado.
        /// </summary>
        /// <param name="minPrice">Pre�o m�nimo (inclusive).</param>
        /// <param name="maxPrice">Pre�o m�ximo (inclusive).</param>
        /// <returns>Cole��o de produtos dentro do intervalo de pre�o.</returns>
        Task<OperationResult<IEnumerable<ProductDto>>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    }
}
