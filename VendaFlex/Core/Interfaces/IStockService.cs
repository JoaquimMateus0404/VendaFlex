using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Servi�o para opera��es relacionadas ao estoque de produtos.
    /// </summary>
    public interface IStockService
    {
        /// <summary>
        /// Obt�m o registro de estoque para um determinado produto.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <returns>Registro de estoque ou <c>null</c> se n�o existir.</returns>
        Task<OperationResult<StockDto>> GetByProductIdAsync(int productId);

        /// <summary>
        /// Obt�m todos os registros de estoque.
        /// </summary>
        /// <returns>Lista de estoques.</returns>
        Task<OperationResult<IEnumerable<StockDto>>> GetAllAsync();

        /// <summary>
        /// Obt�m produtos com estoque baixo, conforme configura��o do produto.
        /// </summary>
        /// <returns>Lista de estoques com n�vel baixo.</returns>
        Task<OperationResult<IEnumerable<StockDto>>> GetLowStockAsync();

        /// <summary>
        /// Obt�m produtos sem estoque dispon�vel.
        /// </summary>
        /// <returns>Lista de estoques zerados.</returns>
        Task<OperationResult<IEnumerable<StockDto>>> GetOutOfStockAsync();

        /// <summary>
        /// Adiciona um novo registro de estoque.
        /// </summary>
        /// <param name="stock">Entidade de estoque a ser adicionada.</param>
        /// <returns>O registro de estoque criado.</returns>
        Task<OperationResult<StockDto>> AddAsync(StockDto stock);

        /// <summary>
        /// Atualiza um registro de estoque existente.
        /// </summary>
        /// <param name="stock">Entidade de estoque com os dados atualizados.</param>
        /// <returns>O registro de estoque atualizado.</returns>
        Task<OperationResult<StockDto>> UpdateAsync(StockDto stock);

        /// <summary>
        /// Remove o registro de estoque de um produto.
        /// </summary>
        /// <param name="productId">Identificador do produto cujo estoque ser� removido.</param>
        /// <returns><c>true</c> se a remo��o foi bem-sucedida; caso contr�rio, <c>false</c>.</returns>
        Task<bool> DeleteAsync(int productId);

        /// <summary>
        /// Verifica se existe registro de estoque para um produto.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <returns><c>true</c> se existir; caso contr�rio, <c>false</c>.</returns>
        Task<bool> ExistsAsync(int productId);

        /// <summary>
        /// Obt�m a quantidade dispon�vel (considerando reservas) para um produto.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <returns>Quantidade dispon�vel.</returns>
        Task<int> GetAvailableQuantityAsync(int productId);

        /// <summary>
        /// Atualiza a quantidade de um produto no estoque.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <param name="quantity">Quantidade a ser definida ou ajustada.</param>
        /// <param name="userId">Identificador do usu�rio que realizou a altera��o (opcional).</param>
        /// <returns><c>true</c> se a atualiza��o foi bem-sucedida; caso contr�rio, <c>false</c>.</returns>
        Task<bool> UpdateQuantityAsync(int productId, int quantity, int? userId = null);

        /// <summary>
        /// Reserva uma quantidade de um produto (reduz a quantidade dispon�vel, aumenta a reservada).
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <param name="quantity">Quantidade a reservar.</param>
        /// <returns><c>true</c> se a reserva foi efetuada; caso contr�rio, <c>false</c>.</returns>
        Task<bool> ReserveQuantityAsync(int productId, int quantity);

        /// <summary>
        /// Libera uma quantidade reservada de um produto.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <param name="quantity">Quantidade a liberar da reserva.</param>
        /// <returns><c>true</c> se a libera��o foi efetuada; caso contr�rio, <c>false</c>.</returns>
        Task<bool> ReleaseReservedQuantityAsync(int productId, int quantity);
    }
}
