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
    /// Serviço para consulta de movimentações de estoque.
    /// ATENÇÃO: As movimentações são criadas AUTOMATICAMENTE pelo sistema através do StockAuditService.
    /// Este serviço é apenas para CONSULTA, não permitindo criação manual de movimentações.
    /// </summary>
    public class StockMovementService : IStockMovementService
    {
        private readonly StockMovementRepository _stockMovementRepository;
        private readonly IValidator<StockMovementDto> _validator;
        private readonly IMapper _mapper;

        public StockMovementService(
            StockMovementRepository stockMovementRepository,
            IValidator<StockMovementDto> validator,
            IMapper mapper)
        {
            _stockMovementRepository = stockMovementRepository;
            _validator = validator;
            _mapper = mapper;
        }

        /// <summary>
        /// MÉTODO INTERNO - Não deve ser usado pela UI.
        /// As movimentações são criadas automaticamente pelo StockAuditService.
        /// </summary>
        [Obsolete("Movimentações são criadas automaticamente pelo sistema. Use apenas para migração de dados.")]
        public async Task<OperationResult<StockMovementDto>> AddAsync(StockMovementDto movement)
        {
            try
            {
                if (movement == null)
                    return OperationResult<StockMovementDto>.CreateFailure("Movimentação é obrigatória.");

                var validation = await _validator.ValidateAsync(movement);
                if (!validation.IsValid)
                {
                    return OperationResult<StockMovementDto>.CreateFailure(
                        "Dados inválidos.",
                        validation.Errors.Select(e => e.ErrorMessage));
                }

                var entity = _mapper.Map<StockMovement>(movement);
                var created = await _stockMovementRepository.AddAsync(entity);
                var resultDto = _mapper.Map<StockMovementDto>(created);

                return OperationResult<StockMovementDto>.CreateSuccess(resultDto, "Movimentação registrada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<StockMovementDto>.CreateFailure(
                    "Erro ao registrar movimentação.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// MÉTODO BLOQUEADO - Movimentações não devem ser deletadas pois são auditoria.
        /// </summary>
        [Obsolete("Movimentações não podem ser deletadas pois fazem parte da auditoria do sistema.")]
        public async Task<bool> DeleteAsync(int id)
        {
            // Movimentações não devem ser deletadas - são histórico de auditoria
            return false;
        }

        public async Task<OperationResult<IEnumerable<StockMovementDto>>> GetAllAsync()
        {
            try
            {
                var entities = await _stockMovementRepository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<StockMovementDto>>(entities);
                return OperationResult<IEnumerable<StockMovementDto>>.CreateSuccess(dtos, $"{dtos.Count()} movimentação(ões) encontrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<StockMovementDto>>.CreateFailure(
                    "Erro ao listar movimentações.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<StockMovementDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (endDate < startDate)
                    return OperationResult<IEnumerable<StockMovementDto>>.CreateFailure("Data final não pode ser menor que a inicial.");

                var entities = await _stockMovementRepository.GetByDateRangeAsync(startDate, endDate);
                var dtos = _mapper.Map<IEnumerable<StockMovementDto>>(entities);
                return OperationResult<IEnumerable<StockMovementDto>>.CreateSuccess(dtos, $"{dtos.Count()} movimentação(ões) no intervalo de datas.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<StockMovementDto>>.CreateFailure(
                    "Erro ao buscar movimentações por intervalo de datas.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<StockMovementDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<StockMovementDto>.CreateFailure("ID inválido.");

                var entity = await _stockMovementRepository.GetByIdAsync(id);
                if (entity == null)
                    return OperationResult<StockMovementDto>.CreateFailure("Movimentação não encontrada.");

                var dto = _mapper.Map<StockMovementDto>(entity);
                return OperationResult<StockMovementDto>.CreateSuccess(dto, "Movimentação encontrada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<StockMovementDto>.CreateFailure(
                    "Erro ao buscar movimentação por ID.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<StockMovementDto>>> GetByProductAndDateRangeAsync(int productId, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (productId <= 0)
                    return OperationResult<IEnumerable<StockMovementDto>>.CreateFailure("Produto inválido.");
                if (endDate < startDate)
                    return OperationResult<IEnumerable<StockMovementDto>>.CreateFailure("Data final não pode ser menor que a inicial.");

                var entities = await _stockMovementRepository.GetByProductAndDateRangeAsync(productId, startDate, endDate);
                var dtos = _mapper.Map<IEnumerable<StockMovementDto>>(entities);
                return OperationResult<IEnumerable<StockMovementDto>>.CreateSuccess(dtos, $"{dtos.Count()} movimentação(ões) do produto no intervalo.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<StockMovementDto>>.CreateFailure(
                    "Erro ao buscar movimentações do produto no intervalo.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<StockMovementDto>>> GetByProductIdAsync(int productId)
        {
            try
            {
                if (productId <= 0)
                    return OperationResult<IEnumerable<StockMovementDto>>.CreateFailure("Produto inválido.");

                var entities = await _stockMovementRepository.GetByProductIdAsync(productId);
                var dtos = _mapper.Map<IEnumerable<StockMovementDto>>(entities);
                return OperationResult<IEnumerable<StockMovementDto>>.CreateSuccess(dtos, $"{dtos.Count()} movimentação(ões) do produto.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<StockMovementDto>>.CreateFailure(
                    "Erro ao buscar movimentações por produto.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<StockMovementDto>>> GetByTypeAsync(StockMovementType type)
        {
            try
            {
                var entities = await _stockMovementRepository.GetByTypeAsync(type);
                var dtos = _mapper.Map<IEnumerable<StockMovementDto>>(entities);
                return OperationResult<IEnumerable<StockMovementDto>>.CreateSuccess(dtos, $"{dtos.Count()} movimentação(ões) do tipo {type}.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<StockMovementDto>>.CreateFailure(
                    "Erro ao buscar movimentações por tipo.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<StockMovementDto>>> GetByUserIdAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    return OperationResult<IEnumerable<StockMovementDto>>.CreateFailure("Usuário inválido.");

                var entities = await _stockMovementRepository.GetByUserIdAsync(userId);
                var dtos = _mapper.Map<IEnumerable<StockMovementDto>>(entities);
                return OperationResult<IEnumerable<StockMovementDto>>.CreateSuccess(dtos, $"{dtos.Count()} movimentação(ões) do usuário.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<StockMovementDto>>.CreateFailure(
                    "Erro ao buscar movimentações por usuário.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<StockMovementDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1)
                    return OperationResult<IEnumerable<StockMovementDto>>.CreateFailure("Página deve ser maior ou igual a 1.");
                if (pageSize < 1)
                    return OperationResult<IEnumerable<StockMovementDto>>.CreateFailure("Tamanho da página deve ser maior que 0.");

                var entities = await _stockMovementRepository.GetPagedAsync(pageNumber, pageSize);
                var dtos = _mapper.Map<IEnumerable<StockMovementDto>>(entities);
                return OperationResult<IEnumerable<StockMovementDto>>.CreateSuccess(dtos, $"Página {pageNumber} com {dtos.Count()} movimentação(ões).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<StockMovementDto>>.CreateFailure(
                    "Erro ao buscar movimentações paginadas.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<decimal>> GetTotalCostByProductAsync(int productId)
        {
            try
            {
                if (productId <= 0)
                    return OperationResult<decimal>.CreateFailure("Produto inválido.");

                var total = await _stockMovementRepository.GetTotalCostByProductAsync(productId);
                return OperationResult<decimal>.CreateSuccess(total, "Custo total calculado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<decimal>.CreateFailure(
                    "Erro ao calcular custo total por produto.",
                    new[] { ex.Message });
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _stockMovementRepository.GetTotalCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// MÉTODO BLOQUEADO - Movimentações não devem ser atualizadas pois são auditoria.
        /// </summary>
        [Obsolete("Movimentações não podem ser alteradas pois fazem parte da auditoria do sistema.")]
        public async Task<OperationResult<StockMovementDto>> UpdateAsync(StockMovementDto movement)
        {
            // Movimentações não devem ser alteradas - são histórico imutável de auditoria
            return OperationResult<StockMovementDto>.CreateFailure(
                "Movimentações não podem ser alteradas pois fazem parte da auditoria do sistema.");
        }
    }
}
