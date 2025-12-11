using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço para despesas e seus tipos.
    /// </summary>
    public class ExpenseService : IExpenseService
    {
        private readonly ExpenseRepository _expenseRepository;
        private readonly ExpenseTypeRepository _expenseTypeRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<ExpenseDto> _expenseValidator;

        public ExpenseService(
            ExpenseRepository expenseRepository,
            ExpenseTypeRepository expenseTypeRepository,
            IMapper mapper,
            IValidator<ExpenseDto> expenseValidator)
        {
            _expenseRepository = expenseRepository ?? throw new ArgumentNullException(nameof(expenseRepository));
            _expenseTypeRepository = expenseTypeRepository ?? throw new ArgumentNullException(nameof(expenseTypeRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _expenseValidator = expenseValidator ?? throw new ArgumentNullException(nameof(expenseValidator));
        }

        #region Expense CRUD

        /// <summary>
        /// Busca uma despesa por ID.
        /// </summary>
        public async Task<OperationResult<ExpenseDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<ExpenseDto>.CreateFailure("ID inválido.");

                var entity = await _expenseRepository.GetByIdAsync(id);
                if (entity == null)
                    return OperationResult<ExpenseDto>.CreateFailure("Despesa não encontrada.");

                var dto = _mapper.Map<ExpenseDto>(entity);
                return OperationResult<ExpenseDto>.CreateSuccess(dto, "Despesa encontrada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<ExpenseDto>.CreateFailure(
                    "Erro ao buscar despesa.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna todas as despesas.
        /// </summary>
        public async Task<OperationResult<IEnumerable<ExpenseDto>>> GetAllAsync()
        {
            try
            {
                var entities = await _expenseRepository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<ExpenseDto>>(entities);
                return OperationResult<IEnumerable<ExpenseDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} despesa(s) encontrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure(
                    "Erro ao listar despesas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Cria uma nova despesa.
        /// </summary>
        public async Task<OperationResult<ExpenseDto>> CreateAsync(ExpenseDto dto)
        {
            try
            {
                if (dto == null)
                    return OperationResult<ExpenseDto>.CreateFailure("Despesa é obrigatória.");

                // Validação
                var validationResult = await _expenseValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return OperationResult<ExpenseDto>.CreateFailure(
                        "Dados inválidos.",
                        validationResult.Errors.Select(e => e.ErrorMessage));
                }

                // Verificar se o tipo de despesa existe e está ativo
                var expenseType = await _expenseTypeRepository.GetByIdAsync(dto.ExpenseTypeId);
                if (expenseType == null)
                    return OperationResult<ExpenseDto>.CreateFailure("Tipo de despesa não encontrado.");

                if (!expenseType.IsActive)
                    return OperationResult<ExpenseDto>.CreateFailure("Tipo de despesa está inativo.");

                // Mapear e criar
                var entity = _mapper.Map<Expense>(dto);
                var created = await _expenseRepository.AddAsync(entity);
                var resultDto = _mapper.Map<ExpenseDto>(created);

                return OperationResult<ExpenseDto>.CreateSuccess(resultDto, "Despesa criada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<ExpenseDto>.CreateFailure(
                    "Erro ao criar despesa.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Atualiza uma despesa existente.
        /// </summary>
        public async Task<OperationResult<ExpenseDto>> UpdateAsync(ExpenseDto dto)
        {
            try
            {
                if (dto == null)
                    return OperationResult<ExpenseDto>.CreateFailure("Despesa é obrigatória.");

                if (dto.ExpenseId <= 0)
                    return OperationResult<ExpenseDto>.CreateFailure("ID da despesa é inválido.");

                // Validação
                var validationResult = await _expenseValidator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return OperationResult<ExpenseDto>.CreateFailure(
                        "Dados inválidos.",
                        validationResult.Errors.Select(e => e.ErrorMessage));
                }

                // Verificar se a despesa existe
                var exists = await _expenseRepository.ExistsAsync(dto.ExpenseId);
                if (!exists)
                    return OperationResult<ExpenseDto>.CreateFailure("Despesa não encontrada.");

                // Verificar se o tipo de despesa existe
                var expenseType = await _expenseTypeRepository.GetByIdAsync(dto.ExpenseTypeId);
                if (expenseType == null)
                    return OperationResult<ExpenseDto>.CreateFailure("Tipo de despesa não encontrado.");

                // Mapear e atualizar
                var entity = _mapper.Map<Expense>(dto);
                var updated = await _expenseRepository.UpdateAsync(entity);
                var resultDto = _mapper.Map<ExpenseDto>(updated);

                return OperationResult<ExpenseDto>.CreateSuccess(resultDto, "Despesa atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<ExpenseDto>.CreateFailure(
                    "Erro ao atualizar despesa.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Remove uma despesa.
        /// </summary>
        public async Task<OperationResult> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult.CreateFailure("ID inválido.");

                var exists = await _expenseRepository.ExistsAsync(id);
                if (!exists)
                    return OperationResult.CreateFailure("Despesa não encontrada.");

                var deleted = await _expenseRepository.DeleteAsync(id);
                if (!deleted)
                    return OperationResult.CreateFailure("Não foi possível remover a despesa.");

                return OperationResult.CreateSuccess("Despesa removida com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure(
                    "Erro ao remover despesa.",
                    new[] { ex.Message });
            }
        }

        #endregion

        #region Expense Queries

        /// <summary>
        /// Retorna despesas por tipo.
        /// </summary>
        public async Task<OperationResult<IEnumerable<ExpenseDto>>> GetByExpenseTypeAsync(int expenseTypeId)
        {
            try
            {
                if (expenseTypeId <= 0)
                    return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure("ID do tipo de despesa inválido.");

                var entities = await _expenseRepository.GetByExpenseTypeAsync(expenseTypeId);
                var dtos = _mapper.Map<IEnumerable<ExpenseDto>>(entities);
                return OperationResult<IEnumerable<ExpenseDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} despesa(s) encontrada(s) para o tipo especificado.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure(
                    "Erro ao buscar despesas por tipo.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna despesas por usuário.
        /// </summary>
        public async Task<OperationResult<IEnumerable<ExpenseDto>>> GetByUserAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure("ID do usuário inválido.");

                var entities = await _expenseRepository.GetByUserAsync(userId);
                var dtos = _mapper.Map<IEnumerable<ExpenseDto>>(entities);
                return OperationResult<IEnumerable<ExpenseDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} despesa(s) encontrada(s) para o usuário.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure(
                    "Erro ao buscar despesas por usuário.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna despesas pagas.
        /// </summary>
        public async Task<OperationResult<IEnumerable<ExpenseDto>>> GetPaidExpensesAsync()
        {
            try
            {
                var entities = await _expenseRepository.GetPaidAsync();
                var dtos = _mapper.Map<IEnumerable<ExpenseDto>>(entities);
                return OperationResult<IEnumerable<ExpenseDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} despesa(s) paga(s) encontrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure(
                    "Erro ao buscar despesas pagas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna despesas não pagas (pendentes).
        /// </summary>
        public async Task<OperationResult<IEnumerable<ExpenseDto>>> GetUnpaidExpensesAsync()
        {
            try
            {
                var entities = await _expenseRepository.GetUnpaidAsync();
                var dtos = _mapper.Map<IEnumerable<ExpenseDto>>(entities);
                return OperationResult<IEnumerable<ExpenseDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} despesa(s) pendente(s) encontrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure(
                    "Erro ao buscar despesas pendentes.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna despesas por intervalo de datas.
        /// </summary>
        public async Task<OperationResult<IEnumerable<ExpenseDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                    return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure("Data inicial não pode ser maior que data final.");

                var entities = await _expenseRepository.GetByDateRangeAsync(startDate, endDate);
                var dtos = _mapper.Map<IEnumerable<ExpenseDto>>(entities);
                return OperationResult<IEnumerable<ExpenseDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} despesa(s) encontrada(s) no período.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure(
                    "Erro ao buscar despesas por período.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Busca despesas por termo de pesquisa.
        /// </summary>
        public async Task<OperationResult<IEnumerable<ExpenseDto>>> SearchAsync(string searchTerm)
        {
            try
            {
                var entities = await _expenseRepository.SearchAsync(searchTerm);
                var dtos = _mapper.Map<IEnumerable<ExpenseDto>>(entities);
                return OperationResult<IEnumerable<ExpenseDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} despesa(s) encontrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure(
                    "Erro ao buscar despesas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna o total de despesas.
        /// </summary>
        public async Task<OperationResult<decimal>> GetTotalAmountAsync()
        {
            try
            {
                var total = await _expenseRepository.GetTotalAmountAsync();
                return OperationResult<decimal>.CreateSuccess(total, $"Total de despesas: {total:C}");
            }
            catch (Exception ex)
            {
                return OperationResult<decimal>.CreateFailure(
                    "Erro ao calcular total de despesas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna o total de despesas pagas.
        /// </summary>
        public async Task<OperationResult<decimal>> GetTotalPaidAmountAsync()
        {
            try
            {
                var total = await _expenseRepository.GetTotalPaidAmountAsync();
                return OperationResult<decimal>.CreateSuccess(total, $"Total de despesas pagas: {total:C}");
            }
            catch (Exception ex)
            {
                return OperationResult<decimal>.CreateFailure(
                    "Erro ao calcular total de despesas pagas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna o total de despesas não pagas.
        /// </summary>
        public async Task<OperationResult<decimal>> GetTotalUnpaidAmountAsync()
        {
            try
            {
                var total = await _expenseRepository.GetTotalUnpaidAmountAsync();
                return OperationResult<decimal>.CreateSuccess(total, $"Total de despesas pendentes: {total:C}");
            }
            catch (Exception ex)
            {
                return OperationResult<decimal>.CreateFailure(
                    "Erro ao calcular total de despesas pendentes.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna o total de despesas por período.
        /// </summary>
        public async Task<OperationResult<decimal>> GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                    return OperationResult<decimal>.CreateFailure("Data inicial não pode ser maior que data final.");

                var total = await _expenseRepository.GetTotalAmountByDateRangeAsync(startDate, endDate);
                return OperationResult<decimal>.CreateSuccess(
                    total,
                    $"Total de despesas no período: {total:C}");
            }
            catch (Exception ex)
            {
                return OperationResult<decimal>.CreateFailure(
                    "Erro ao calcular total de despesas por período.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna o total de despesas por tipo.
        /// </summary>
        public async Task<OperationResult<decimal>> GetTotalAmountByTypeAsync(int expenseTypeId)
        {
            try
            {
                if (expenseTypeId <= 0)
                    return OperationResult<decimal>.CreateFailure("ID do tipo de despesa inválido.");

                var total = await _expenseRepository.GetTotalAmountByTypeAsync(expenseTypeId);
                return OperationResult<decimal>.CreateSuccess(
                    total,
                    $"Total de despesas do tipo: {total:C}");
            }
            catch (Exception ex)
            {
                return OperationResult<decimal>.CreateFailure(
                    "Erro ao calcular total de despesas por tipo.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna o número total de despesas.
        /// </summary>
        public async Task<OperationResult<int>> GetTotalCountAsync()
        {
            try
            {
                var count = await _expenseRepository.GetTotalCountAsync();
                return OperationResult<int>.CreateSuccess(count, $"Total de {count} despesa(s) cadastrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<int>.CreateFailure(
                    "Erro ao contar despesas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna despesas por referência.
        /// </summary>
        public async Task<OperationResult<IEnumerable<ExpenseDto>>> GetByReferenceAsync(string reference)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(reference))
                    return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure("Referência é obrigatória.");

                var entities = await _expenseRepository.GetByReferenceAsync(reference);
                var dtos = _mapper.Map<IEnumerable<ExpenseDto>>(entities);
                return OperationResult<IEnumerable<ExpenseDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} despesa(s) encontrada(s) com a referência.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure(
                    "Erro ao buscar despesas por referência.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna uma lista paginada de despesas.
        /// </summary>
        public async Task<OperationResult<IEnumerable<ExpenseDto>>> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1)
                    return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure("Número da página deve ser maior ou igual a 1.");

                if (pageSize < 1)
                    return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure("Tamanho da página deve ser maior que 0.");

                var entities = await _expenseRepository.GetPagedAsync(pageNumber, pageSize);
                var dtos = _mapper.Map<IEnumerable<ExpenseDto>>(entities);
                return OperationResult<IEnumerable<ExpenseDto>>.CreateSuccess(
                    dtos,
                    $"Página {pageNumber} com {dtos.Count()} despesa(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpenseDto>>.CreateFailure(
                    "Erro ao buscar despesas paginadas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Marca uma despesa como paga.
        /// </summary>
        public async Task<OperationResult<ExpenseDto>> MarkAsPaidAsync(int expenseId, DateTime? paidDate = null)
        {
            try
            {
                if (expenseId <= 0)
                    return OperationResult<ExpenseDto>.CreateFailure("ID da despesa inválido.");

                var expense = await _expenseRepository.GetByIdAsync(expenseId);
                if (expense == null)
                    return OperationResult<ExpenseDto>.CreateFailure("Despesa não encontrada.");

                if (expense.IsPaid)
                    return OperationResult<ExpenseDto>.CreateFailure("Despesa já está marcada como paga.");

                expense.IsPaid = true;
                expense.PaidDate = paidDate ?? DateTime.UtcNow;

                var updated = await _expenseRepository.UpdateAsync(expense);
                var dto = _mapper.Map<ExpenseDto>(updated);

                return OperationResult<ExpenseDto>.CreateSuccess(dto, "Despesa marcada como paga com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<ExpenseDto>.CreateFailure(
                    "Erro ao marcar despesa como paga.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Marca uma despesa como não paga.
        /// </summary>
        public async Task<OperationResult<ExpenseDto>> MarkAsUnpaidAsync(int expenseId)
        {
            try
            {
                if (expenseId <= 0)
                    return OperationResult<ExpenseDto>.CreateFailure("ID da despesa inválido.");

                var expense = await _expenseRepository.GetByIdAsync(expenseId);
                if (expense == null)
                    return OperationResult<ExpenseDto>.CreateFailure("Despesa não encontrada.");

                if (!expense.IsPaid)
                    return OperationResult<ExpenseDto>.CreateFailure("Despesa já está marcada como não paga.");

                expense.IsPaid = false;
                expense.PaidDate = null;

                var updated = await _expenseRepository.UpdateAsync(expense);
                var dto = _mapper.Map<ExpenseDto>(updated);

                return OperationResult<ExpenseDto>.CreateSuccess(dto, "Despesa marcada como não paga com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<ExpenseDto>.CreateFailure(
                    "Erro ao marcar despesa como não paga.",
                    new[] { ex.Message });
            }
        }

        #endregion

        #region ExpenseType Operations

        /// <summary>
        /// Retorna todos os tipos de despesas.
        /// </summary>
        public async Task<OperationResult<IEnumerable<ExpenseTypeDto>>> GetExpenseTypesAsync()
        {
            try
            {
                var entities = await _expenseTypeRepository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<ExpenseTypeDto>>(entities);
                return OperationResult<IEnumerable<ExpenseTypeDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} tipo(s) de despesa encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpenseTypeDto>>.CreateFailure(
                    "Erro ao buscar tipos de despesas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna apenas tipos de despesas ativos.
        /// </summary>
        public async Task<OperationResult<IEnumerable<ExpenseTypeDto>>> GetActiveExpenseTypesAsync()
        {
            try
            {
                var entities = await _expenseTypeRepository.GetActiveAsync();
                var dtos = _mapper.Map<IEnumerable<ExpenseTypeDto>>(entities);
                return OperationResult<IEnumerable<ExpenseTypeDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} tipo(s) de despesa ativo(s) encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpenseTypeDto>>.CreateFailure(
                    "Erro ao buscar tipos de despesas ativos.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Busca um tipo de despesa por ID.
        /// </summary>
        public async Task<OperationResult<ExpenseTypeDto>> GetExpenseTypeByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<ExpenseTypeDto>.CreateFailure("ID inválido.");

                var entity = await _expenseTypeRepository.GetByIdAsync(id);
                if (entity == null)
                    return OperationResult<ExpenseTypeDto>.CreateFailure("Tipo de despesa não encontrado.");

                var dto = _mapper.Map<ExpenseTypeDto>(entity);
                return OperationResult<ExpenseTypeDto>.CreateSuccess(dto, "Tipo de despesa encontrado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<ExpenseTypeDto>.CreateFailure(
                    "Erro ao buscar tipo de despesa.",
                    new[] { ex.Message });
            }
        }

        #endregion
    }
}
