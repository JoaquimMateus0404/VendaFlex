using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Serviço para operações relacionadas ao estoque de produtos.
    /// </summary>
    public interface IStockService
    {
        /// <summary>
        /// Obtém o registro de estoque para um determinado produto.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <returns>Registro de estoque ou <c>null</c> se não existir.</returns>
        Task<OperationResult<StockDto>> GetByProductIdAsync(int productId);

        /// <summary>
        /// Obtém todos os registros de estoque.
        /// </summary>
        /// <returns>Lista de estoques.</returns>
        Task<OperationResult<IEnumerable<StockDto>>> GetAllAsync();

        /// <summary>
        /// Obtém produtos com estoque baixo, conforme configuração do produto.
        /// </summary>
        /// <returns>Lista de estoques com nível baixo.</returns>
        Task<OperationResult<IEnumerable<StockDto>>> GetLowStockAsync();

        /// <summary>
        /// Obtém produtos sem estoque disponível.
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
        /// <param name="productId">Identificador do produto cujo estoque será removido.</param>
        /// <returns><c>true</c> se a remoção foi bem-sucedida; caso contrário, <c>false</c>.</returns>
        Task<bool> DeleteAsync(int productId);

        /// <summary>
        /// Verifica se existe registro de estoque para um produto.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <returns><c>true</c> se existir; caso contrário, <c>false</c>.</returns>
        Task<bool> ExistsAsync(int productId);

        /// <summary>
        /// Obtém a quantidade disponível (considerando reservas) para um produto.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <returns>Quantidade disponível.</returns>
        Task<int> GetAvailableQuantityAsync(int productId);

        /// <summary>
        /// Atualiza a quantidade de um produto no estoque.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <param name="quantity">Quantidade a ser definida ou ajustada.</param>
        /// <param name="userId">Identificador do usuário que realizou a alteração (opcional).</param>
        /// <returns><c>true</c> se a atualização foi bem-sucedida; caso contrário, <c>false</c>.</returns>
        Task<bool> UpdateQuantityAsync(int productId, int quantity, int? userId = null);

        /// <summary>
        /// Atualiza a quantidade de um produto no estoque com nota personalizada.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <param name="quantity">Quantidade a ser definida ou ajustada.</param>
        /// <param name="userId">Identificador do usuário que realizou a alteração (opcional).</param>
        /// <param name="notes">Nota ou motivo da alteração.</param>
        /// <returns><c>true</c> se a atualização foi bem-sucedida; caso contrário, <c>false</c>.</returns>
        Task<bool> UpdateQuantityAsync(int productId, int quantity, int? userId, string? notes);

        /// <summary>
        /// Reserva uma quantidade de um produto (reduz a quantidade disponível, aumenta a reservada).
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <param name="quantity">Quantidade a reservar.</param>
        /// <returns><c>true</c> se a reserva foi efetuada; caso contrário, <c>false</c>.</returns>
        Task<bool> ReserveQuantityAsync(int productId, int quantity);

        /// <summary>
        /// Libera uma quantidade reservada de um produto.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <param name="quantity">Quantidade a liberar da reserva.</param>
        /// <returns><c>true</c> se a liberação foi efetuada; caso contrário, <c>false</c>.</returns>
        Task<bool> ReleaseReservedQuantityAsync(int productId, int quantity);
    }
}
