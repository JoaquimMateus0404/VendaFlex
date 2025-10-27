using AutoMapper;
using FluentValidation;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    public class PriceHistoryService : IPriceHistoryService
    {
        private readonly PriceHistoryRepository _priceHistoryRepository;
        private readonly IValidator<PriceHistoryDto> _validator;
        private readonly IMapper _mapper;

        public PriceHistoryService(
            PriceHistoryRepository priceHistoryRepository,
            IValidator<PriceHistoryDto> validator,
            IMapper mapper)
        {
            _priceHistoryRepository = priceHistoryRepository;
            _validator = validator;
            _mapper = mapper;
        }

        public async Task<OperationResult<PriceHistoryDto>> AddAsync(PriceHistoryDto priceHistoryDto)
        {
            try
            {
                if (priceHistoryDto == null)
                    return OperationResult<PriceHistoryDto>.CreateFailure("Histórico é obrigatório.");

                var validation = await _validator.ValidateAsync(priceHistoryDto);
                if (!validation.IsValid)
                {
                    return OperationResult<PriceHistoryDto>.CreateFailure(
                        "Dados inválidos.",
                        validation.Errors.Select(e => e.ErrorMessage));
                }

                var entity = _mapper.Map<PriceHistory>(priceHistoryDto);
                var created = await _priceHistoryRepository.AddAsync(entity);
                var resultDto = _mapper.Map<PriceHistoryDto>(created);

                return OperationResult<PriceHistoryDto>.CreateSuccess(resultDto, "Histórico de preço criado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PriceHistoryDto>.CreateFailure(
                    "Erro ao criar histórico de preço.",
                    new[] { ex.Message });
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                return await _priceHistoryRepository.DeleteAsync(id);
            }
            catch
            {
                return false;
            }
        }

        public async Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetAllAsync()
        {
            try
            {
                var entities = await _priceHistoryRepository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<PriceHistoryDto>>(entities);
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateSuccess(dtos, $"{dtos.Count()} registro(s) de histórico encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure(
                    "Erro ao listar históricos de preço.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (endDate < startDate)
                    return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure("Data final não pode ser menor que a inicial.");

                var entities = await _priceHistoryRepository.GetByDateRangeAsync(startDate, endDate);
                var dtos = _mapper.Map<IEnumerable<PriceHistoryDto>>(entities);
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateSuccess(dtos, $"{dtos.Count()} registro(s) no intervalo de datas.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure(
                    "Erro ao buscar históricos por intervalo de datas.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<PriceHistoryDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<PriceHistoryDto>.CreateFailure("ID inválido.");

                var entity = await _priceHistoryRepository.GetByIdAsync(id);
                if (entity == null)
                    return OperationResult<PriceHistoryDto>.CreateFailure("Histórico não encontrado.");

                var dto = _mapper.Map<PriceHistoryDto>(entity);
                return OperationResult<PriceHistoryDto>.CreateSuccess(dto, "Histórico de preço encontrado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PriceHistoryDto>.CreateFailure(
                    "Erro ao buscar histórico por ID.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetByProductIdAsync(int productId)
        {
            try
            {
                if (productId <= 0)
                    return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure("Produto inválido.");

                var entities = await _priceHistoryRepository.GetByProductIdAsync(productId);
                var dtos = _mapper.Map<IEnumerable<PriceHistoryDto>>(entities);
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateSuccess(dtos, $"{dtos.Count()} registro(s) para o produto.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure(
                    "Erro ao buscar históricos por produto.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<PriceHistoryDto>> GetLatestByProductIdAsync(int productId)
        {
            try
            {
                if (productId <= 0)
                    return OperationResult<PriceHistoryDto>.CreateFailure("Produto inválido.");

                var entity = await _priceHistoryRepository.GetLatestByProductIdAsync(productId);
                if (entity == null)
                    return OperationResult<PriceHistoryDto>.CreateFailure("Nenhum histórico encontrado para o produto.");

                var dto = _mapper.Map<PriceHistoryDto>(entity);
                return OperationResult<PriceHistoryDto>.CreateSuccess(dto, "Último histórico recuperado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PriceHistoryDto>.CreateFailure(
                    "Erro ao buscar último histórico por produto.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1)
                    return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure("Página deve ser maior ou igual a 1.");
                if (pageSize < 1)
                    return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure("Tamanho da página deve ser maior que 0.");

                var entities = await _priceHistoryRepository.GetPagedAsync(pageNumber, pageSize);
                var dtos = _mapper.Map<IEnumerable<PriceHistoryDto>>(entities);
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateSuccess(dtos, $"Página {pageNumber} com {dtos.Count()} registro(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure(
                    "Erro ao buscar históricos paginados.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetPriceDecreaseHistoryAsync()
        {
            try
            {
                var entities = await _priceHistoryRepository.GetPriceDecreaseHistoryAsync();
                var dtos = _mapper.Map<IEnumerable<PriceHistoryDto>>(entities);
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateSuccess(dtos, $"{dtos.Count()} registro(s) com redução de preço.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure(
                    "Erro ao buscar reduções de preço.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetPriceIncreaseHistoryAsync()
        {
            try
            {
                var entities = await _priceHistoryRepository.GetPriceIncreaseHistoryAsync();
                var dtos = _mapper.Map<IEnumerable<PriceHistoryDto>>(entities);
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateSuccess(dtos, $"{dtos.Count()} registro(s) com aumento de preço.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure(
                    "Erro ao buscar aumentos de preço.",
                    new[] { ex.Message });
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _priceHistoryRepository.GetTotalCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<OperationResult<PriceHistoryDto>> UpdateAsync(PriceHistoryDto priceHistoryDto)
        {
            try
            {
                if (priceHistoryDto == null)
                    return OperationResult<PriceHistoryDto>.CreateFailure("Histórico é obrigatório.");

                var validation = await _validator.ValidateAsync(priceHistoryDto);
                if (!validation.IsValid)
                {
                    return OperationResult<PriceHistoryDto>.CreateFailure(
                        "Dados inválidos.",
                        validation.Errors.Select(e => e.ErrorMessage));
                }

                var existing = await _priceHistoryRepository.GetByIdAsync(priceHistoryDto.PriceHistoryId);
                if (existing == null)
                    return OperationResult<PriceHistoryDto>.CreateFailure("Histórico não encontrado.");

                _mapper.Map(priceHistoryDto, existing);
                var updated = await _priceHistoryRepository.UpdateAsync(existing);
                var resultDto = _mapper.Map<PriceHistoryDto>(updated);

                return OperationResult<PriceHistoryDto>.CreateSuccess(resultDto, "Histórico de preço atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PriceHistoryDto>.CreateFailure(
                    "Erro ao atualizar histórico.",
                    new[] { ex.Message });
            }
        }
    }
}
