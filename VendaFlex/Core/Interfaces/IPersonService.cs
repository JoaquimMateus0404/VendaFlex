using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Entities;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Interface para servi�os de gerenciamento de pessoas (clientes, fornecedores, funcion�rios).
    /// Define opera��es CRUD e consultas espec�ficas por tipo.
    /// </summary>
    public interface IPersonService
    {
        #region CRUD Operations

        /// <summary>
        /// Busca uma pessoa por ID.
        /// </summary>
        /// <param name="id">ID da pessoa</param>
        /// <returns>Resultado com PersonDto se encontrado</returns>
        Task<OperationResult<PersonDto>> GetByIdAsync(int id);

        /// <summary>
        /// Retorna todas as pessoas cadastradas.
        /// </summary>
        /// <returns>Resultado com lista de pessoas</returns>
        Task<OperationResult<IEnumerable<PersonDto>>> GetAllAsync();

        /// <summary>
        /// Cria uma nova pessoa.
        /// </summary>
        /// <param name="dto">Dados da pessoa</param>
        /// <returns>Resultado com PersonDto criado</returns>
        Task<OperationResult<PersonDto>> CreateAsync(PersonDto dto);

        /// <summary>
        /// Atualiza dados de uma pessoa existente.
        /// </summary>
        /// <param name="dto">Dados atualizados</param>
        /// <returns>Resultado com PersonDto atualizado</returns>
        Task<OperationResult<PersonDto>> UpdateAsync(PersonDto dto);

        /// <summary>
        /// Remove uma pessoa do sistema.
        /// </summary>
        /// <param name="id">ID da pessoa</param>
        /// <returns>Resultado indicando sucesso ou falha</returns>
        Task<OperationResult> DeleteAsync(int id);

        #endregion

        #region Pagination

        /// <summary>
        /// Retorna pessoas paginadas.
        /// </summary>
        /// <param name="page">N�mero da p�gina (inicia em 1)</param>
        /// <param name="pageSize">Quantidade de itens por p�gina</param>
        /// <returns>Resultado com lista paginada</returns>
        Task<OperationResult<IEnumerable<PersonDto>>> GetPagedAsync(int page, int pageSize);

        /// <summary>
        /// Retorna o total de pessoas cadastradas.
        /// </summary>
        /// <returns>Quantidade total</returns>
        Task<int> GetTotalCountAsync();

        #endregion

        #region Search Operations

        /// <summary>
        /// Busca pessoas por termo (nome, email, documento).
        /// </summary>
        /// <param name="term">Termo de busca</param>
        /// <returns>Resultado com lista de pessoas encontradas</returns>
        Task<OperationResult<IEnumerable<PersonDto>>> SearchAsync(string term);

        /// <summary>
        /// Busca pessoa por email.
        /// </summary>
        /// <param name="email">Email da pessoa</param>
        /// <returns>Resultado com PersonDto se encontrado</returns>
        Task<OperationResult<PersonDto>> GetByEmailAsync(string email);

        /// <summary>
        /// Busca pessoa por documento fiscal (NIF, CNPJ, CPF).
        /// </summary>
        /// <param name="taxId">Documento fiscal</param>
        /// <returns>Resultado com PersonDto se encontrado</returns>
        Task<OperationResult<PersonDto>> GetByTaxIdAsync(string taxId);

        /// <summary>
        /// Busca pessoa por n�mero de identifica��o (BI, RG, Passaporte).
        /// </summary>
        /// <param name="identificationNumber">N�mero de identifica��o</param>
        /// <returns>Resultado com PersonDto se encontrado</returns>
        Task<OperationResult<PersonDto>> GetByIdentificationNumberAsync(string identificationNumber);

        #endregion

        #region Query by Type

        /// <summary>
        /// Retorna apenas clientes.
        /// </summary>
        /// <returns>Resultado com lista de clientes</returns>
        Task<OperationResult<IEnumerable<PersonDto>>> GetCustomersAsync();

        /// <summary>
        /// Retorna apenas fornecedores.
        /// </summary>
        /// <returns>Resultado com lista de fornecedores</returns>
        Task<OperationResult<IEnumerable<PersonDto>>> GetSuppliersAsync();

        /// <summary>
        /// Retorna apenas funcion�rios.
        /// </summary>
        /// <returns>Resultado com lista de funcion�rios</returns>
        Task<OperationResult<IEnumerable<PersonDto>>> GetEmployeesAsync();

        /// <summary>
        /// Retorna pessoas por tipo espec�fico.
        /// </summary>
        /// <param name="type">Tipo de pessoa</param>
        /// <returns>Resultado com lista de pessoas do tipo especificado</returns>
        Task<OperationResult<IEnumerable<PersonDto>>> GetByTypeAsync(PersonType type);

        #endregion

        #region Status Operations

        /// <summary>
        /// Retorna apenas pessoas ativas.
        /// </summary>
        /// <returns>Resultado com lista de pessoas ativas</returns>
        Task<OperationResult<IEnumerable<PersonDto>>> GetActiveAsync();

        /// <summary>
        /// Retorna apenas pessoas inativas.
        /// </summary>
        /// <returns>Resultado com lista de pessoas inativas</returns>
        Task<OperationResult<IEnumerable<PersonDto>>> GetInactiveAsync();

        /// <summary>
        /// Ativa uma pessoa.
        /// </summary>
        /// <param name="id">ID da pessoa</param>
        /// <returns>Resultado da opera��o</returns>
        Task<OperationResult> ActivateAsync(int id);

        /// <summary>
        /// Desativa uma pessoa.
        /// </summary>
        /// <param name="id">ID da pessoa</param>
        /// <returns>Resultado da opera��o</returns>
        Task<OperationResult> DeactivateAsync(int id);

        #endregion

        #region Validation Operations

        /// <summary>
        /// Verifica se um email j� est� em uso.
        /// </summary>
        /// <param name="email">Email a verificar</param>
        /// <param name="excludePersonId">ID da pessoa a excluir da verifica��o</param>
        /// <returns>True se o email j� existe</returns>
        Task<bool> EmailExistsAsync(string email, int? excludePersonId = null);

        /// <summary>
        /// Verifica se um documento fiscal j� est� em uso.
        /// </summary>
        /// <param name="taxId">Documento a verificar</param>
        /// <param name="excludePersonId">ID da pessoa a excluir da verifica��o</param>
        /// <returns>True se o documento j� existe</returns>
        Task<bool> TaxIdExistsAsync(string taxId, int? excludePersonId = null);

        #endregion

        #region Financial Operations (for Customers)

        /// <summary>
        /// Atualiza o saldo atual de um cliente.
        /// </summary>
        /// <param name="personId">ID do cliente</param>
        /// <param name="amount">Valor a adicionar/subtrair</param>
        /// <returns>Resultado da opera��o</returns>
        Task<OperationResult> UpdateBalanceAsync(int personId, decimal amount);

        /// <summary>
        /// Retorna clientes com saldo devedor.
        /// </summary>
        /// <returns>Resultado com lista de clientes com d�vidas</returns>
        Task<OperationResult<IEnumerable<PersonDto>>> GetCustomersWithDebtAsync();

        /// <summary>
        /// Retorna clientes pr�ximos ou acima do limite de cr�dito.
        /// </summary>
        /// <param name="percentageThreshold">Percentual do limite (ex: 90 para 90%)</param>
        /// <returns>Resultado com lista de clientes</returns>
        Task<OperationResult<IEnumerable<PersonDto>>> GetCustomersNearCreditLimitAsync(decimal percentageThreshold = 90);

        #endregion
    }
}