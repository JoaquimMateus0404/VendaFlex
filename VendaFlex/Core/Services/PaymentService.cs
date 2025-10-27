using AutoMapper;
using FluentValidation;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço para pagamentos.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly PaymentRepository _paymentRepository;
        private readonly IValidator<PaymentDto> _paymentValidator;
        private readonly IMapper _mapper;

        public PaymentService(
            PaymentRepository paymentRepository,
            IValidator<PaymentDto> paymentValidator,
            IMapper mapper)
        {
            _paymentRepository = paymentRepository;
            _paymentValidator = paymentValidator;
            _mapper = mapper;
        }

        public async Task<OperationResult<PaymentDto>> AddAsync(PaymentDto payment)
        {
            try
            {
                if (payment == null)
                    return OperationResult<PaymentDto>.CreateFailure("Pagamento é obrigatório.");

                var validation = await _paymentValidator.ValidateAsync(payment);
                if (!validation.IsValid)
                    return OperationResult<PaymentDto>.CreateFailure("Dados inválidos.", validation.Errors.Select(e => e.ErrorMessage));

                var entity = _mapper.Map<VendaFlex.Data.Entities.Payment>(payment);
                var created = await _paymentRepository.AddAsync(entity);
                var dto = _mapper.Map<PaymentDto>(created);
                return OperationResult<PaymentDto>.CreateSuccess(dto, "Pagamento registrado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PaymentDto>.CreateFailure("Erro ao registrar pagamento.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<bool>> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<bool>.CreateFailure("ID inválido.");

                var exists = await _paymentRepository.ExistsAsync(id);
                if (!exists)
                    return OperationResult<bool>.CreateFailure("Pagamento não encontrado.");

                var deleted = await _paymentRepository.DeleteAsync(id);
                return deleted
                    ? OperationResult<bool>.CreateSuccess(true, "Pagamento removido com sucesso.")
                    : OperationResult<bool>.CreateFailure("Não foi possível remover o pagamento.");
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.CreateFailure("Erro ao remover pagamento.", new[] { ex.Message });
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try { return await _paymentRepository.ExistsAsync(id); } catch { return false; }
        }

        public async Task<OperationResult<IEnumerable<PaymentDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                    return OperationResult<IEnumerable<PaymentDto>>.CreateFailure("Data inicial não pode ser maior que a final.");

                var entities = await _paymentRepository.GetByDateRangeAsync(startDate, endDate);
                var dtos = _mapper.Map<IEnumerable<PaymentDto>>(entities);
                return OperationResult<IEnumerable<PaymentDto>>.CreateSuccess(dtos, $"{dtos.Count()} pagamento(s) no período.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PaymentDto>>.CreateFailure("Erro ao buscar por período.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<PaymentDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<PaymentDto>.CreateFailure("ID inválido.");

                var entity = await _paymentRepository.GetByIdAsync(id);
                if (entity == null)
                    return OperationResult<PaymentDto>.CreateFailure("Pagamento não encontrado.");

                var dto = _mapper.Map<PaymentDto>(entity);
                return OperationResult<PaymentDto>.CreateSuccess(dto, "Pagamento encontrado.");
            }
            catch (Exception ex)
            {
                return OperationResult<PaymentDto>.CreateFailure("Erro ao buscar pagamento.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PaymentDto>>> GetByInvoiceIdAsync(int invoiceId)
        {
            try
            {
                if (invoiceId <= 0)
                    return OperationResult<IEnumerable<PaymentDto>>.CreateFailure("Fatura inválida.");

                var entities = await _paymentRepository.GetByInvoiceIdAsync(invoiceId);
                var dtos = _mapper.Map<IEnumerable<PaymentDto>>(entities);
                return OperationResult<IEnumerable<PaymentDto>>.CreateSuccess(dtos, $"{dtos.Count()} pagamento(s) da fatura.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PaymentDto>>.CreateFailure("Erro ao listar pagamentos da fatura.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PaymentDto>>> GetByPaymentTypeIdAsync(int paymentTypeId)
        {
            try
            {
                if (paymentTypeId <= 0)
                    return OperationResult<IEnumerable<PaymentDto>>.CreateFailure("Tipo de pagamento inválido.");

                var entities = await _paymentRepository.GetByPaymentTypeIdAsync(paymentTypeId);
                var dtos = _mapper.Map<IEnumerable<PaymentDto>>(entities);
                return OperationResult<IEnumerable<PaymentDto>>.CreateSuccess(dtos, $"{dtos.Count()} pagamento(s) do tipo.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PaymentDto>>.CreateFailure("Erro ao listar por tipo de pagamento.", new[] { ex.Message });
            }
        }

        public async Task<decimal> GetTotalAmountByInvoiceIdAsync(int invoiceId)
        {
            try { return await _paymentRepository.GetTotalAmountByInvoiceIdAsync(invoiceId); } catch { return 0m; }
        }

        public async Task<OperationResult<PaymentDto>> UpdateAsync(PaymentDto payment)
        {
            try
            {
                if (payment == null)
                    return OperationResult<PaymentDto>.CreateFailure("Pagamento é obrigatório.");

                var validation = await _paymentValidator.ValidateAsync(payment);
                if (!validation.IsValid)
                    return OperationResult<PaymentDto>.CreateFailure("Dados inválidos.", validation.Errors.Select(e => e.ErrorMessage));

                var existing = await _paymentRepository.GetByIdAsync(payment.PaymentId);
                if (existing == null)
                    return OperationResult<PaymentDto>.CreateFailure("Pagamento não encontrado.");

                var entity = _mapper.Map<VendaFlex.Data.Entities.Payment>(payment);
                var updated = await _paymentRepository.UpdateAsync(entity);
                var dto = _mapper.Map<PaymentDto>(updated);
                return OperationResult<PaymentDto>.CreateSuccess(dto, "Pagamento atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PaymentDto>.CreateFailure("Erro ao atualizar pagamento.", new[] { ex.Message });
            }
        }
    }
}
