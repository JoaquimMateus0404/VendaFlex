using AutoMapper;
using FluentValidation;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    public class PaymentTypeService : IPaymentTypeService
    {
        private readonly PaymentTypeRepository _paymentTypeRepository;
        private readonly IValidator<PaymentTypeDto> _validator;
        private readonly IMapper _mapper;

        public PaymentTypeService(PaymentTypeRepository paymentTypeRepository, IValidator<PaymentTypeDto> validator, IMapper mapper)
        {
            _paymentTypeRepository = paymentTypeRepository;
            _validator = validator;
            _mapper = mapper;
        }

        public async Task<OperationResult<PaymentTypeDto>> AddAsync(PaymentTypeDto dto)
        {
            try
            {
                if (dto == null)
                    return OperationResult<PaymentTypeDto>.CreateFailure("Tipo de pagamento é obrigatório.");

                var validation = await _validator.ValidateAsync(dto);
                if (!validation.IsValid)
                    return OperationResult<PaymentTypeDto>.CreateFailure("Dados inválidos.", validation.Errors.Select(e => e.ErrorMessage));

                // Verificar duplicidade por nome
                if (await _paymentTypeRepository.NameExistsAsync(dto.Name))
                    return OperationResult<PaymentTypeDto>.CreateFailure("Nome já está em uso.");

                var entity = _mapper.Map<VendaFlex.Data.Entities.PaymentType>(dto);
                var created = await _paymentTypeRepository.AddAsync(entity);
                var result = _mapper.Map<PaymentTypeDto>(created);
                return OperationResult<PaymentTypeDto>.CreateSuccess(result, "Tipo de pagamento criado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PaymentTypeDto>.CreateFailure("Erro ao criar tipo de pagamento.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<bool>> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<bool>.CreateFailure("ID inválido.");

                var exists = await _paymentTypeRepository.ExistsAsync(id);
                if (!exists)
                    return OperationResult<bool>.CreateFailure("Tipo de pagamento não encontrado.");

                var deleted = await _paymentTypeRepository.DeleteAsync(id);
                return deleted
                    ? OperationResult<bool>.CreateSuccess(true, "Tipo de pagamento removido com sucesso.")
                    : OperationResult<bool>.CreateFailure("Não foi possível remover o tipo de pagamento.");
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.CreateFailure("Erro ao remover tipo de pagamento.", new[] { ex.Message });
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try { return await _paymentTypeRepository.ExistsAsync(id); } catch { return false; }
        }

        public async Task<OperationResult<IEnumerable<PaymentTypeDto>>> GetActiveAsync()
        {
            try
            {
                var entities = await _paymentTypeRepository.GetActiveAsync();
                var dtos = _mapper.Map<IEnumerable<PaymentTypeDto>>(entities);
                return OperationResult<IEnumerable<PaymentTypeDto>>.CreateSuccess(dtos, $"{dtos.Count()} tipo(s) ativo(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PaymentTypeDto>>.CreateFailure("Erro ao listar tipos de pagamento ativos.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PaymentTypeDto>>> GetAllAsync()
        {
            try
            {
                var entities = await _paymentTypeRepository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<PaymentTypeDto>>(entities);
                return OperationResult<IEnumerable<PaymentTypeDto>>.CreateSuccess(dtos, $"{dtos.Count()} tipo(s) encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PaymentTypeDto>>.CreateFailure("Erro ao listar tipos de pagamento.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<PaymentTypeDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<PaymentTypeDto>.CreateFailure("ID inválido.");

                var entity = await _paymentTypeRepository.GetByIdAsync(id);
                if (entity == null)
                    return OperationResult<PaymentTypeDto>.CreateFailure("Tipo de pagamento não encontrado.");

                var dto = _mapper.Map<PaymentTypeDto>(entity);
                return OperationResult<PaymentTypeDto>.CreateSuccess(dto, "Tipo de pagamento encontrado.");
            }
            catch (Exception ex)
            {
                return OperationResult<PaymentTypeDto>.CreateFailure("Erro ao buscar tipo de pagamento.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PaymentTypeDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1)
                    return OperationResult<IEnumerable<PaymentTypeDto>>.CreateFailure("Página deve ser >= 1.");
                if (pageSize < 1)
                    return OperationResult<IEnumerable<PaymentTypeDto>>.CreateFailure("Tamanho da página deve ser > 0.");

                var entities = await _paymentTypeRepository.GetPagedAsync(pageNumber, pageSize);
                var dtos = _mapper.Map<IEnumerable<PaymentTypeDto>>(entities);
                return OperationResult<IEnumerable<PaymentTypeDto>>.CreateSuccess(dtos, $"Página {pageNumber} com {dtos.Count()} tipo(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PaymentTypeDto>>.CreateFailure("Erro ao buscar paginado.", new[] { ex.Message });
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            try { return await _paymentTypeRepository.GetTotalCountAsync(); } catch { return 0; }
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            try { return await _paymentTypeRepository.NameExistsAsync(name, excludeId); } catch { return false; }
        }

        public async Task<OperationResult<IEnumerable<PaymentTypeDto>>> SearchAsync(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                    return OperationResult<IEnumerable<PaymentTypeDto>>.CreateFailure("Termo de busca é obrigatório.");

                var entities = await _paymentTypeRepository.SearchAsync(term);
                var dtos = _mapper.Map<IEnumerable<PaymentTypeDto>>(entities);
                return OperationResult<IEnumerable<PaymentTypeDto>>.CreateSuccess(dtos, $"{dtos.Count()} resultado(s) para '{term}'.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PaymentTypeDto>>.CreateFailure("Erro na busca de tipos de pagamento.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<PaymentTypeDto>> UpdateAsync(PaymentTypeDto dto)
        {
            try
            {
                if (dto == null)
                    return OperationResult<PaymentTypeDto>.CreateFailure("Tipo de pagamento é obrigatório.");

                var validation = await _validator.ValidateAsync(dto);
                if (!validation.IsValid)
                    return OperationResult<PaymentTypeDto>.CreateFailure("Dados inválidos.", validation.Errors.Select(e => e.ErrorMessage));

                var existing = await _paymentTypeRepository.GetByIdAsync(dto.PaymentTypeId);
                if (existing == null)
                    return OperationResult<PaymentTypeDto>.CreateFailure("Tipo de pagamento não encontrado.");

                if (await _paymentTypeRepository.NameExistsAsync(dto.Name, dto.PaymentTypeId))
                    return OperationResult<PaymentTypeDto>.CreateFailure("Nome já está em uso.");

                var entity = _mapper.Map<VendaFlex.Data.Entities.PaymentType>(dto);
                var updated = await _paymentTypeRepository.UpdateAsync(entity);
                var result = _mapper.Map<PaymentTypeDto>(updated);
                return OperationResult<PaymentTypeDto>.CreateSuccess(result, "Tipo de pagamento atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PaymentTypeDto>.CreateFailure("Erro ao atualizar tipo de pagamento.", new[] { ex.Message });
            }
        }
    }
}
