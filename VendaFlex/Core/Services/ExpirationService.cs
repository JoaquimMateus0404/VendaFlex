using AutoMapper;
using FluentValidation;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    public class ExpirationService : IExpirationService
    {
        private readonly ExpirationRepository _expirationRepository;
        private readonly IValidator<ExpirationDto> _expirationDtoValidator;
        private readonly IMapper _mapper;

        public ExpirationService(
            ExpirationRepository expirationRepository,
            IValidator<ExpirationDto> expirationDtoValidator,
            IMapper mapper)
        {
            _expirationRepository = expirationRepository;
            _expirationDtoValidator = expirationDtoValidator;
            _mapper = mapper;
        }

        public async Task<OperationResult<ExpirationDto>> AddAsync(ExpirationDto expiration)
        {
            try
            {
                if (expiration == null)
                    return OperationResult<ExpirationDto>.CreateFailure("Expiração é obrigatória.");

                var validation = await _expirationDtoValidator.ValidateAsync(expiration);
                if (!validation.IsValid)
                {
                    return OperationResult<ExpirationDto>.CreateFailure(
                        "Dados inválidos.",
                        validation.Errors.Select(e => e.ErrorMessage));
                }

                var entity = _mapper.Map<VendaFlex.Data.Entities.Expiration>(expiration);
                var created = await _expirationRepository.AddAsync(entity);
                var resultDto = _mapper.Map<ExpirationDto>(created);

                return OperationResult<ExpirationDto>.CreateSuccess(resultDto, "Expiração criada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<ExpirationDto>.CreateFailure(
                    "Erro ao criar expiração.",
                    new[] { ex.Message });
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                return await _expirationRepository.DeleteAsync(id);
            }
            catch
            {
                return false;
            }
        }

        public async Task<OperationResult<IEnumerable<ExpirationDto>>> GetAllAsync()
        {
            try
            {
                var entities = await _expirationRepository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<ExpirationDto>>(entities);
                return OperationResult<IEnumerable<ExpirationDto>>.CreateSuccess(dtos, $"{dtos.Count()} expiração(ões) encontrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpirationDto>>.CreateFailure(
                    "Erro ao listar expirações.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ExpirationDto>>> GetByBatchNumberAsync(string batchNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(batchNumber))
                    return OperationResult<IEnumerable<ExpirationDto>>.CreateFailure("Número do lote é obrigatório.");

                var entities = await _expirationRepository.GetByBatchNumberAsync(batchNumber);
                var dtos = _mapper.Map<IEnumerable<ExpirationDto>>(entities);
                return OperationResult<IEnumerable<ExpirationDto>>.CreateSuccess(dtos, $"{dtos.Count()} registro(s) para o lote.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpirationDto>>.CreateFailure(
                    "Erro ao buscar por número de lote.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ExpirationDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (endDate < startDate)
                    return OperationResult<IEnumerable<ExpirationDto>>.CreateFailure("Data final não pode ser menor que a inicial.");

                var entities = await _expirationRepository.GetByDateRangeAsync(startDate, endDate);
                var dtos = _mapper.Map<IEnumerable<ExpirationDto>>(entities);
                return OperationResult<IEnumerable<ExpirationDto>>.CreateSuccess(dtos, $"{dtos.Count()} registro(s) no intervalo de datas.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpirationDto>>.CreateFailure(
                    "Erro ao buscar por intervalo de datas.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<ExpirationDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<ExpirationDto>.CreateFailure("ID inválido.");

                var entity = await _expirationRepository.GetByIdAsync(id);
                if (entity == null)
                    return OperationResult<ExpirationDto>.CreateFailure("Expiração não encontrada.");

                var dto = _mapper.Map<ExpirationDto>(entity);
                return OperationResult<ExpirationDto>.CreateSuccess(dto, "Expiração encontrada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<ExpirationDto>.CreateFailure(
                    "Erro ao buscar expiração.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ExpirationDto>>> GetByProductIdAsync(int productId)
        {
            try
            {
                if (productId <= 0)
                    return OperationResult<IEnumerable<ExpirationDto>>.CreateFailure("Produto inválido.");

                var entities = await _expirationRepository.GetByProductIdAsync(productId);
                var dtos = _mapper.Map<IEnumerable<ExpirationDto>>(entities);
                return OperationResult<IEnumerable<ExpirationDto>>.CreateSuccess(dtos, $"{dtos.Count()} registro(s) para o produto.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpirationDto>>.CreateFailure(
                    "Erro ao buscar por produto.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ExpirationDto>>> GetExpiredAsync()
        {
            try
            {
                var entities = await _expirationRepository.GetExpiredAsync();
                var dtos = _mapper.Map<IEnumerable<ExpirationDto>>(entities);
                return OperationResult<IEnumerable<ExpirationDto>>.CreateSuccess(dtos, $"{dtos.Count()} expiração(ões) vencida(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpirationDto>>.CreateFailure(
                    "Erro ao buscar expirações vencidas.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ExpirationDto>>> GetExpiringAsync(int days)
        {
            try
            {
                var entities = await _expirationRepository.GetNearExpirationAsync(days);
                var dtos = _mapper.Map<IEnumerable<ExpirationDto>>(entities);
                return OperationResult<IEnumerable<ExpirationDto>>.CreateSuccess(dtos, $"{dtos.Count()} expiração(ões) prestes a vencer.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpirationDto>>.CreateFailure(
                    "Erro ao buscar expirações prestes a vencer.",
                    new[] { ex.Message });
            }
        }
        public async Task<int> GetExpiredQuantityByProductAsync(int productId)
        {
            try
            {
                if (productId <= 0)
                    return 0;

                return await _expirationRepository.GetExpiredQuantityByProductAsync(productId);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<OperationResult<IEnumerable<ExpirationDto>>> GetNearExpirationAsync()
        {
            try
            {
                var entities = await _expirationRepository.GetNearExpirationAsync();
                var dtos = _mapper.Map<IEnumerable<ExpirationDto>>(entities);
                return OperationResult<IEnumerable<ExpirationDto>>.CreateSuccess(dtos, $"{dtos.Count()} expiração(ões) próximas do vencimento.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpirationDto>>.CreateFailure(
                    "Erro ao buscar expirações próximas do vencimento.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ExpirationDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1)
                    return OperationResult<IEnumerable<ExpirationDto>>.CreateFailure("Página deve ser maior ou igual a 1.");
                if (pageSize < 1)
                    return OperationResult<IEnumerable<ExpirationDto>>.CreateFailure("Tamanho da página deve ser maior que 0.");

                var entities = await _expirationRepository.GetPagedAsync(pageNumber, pageSize);
                var dtos = _mapper.Map<IEnumerable<ExpirationDto>>(entities);
                return OperationResult<IEnumerable<ExpirationDto>>.CreateSuccess(dtos, $"Página {pageNumber} com {dtos.Count()} registro(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ExpirationDto>>.CreateFailure(
                    "Erro ao buscar expirações paginadas.",
                    new[] { ex.Message });
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _expirationRepository.GetTotalCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<OperationResult<ExpirationDto>> UpdateAsync(ExpirationDto expiration)
        {
            try
            {
                if (expiration == null)
                    return OperationResult<ExpirationDto>.CreateFailure("Expiração é obrigatória.");

                var validation = await _expirationDtoValidator.ValidateAsync(expiration);
                if (!validation.IsValid)
                {
                    return OperationResult<ExpirationDto>.CreateFailure(
                        "Dados inválidos.",
                        validation.Errors.Select(e => e.ErrorMessage));
                }

                var existing = await _expirationRepository.GetByIdAsync(expiration.ExpirationId);
                if (existing == null)
                    return OperationResult<ExpirationDto>.CreateFailure("Expiração não encontrada.");

                _mapper.Map(expiration, existing);
                var updated = await _expirationRepository.UpdateAsync(existing);
                var resultDto = _mapper.Map<ExpirationDto>(updated);

                return OperationResult<ExpirationDto>.CreateSuccess(resultDto, "Expiração atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<ExpirationDto>.CreateFailure(
                    "Erro ao atualizar expiração.",
                    new[] { ex.Message });
            }
        }
    }
}
