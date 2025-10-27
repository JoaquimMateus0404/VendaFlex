using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Servi�o para hist�rico de pre�os de produtos.
    /// </summary>
    public interface IPriceHistoryService
    {
        /// <summary>
        /// Obt�m um hist�rico de pre�o pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador do hist�rico.</param>
        /// <returns>Registro de hist�rico de pre�o ou <c>null</c> se n�o encontrado.</returns>
        Task<OperationResult<PriceHistoryDto>> GetByIdAsync(int id);

        /// <summary>
        /// Obt�m todos os registros de hist�rico de pre�os.
        /// </summary>
        /// <returns>Lista de hist�ricos de pre�os.</returns>
        Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetAllAsync();

        /// <summary>
        /// Obt�m o hist�rico de pre�os de um produto espec�fico.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <returns>Lista de hist�ricos do produto.</returns>
        Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetByProductIdAsync(int productId);

        /// <summary>
        /// Obt�m registros de hist�rico de pre�os dentro de um intervalo de datas.
        /// </summary>
        /// <param name="startDate">Data inicial do intervalo.</param>
        /// <param name="endDate">Data final do intervalo.</param>
        /// <returns>Lista de hist�ricos no intervalo informado.</returns>
        Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Obt�m o �ltimo registro de pre�o para um produto.
        /// </summary>
        /// <param name="productId">Identificador do produto.</param>
        /// <returns>�ltimo hist�rico de pre�o do produto.</returns>
        Task<OperationResult<PriceHistoryDto>> GetLatestByProductIdAsync(int productId);

        /// <summary>
        /// Adiciona um novo registro de hist�rico de pre�o.
        /// </summary>
        /// <param name="priceHistoryDto">Entidade de hist�rico a ser adicionada.</param>
        /// <returns>O registro criado.</returns>
        Task<OperationResult<PriceHistoryDto>> AddAsync(PriceHistoryDto priceHistoryDto);

        /// <summary>
        /// Atualiza um registro de hist�rico de pre�o existente.
        /// </summary>
        /// <param name="priceHistoryDto">Entidade de hist�rico com os dados atualizados.</param>
        /// <returns>O registro atualizado.</returns>
        Task<OperationResult<PriceHistoryDto>> UpdateAsync(PriceHistoryDto priceHistoryDto);

        /// <summary>
        /// Remove um registro de hist�rico pelo seu identificador.</summary>
        /// <param name="id">Identificador do registro a ser removido.</param>
        /// <returns><c>true</c> se a remo��o foi bem-sucedida; caso contr�rio, <c>false</c>.</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Obt�m registros de hist�rico de pre�os paginados.
        /// </summary>
        /// <param name="pageNumber">N�mero da p�gina (1-based).</param>
        /// <param name="pageSize">Tamanho da p�gina.</param>
        /// <returns>Lista de hist�ricos correspondentes � p�gina solicitada.</returns>
        Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetPaginatedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Obt�m a contagem total de registros de hist�rico de pre�os.
        /// </summary>
        /// <returns>N�mero total de registros.</returns>
        Task<int> GetTotalCountAsync();

        /// <summary>
        /// Obt�m os registros de hist�rico onde houve aumento de pre�o.
        /// </summary>
        /// <returns>Lista de hist�ricos com aumento de pre�o.</returns>
        Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetPriceIncreaseHistoryAsync();

        /// <summary>
        /// Obt�m os registros de hist�rico onde houve redu��o de pre�o.
        /// </summary>
        /// <returns>Lista de hist�ricos com redu��o de pre�o.</returns>
        Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetPriceDecreaseHistoryAsync();
    }
}
