using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Interface para serviço de gestão de despesas e tipos de despesas.
    /// </summary>
    public interface IExpenseService
    {
        #region Expense CRUD

        /// <summary>
        /// Busca uma despesa por ID.
        /// </summary>
        Task<OperationResult<ExpenseDto>> GetByIdAsync(int id);

        /// <summary>
        /// Retorna todas as despesas.
        /// </summary>
        Task<OperationResult<IEnumerable<ExpenseDto>>> GetAllAsync();

        /// <summary>
        /// Cria uma nova despesa.
        /// </summary>
        Task<OperationResult<ExpenseDto>> CreateAsync(ExpenseDto dto);

        /// <summary>
        /// Atualiza uma despesa existente.
        /// </summary>
        Task<OperationResult<ExpenseDto>> UpdateAsync(ExpenseDto dto);

        /// <summary>
        /// Remove uma despesa.
        /// </summary>
        Task<OperationResult> DeleteAsync(int id);

        #endregion

        #region Expense Queries

        /// <summary>
        /// Retorna despesas por tipo.
        /// </summary>
        Task<OperationResult<IEnumerable<ExpenseDto>>> GetByExpenseTypeAsync(int expenseTypeId);

        /// <summary>
        /// Retorna despesas por usuário.
        /// </summary>
        Task<OperationResult<IEnumerable<ExpenseDto>>> GetByUserAsync(int userId);

        /// <summary>
        /// Retorna despesas pagas.
        /// </summary>
        Task<OperationResult<IEnumerable<ExpenseDto>>> GetPaidExpensesAsync();

        /// <summary>
        /// Retorna despesas não pagas (pendentes).
        /// </summary>
        Task<OperationResult<IEnumerable<ExpenseDto>>> GetUnpaidExpensesAsync();

        /// <summary>
        /// Retorna despesas por intervalo de datas.
        /// </summary>
        Task<OperationResult<IEnumerable<ExpenseDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Busca despesas por termo de pesquisa.
        /// </summary>
        Task<OperationResult<IEnumerable<ExpenseDto>>> SearchAsync(string searchTerm);

        /// <summary>
        /// Retorna o total de despesas.
        /// </summary>
        Task<OperationResult<decimal>> GetTotalAmountAsync();

        /// <summary>
        /// Retorna o total de despesas pagas.
        /// </summary>
        Task<OperationResult<decimal>> GetTotalPaidAmountAsync();

        /// <summary>
        /// Retorna o total de despesas não pagas.
        /// </summary>
        Task<OperationResult<decimal>> GetTotalUnpaidAmountAsync();

        /// <summary>
        /// Retorna o total de despesas por período.
        /// </summary>
        Task<OperationResult<decimal>> GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Retorna o total de despesas por tipo.
        /// </summary>
        Task<OperationResult<decimal>> GetTotalAmountByTypeAsync(int expenseTypeId);

        /// <summary>
        /// Retorna o número total de despesas.
        /// </summary>
        Task<OperationResult<int>> GetTotalCountAsync();

        /// <summary>
        /// Retorna despesas por referência.
        /// </summary>
        Task<OperationResult<IEnumerable<ExpenseDto>>> GetByReferenceAsync(string reference);

        /// <summary>
        /// Retorna uma lista paginada de despesas.
        /// </summary>
        Task<OperationResult<IEnumerable<ExpenseDto>>> GetPagedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Marca uma despesa como paga.
        /// </summary>
        Task<OperationResult<ExpenseDto>> MarkAsPaidAsync(int expenseId, DateTime? paidDate = null);

        /// <summary>
        /// Marca uma despesa como não paga.
        /// </summary>
        Task<OperationResult<ExpenseDto>> MarkAsUnpaidAsync(int expenseId);

        #endregion

        #region ExpenseType Operations

        /// <summary>
        /// Retorna todos os tipos de despesas.
        /// </summary>
        Task<OperationResult<IEnumerable<ExpenseTypeDto>>> GetExpenseTypesAsync();

        /// <summary>
        /// Retorna apenas tipos de despesas ativos.
        /// </summary>
        Task<OperationResult<IEnumerable<ExpenseTypeDto>>> GetActiveExpenseTypesAsync();

        /// <summary>
        /// Busca um tipo de despesa por ID.
        /// </summary>
        Task<OperationResult<ExpenseTypeDto>> GetExpenseTypeByIdAsync(int id);

        #endregion
    }
}
