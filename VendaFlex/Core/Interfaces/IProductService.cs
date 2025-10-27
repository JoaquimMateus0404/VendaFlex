using VendaFlex.Core.Utils;
using VendaFlex.Data.Entities;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Serviço responsável por operações relacionadas a produtos.
    /// Define métodos para consulta, validação, CRUD, buscas e consultas auxiliares.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Recupera um produto pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador do produto.</param>
        /// <returns>
        /// Um <see cref="OperationResult{Product}"/> contendo o produto quando a operação for bem-sucedida;
        /// em caso de falha (não encontrado, erro de negócio) contém informações de erro.
        /// </returns>
        Task<OperationResult<Product>> GetByIdAsync(int id);

        /// <summary>
        /// Recupera todos os produtos.
        /// </summary>
        /// <returns>
        /// Um <see cref="OperationResult{IEnumerable{Product}}"/> com a coleção de produtos ou informações de erro.
        /// </returns>
        Task<OperationResult<IEnumerable<Product>>> GetAllAsync();

        /// <summary>
        /// Recupera apenas os produtos com status ou sinalização de ativos.
        /// </summary>
        /// <returns>Lista de produtos ativos.</returns>
        Task<OperationResult<IEnumerable<Product>>> GetActiveAsync();

        /// <summary>
        /// Recupera produtos pertencentes a uma categoria específica.
        /// </summary>
        /// <param name="categoryId">Identificador da categoria.</param>
        /// <returns>Coleção de produtos da categoria.</returns>
        Task<OperationResult<IEnumerable<Product>>> GetByCategoryIdAsync(int categoryId);

        /// <summary>
        /// Recupera produtos fornecidos por um fornecedor específico.
        /// </summary>
        /// <param name="supplierId">Identificador do fornecedor.</param>
        /// <returns>Coleção de produtos do fornecedor.</returns>
        Task<OperationResult<IEnumerable<Product>>> GetBySupplierIdAsync(int supplierId);

        /// <summary>
        /// Recupera um produto pelo seu código de barras.
        /// </summary>
        /// <param name="barcode">Código de barras do produto.</param>
        /// <returns>
        /// Um <see cref="OperationResult{Product}"/> com o produto correspondente ou informações de erro se não encontrado.
        /// </returns>
        Task<OperationResult<Product>> GetByBarcodeAsync(string barcode);

        /// <summary>
        /// Recupera um produto pelo seu SKU.
        /// </summary>
        /// <param name="sku">SKU do produto.</param>
        /// <returns>
        /// Um <see cref="OperationResult{Product}"/> com o produto correspondente ou informações de erro se não encontrado.
        /// </returns>
        Task<OperationResult<Product>> GetBySKUAsync(string sku);

        /// <summary>
        /// Recupera um produto pelo seu código único.
        /// </summary>
        /// <param name="code">Código do produto.</param>
        /// <returns>
        /// Um <see cref="OperationResult{Product}"/> com o produto correspondente ou informações de erro se não encontrado.
        /// </returns>
        Task<OperationResult<Product>> GetByCodeAsync(string code);

        /// <summary>
        /// Recupera produtos com estoque baixo, conforme regras de controle de estoque (por exemplo, abaixo do mínimo).
        /// </summary>
        /// <returns>Coleção de produtos com baixo estoque.</returns>
        Task<OperationResult<IEnumerable<Product>>> GetLowStockAsync();

        /// <summary>
        /// Recupera produtos que estão sem estoque.
        /// </summary>
        /// <returns>Coleção de produtos sem estoque.</returns>
        Task<OperationResult<IEnumerable<Product>>> GetOutOfStockAsync();

        /// <summary>
        /// Recupera produtos marcados como em destaque (featured).
        /// </summary>
        /// <returns>Coleção de produtos em destaque.</returns>
        Task<OperationResult<IEnumerable<Product>>> GetFeaturedAsync();

        /// <summary>
        /// Recupera produtos que possuem controle de validade/expiração.
        /// </summary>
        /// <returns>Coleção de produtos que possuem data de expiração.</returns>
        Task<OperationResult<IEnumerable<Product>>> GetWithExpirationAsync();

        /// <summary>
        /// Verifica se um produto existe pelo identificador.
        /// </summary>
        /// <param name="id">Identificador do produto.</param>
        /// <returns>True se o produto existir; caso contrário, false.</returns>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Verifica se um código de barras já está em uso por outro produto.
        /// </summary>
        /// <param name="barcode">Código de barras a verificar.</param>
        /// <param name="excludeId">Id opcional para excluir da verificação (útil em atualizações).</param>
        /// <returns>True se o código de barras existir em outro produto; caso contrário, false.</returns>
        Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null);

        /// <summary>
        /// Verifica se um SKU já está em uso por outro produto.
        /// </summary>
        /// <param name="sku">SKU a verificar.</param>
        /// <param name="excludeId">Id opcional para excluir da verificação (útil em atualizações).</param>
        /// <returns>True se o SKU existir em outro produto; caso contrário, false.</returns>
        Task<bool> SKUExistsAsync(string sku, int? excludeId = null);

        /// <summary>
        /// Verifica se um código de produto já está em uso por outro produto.
        /// </summary>
        /// <param name="code">Código a verificar.</param>
        /// <param name="excludeId">Id opcional para excluir da verificação (útil em atualizações).</param>
        /// <returns>True se o código existir em outro produto; caso contrário, false.</returns>
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);

        /// <summary>
        /// Adiciona um novo produto ao sistema.
        /// </summary>
        /// <param name="product">Instância de <see cref="Product"/> com os dados do novo produto.</param>
        /// <returns>
        /// Um <see cref="OperationResult{Product}"/> com o produto criado (incluindo id) ou erros de validação.
        /// </returns>
        Task<OperationResult<Product>> AddAsync(Product product);

        /// <summary>
        /// Atualiza os dados de um produto existente.
        /// </summary>
        /// <param name="product">Instância de <see cref="Product"/> com os dados atualizados (deve conter o id).</param>
        /// <returns>
        /// Um <see cref="OperationResult{Product}"/> com o produto atualizado ou informações de erro.
        /// </returns>
        Task<OperationResult<Product>> UpdateAsync(Product product);

        /// <summary>
        /// Remove um produto pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador do produto a remover.</param>
        /// <returns>
        /// Um <see cref="OperationResult{bool}"/> onde o valor indica sucesso da exclusão e erros descrevem falhas.
        /// </returns>
        Task<OperationResult<bool>> DeleteAsync(int id);

        /// <summary>
        /// Realiza busca por produtos usando um termo (nome, código, SKU, descrição, etc.).
        /// </summary>
        /// <param name="searchTerm">Termo de busca parcial ou completo.</param>
        /// <returns>Coleção de produtos que correspondem ao termo.</returns>
        Task<OperationResult<IEnumerable<Product>>> SearchAsync(string searchTerm);

        /// <summary>
        /// Recupera uma página de produtos usando paginação baseada em número de página e tamanho.
        /// </summary>
        /// <param name="pageNumber">Número da página (1-based).</param>
        /// <param name="pageSize">Quantidade de itens por página.</param>
        /// <returns>Coleção de produtos correspondentes à página solicitada.</returns>
        Task<OperationResult<IEnumerable<Product>>> GetPaginatedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Recupera o número total de produtos existentes.
        /// </summary>
        /// <returns>Total de produtos cadastrados.</returns>
        Task<int> GetTotalCountAsync();

        /// <summary>
        /// Recupera produtos filtrados por status.
        /// </summary>
        /// <param name="status">Status do produto (<see cref="ProductStatus"/>).</param>
        /// <returns>Coleção de produtos com o status informado.</returns>
        Task<OperationResult<IEnumerable<Product>>> GetByStatusAsync(ProductStatus status);

        /// <summary>
        /// Recupera produtos cujo preço está dentro de um intervalo informado.
        /// </summary>
        /// <param name="minPrice">Preço mínimo (inclusive).</param>
        /// <param name="maxPrice">Preço máximo (inclusive).</param>
        /// <returns>Coleção de produtos dentro do intervalo de preço.</returns>
        Task<OperationResult<IEnumerable<Product>>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    }
}
