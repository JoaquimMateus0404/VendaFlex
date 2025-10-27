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
                    return OperationResult<PriceHistoryDto>.CreateFailure("Hist�rico � obrigat�rio.");

                var validation = await _validator.ValidateAsync(priceHistoryDto);
                if (!validation.IsValid)
                {
                    return OperationResult<PriceHistoryDto>.CreateFailure(
                        "Dados inv�lidos.",
                        validation.Errors.Select(e => e.ErrorMessage));
                }

                var entity = _mapper.Map<PriceHistory>(priceHistoryDto);
                var created = await _priceHistoryRepository.AddAsync(entity);
                var resultDto = _mapper.Map<PriceHistoryDto>(created);

                return OperationResult<PriceHistoryDto>.CreateSuccess(resultDto, "Hist�rico de pre�o criado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PriceHistoryDto>.CreateFailure(
                    "Erro ao criar hist�rico de pre�o.",
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
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateSuccess(dtos, $"{dtos.Count()} registro(s) de hist�rico encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure(
                    "Erro ao listar hist�ricos de pre�o.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (endDate < startDate)
                    return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure("Data final n�o pode ser menor que a inicial.");

                var entities = await _priceHistoryRepository.GetByDateRangeAsync(startDate, endDate);
                var dtos = _mapper.Map<IEnumerable<PriceHistoryDto>>(entities);
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateSuccess(dtos, $"{dtos.Count()} registro(s) no intervalo de datas.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure(
                    "Erro ao buscar hist�ricos por intervalo de datas.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<PriceHistoryDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<PriceHistoryDto>.CreateFailure("ID inv�lido.");

                var entity = await _priceHistoryRepository.GetByIdAsync(id);
                if (entity == null)
                    return OperationResult<PriceHistoryDto>.CreateFailure("Hist�rico n�o encontrado.");

                var dto = _mapper.Map<PriceHistoryDto>(entity);
                return OperationResult<PriceHistoryDto>.CreateSuccess(dto, "Hist�rico de pre�o encontrado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PriceHistoryDto>.CreateFailure(
                    "Erro ao buscar hist�rico por ID.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetByProductIdAsync(int productId)
        {
            try
            {
                if (productId <= 0)
                    return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure("Produto inv�lido.");

                var entities = await _priceHistoryRepository.GetByProductIdAsync(productId);
                var dtos = _mapper.Map<IEnumerable<PriceHistoryDto>>(entities);
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateSuccess(dtos, $"{dtos.Count()} registro(s) para o produto.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure(
                    "Erro ao buscar hist�ricos por produto.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<PriceHistoryDto>> GetLatestByProductIdAsync(int productId)
        {
            try
            {
                if (productId <= 0)
                    return OperationResult<PriceHistoryDto>.CreateFailure("Produto inv�lido.");

                var entity = await _priceHistoryRepository.GetLatestByProductIdAsync(productId);
                if (entity == null)
                    return OperationResult<PriceHistoryDto>.CreateFailure("Nenhum hist�rico encontrado para o produto.");

                var dto = _mapper.Map<PriceHistoryDto>(entity);
                return OperationResult<PriceHistoryDto>.CreateSuccess(dto, "�ltimo hist�rico recuperado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PriceHistoryDto>.CreateFailure(
                    "Erro ao buscar �ltimo hist�rico por produto.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1)
                    return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure("P�gina deve ser maior ou igual a 1.");
                if (pageSize < 1)
                    return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure("Tamanho da p�gina deve ser maior que 0.");

                var entities = await _priceHistoryRepository.GetPagedAsync(pageNumber, pageSize);
                var dtos = _mapper.Map<IEnumerable<PriceHistoryDto>>(entities);
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateSuccess(dtos, $"P�gina {pageNumber} com {dtos.Count()} registro(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure(
                    "Erro ao buscar hist�ricos paginados.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetPriceDecreaseHistoryAsync()
        {
            try
            {
                var entities = await _priceHistoryRepository.GetPriceDecreaseHistoryAsync();
                var dtos = _mapper.Map<IEnumerable<PriceHistoryDto>>(entities);
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateSuccess(dtos, $"{dtos.Count()} registro(s) com redu��o de pre�o.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure(
                    "Erro ao buscar redu��es de pre�o.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PriceHistoryDto>>> GetPriceIncreaseHistoryAsync()
        {
            try
            {
                var entities = await _priceHistoryRepository.GetPriceIncreaseHistoryAsync();
                var dtos = _mapper.Map<IEnumerable<PriceHistoryDto>>(entities);
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateSuccess(dtos, $"{dtos.Count()} registro(s) com aumento de pre�o.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PriceHistoryDto>>.CreateFailure(
                    "Erro ao buscar aumentos de pre�o.",
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
                    return OperationResult<PriceHistoryDto>.CreateFailure("Hist�rico � obrigat�rio.");

                var validation = await _validator.ValidateAsync(priceHistoryDto);
                if (!validation.IsValid)
                {
                    return OperationResult<PriceHistoryDto>.CreateFailure(
                        "Dados inv�lidos.",
                        validation.Errors.Select(e => e.ErrorMessage));
                }

                var existing = await _priceHistoryRepository.GetByIdAsync(priceHistoryDto.PriceHistoryId);
                if (existing == null)
                    return OperationResult<PriceHistoryDto>.CreateFailure("Hist�rico n�o encontrado.");

                _mapper.Map(priceHistoryDto, existing);
                var updated = await _priceHistoryRepository.UpdateAsync(existing);
                var resultDto = _mapper.Map<PriceHistoryDto>(updated);

                return OperationResult<PriceHistoryDto>.CreateSuccess(resultDto, "Hist�rico de pre�o atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PriceHistoryDto>.CreateFailure(
                    "Erro ao atualizar hist�rico.",
                    new[] { ex.Message });
            }
        }
    }
}
