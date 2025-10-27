using VendaFlex.Core.Utils;
using VendaFlex.Core.DTOs;
using VendaFlex.Data.Entities;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Serviço para gerenciamento de movimentações de estoque.
    /// </summary>
    public interface IStockMovementService
    {
        /// <summary>
        /// Obtém uma movimentação pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador da movimentação.</param>
        /// <returns>A movimentação correspondente ou <c>null</c> se não encontrada.</returns>
        Task<OperationResult<StockMovementDto>> GetByIdAsync(int id);

        /// <summary>
        /// Obtém todas as movimentações registradas.
        /// </summary>
        /// <returns>Lista de movimentações.</returns>
        Task<OperationResult<IEnumerable<StockMovementDto>>> GetAllAsync();

        /// <summary>
        /// Obtém movimentações relacionadas a um produto específico.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <returns>Lista de movimentações do produto.</returns>
        Task<OperationResult<IEnumerable<StockMovementDto>>> GetByProductIdAsync(int productId);

        /// <summary>
        /// Obtém movimentações realizadas por um determinado usuário.
        /// </summary>
        /// <param name="userId">Identificador do usuário.</param>
        /// <returns>Lista de movimentações do usuário.</returns>
        Task<OperationResult<IEnumerable<StockMovementDto>>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Obtém movimentações por tipo (entrada, saída, ajuste, etc.).
        /// </summary>
        /// <param name="type">Tipo da movimentação.</param>
        /// <returns>Lista de movimentações do tipo informado.</returns>
        Task<OperationResult<IEnumerable<StockMovementDto>>> GetByTypeAsync(StockMovementType type);

        /// <summary>
        /// Obtém movimentações dentro de um intervalo de datas.
        /// </summary>
        /// <param name="startDate">Data inicial do intervalo.</param>
        /// <param name="endDate">Data final do intervalo.</param>
        /// <returns>Lista de movimentações no intervalo informado.</returns>
        Task<OperationResult<IEnumerable<StockMovementDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Obtém movimentações de um produto dentro de um intervalo de datas.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <param name="startDate">Data inicial do intervalo.</param>
        /// <param name="endDate">Data final do intervalo.</param>
        /// <returns>Lista de movimentações do produto no intervalo informado.</returns>
        Task<OperationResult<IEnumerable<StockMovementDto>>> GetByProductAndDateRangeAsync(int productId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Adiciona uma nova movimentação de estoque.
        /// </summary>
        /// <param name="movement">Entidade de movimentação a ser adicionada.</param>
        /// <returns>A movimentação criada.</returns>
        Task<OperationResult<StockMovementDto>> AddAsync(StockMovementDto movement);

        /// <summary>
        /// Atualiza uma movimentação de estoque existente.
        /// </summary>
        /// <param name="movement">Entidade de movimentação com os dados atualizados.</param>
        /// <returns>A movimentação atualizada.</returns>
        Task<OperationResult<StockMovementDto>> UpdateAsync(StockMovementDto movement);

        /// <summary>
        /// Remove uma movimentação pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador da movimentação a ser removida.</param>
        /// <returns><c>true</c> se a remoção foi bem-sucedida; caso contrário, <c>false</c>.</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Obtém movimentações paginadas.
        /// </summary>
        /// <param name="pageNumber">Número da página (1-based).</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <returns>Lista de movimentações correspondentes à página solicitada.</returns>
        Task<OperationResult<IEnumerable<StockMovementDto>>> GetPaginatedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Obtém a contagem total de movimentações.
        /// </summary>
        /// <returns>Número total de registros de movimentação.</returns>
        Task<int> GetTotalCountAsync();

        /// <summary>
        /// Calcula o custo total de movimentações (ou custo total) relacionadas a um produto.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <returns>Custo total das movimentações do produto.</returns>
        Task<OperationResult<decimal>> GetTotalCostByProductAsync(int productId);
    }
}
