using VendaFlex.Core.Utils;
using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Serviço para operações relacionadas a datas de validade (expirações) de produtos.
    /// </summary>
    public interface IExpirationService
    {
        /// <summary>
        /// Obtém uma expiração pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador da expiração.</param>
        /// <returns>A expiração correspondente ou <c>null</c> se não encontrada.</returns>
        Task<OperationResult<ExpirationDto>> GetByIdAsync(int id);

        /// <summary>
        /// Obtém todas as expirações registradas.
        /// </summary>
        /// <returns>Lista de expirações.</returns>
        Task<OperationResult<IEnumerable<ExpirationDto>>> GetAllAsync();

        /// <summary>
        /// Obtém todas as expirações associadas a um produto específico.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <returns>Lista de expirações do produto.</returns>
        Task<OperationResult<IEnumerable<ExpirationDto>>> GetByProductIdAsync(int productId);

        /// <summary>
        /// Obtém as expirações que já estão vencidas.
        /// </summary>
        /// <returns>Lista de expirações vencidas.</returns>
        Task<OperationResult<IEnumerable<ExpirationDto>>> GetExpiredAsync();

        /// <summary>
        /// Obtém as expirações que estão próximas da data de vencimento (aviso).
        /// </summary>
        /// <returns>Lista de expirações próximas do vencimento.</returns>
        Task<OperationResult<IEnumerable<ExpirationDto>>> GetNearExpirationAsync();

        /// <summary>
        /// Obtém as expirações que vencerão dentro do número de dias especificado.
        /// </summary>
        /// <param name="days">Número de dias para buscar expirações futuras.</param>
        /// <returns>Lista de expirações que vencerão no período.</returns>
        Task<OperationResult<IEnumerable<ExpirationDto>>> GetExpiringAsync(int days);

        /// <summary>
        /// Obtém expirações pelo número de lote.
        /// </summary>
        /// <param name="batchNumber">Número do lote.</param>
        /// <returns>Lista de expirações correspondentes ao lote.</returns>
        Task<OperationResult<IEnumerable<ExpirationDto>>> GetByBatchNumberAsync(string batchNumber);

        /// <summary>
        /// Obtém expirações dentro de um intervalo de datas.
        /// </summary>
        /// <param name="startDate">Data inicial do intervalo.</param>
        /// <param name="endDate">Data final do intervalo.</param>
        /// <returns>Lista de expirações no intervalo informado.</returns>
        Task<OperationResult<IEnumerable<ExpirationDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Adiciona uma nova expiração.
        /// </summary>
        /// <param name="expiration">Entidade de expiração a ser adicionada.</param>
        /// <returns>A expiração criada.</returns>
        Task<OperationResult<ExpirationDto>> AddAsync(ExpirationDto expiration);

        /// <summary>
        /// Atualiza uma expiração existente.
        /// </summary>
        /// <param name="expiration">Entidade de expiração com os dados atualizados.</param>
        /// <returns>A expiração atualizada.</returns>
        Task<OperationResult<ExpirationDto>> UpdateAsync(ExpirationDto expiration);

        /// <summary>
        /// Remove uma expiração pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador da expiração a ser removida.</param>
        /// <returns><c>true</c> se a remoção foi bem-sucedida; caso contrário, <c>false</c>.</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Obtém expirações paginadas.
        /// </summary>
        /// <param name="pageNumber">Número da página (1-based).</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <returns>Lista de expirações correspondentes à página solicitada.</returns>
        Task<OperationResult<IEnumerable<ExpirationDto>>> GetPaginatedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Obtém a contagem total de expirações.
        /// </summary>
        /// <returns>Número total de registros de expiração.</returns>
        Task<int> GetTotalCountAsync();

        /// <summary>
        /// Obtém a quantidade total de itens expirados para um produto específico.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <returns>Quantidade de itens expirados do produto.</returns>
        Task<int> GetExpiredQuantityByProductAsync(int productId);
    }
}
