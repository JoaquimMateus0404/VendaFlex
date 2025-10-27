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
    /// Servi�o para gest�o de faturas e opera��es relacionadas.
    /// </summary>
    public class InvoiceService : IInvoiceService
    {
        private readonly InvoiceRepository _invoiceRepository;
        private readonly IValidator<InvoiceDto> _invoiceValidator;
        private readonly IMapper _mapper;
        public InvoiceService(
            InvoiceRepository invoiceRepository,
            IValidator<InvoiceDto> invoiceValidator,
            IMapper mapper)
        {
            _invoiceRepository = invoiceRepository;
            _invoiceValidator = invoiceValidator;
            _mapper = mapper;
        }

        public async Task<OperationResult<InvoiceDto>> AddAsync(InvoiceDto invoice)
        {
            try
            {
                if (invoice == null)
                    return OperationResult<InvoiceDto>.CreateFailure("Fatura � obrigat�ria.");

                var validation = await _invoiceValidator.ValidateAsync(invoice);
                if (!validation.IsValid)
                    return OperationResult<InvoiceDto>.CreateFailure("Dados inv�lidos.", validation.Errors.Select(e => e.ErrorMessage));

                if (await _invoiceRepository.NumberExistsAsync(invoice.InvoiceNumber))
                    return OperationResult<InvoiceDto>.CreateFailure("N�mero de fatura j� est� em uso.");

                var entity = _mapper.Map<Invoice>(invoice);
                var created = await _invoiceRepository.AddAsync(entity);
                var dto = _mapper.Map<InvoiceDto>(created);
                return OperationResult<InvoiceDto>.CreateSuccess(dto, "Fatura criada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<InvoiceDto>.CreateFailure("Erro ao criar fatura.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<bool>> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<bool>.CreateFailure("ID inv�lido.");

                var exists = await _invoiceRepository.ExistsAsync(id);
                if (!exists)
                    return OperationResult<bool>.CreateFailure("Fatura n�o encontrada.");

                var deleted = await _invoiceRepository.DeleteAsync(id);
                return deleted
                    ? OperationResult<bool>.CreateSuccess(true, "Fatura removida com sucesso.")
                    : OperationResult<bool>.CreateFailure("N�o foi poss�vel remover a fatura.");
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.CreateFailure("Erro ao remover fatura.", new[] { ex.Message });
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try { return await _invoiceRepository.ExistsAsync(id); } catch { return false; }
        }

        public async Task<OperationResult<IEnumerable<InvoiceDto>>> GetAllAsync()
        {
            try
            {
                var entities = await _invoiceRepository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(entities);
                return OperationResult<IEnumerable<InvoiceDto>>.CreateSuccess(dtos, $"{dtos.Count()} fatura(s) encontrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Erro ao listar faturas.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<InvoiceDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                    return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Data inicial n�o pode ser maior que a final.");

                var entities = await _invoiceRepository.GetByDateRangeAsync(startDate, endDate);
                var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(entities);
                return OperationResult<IEnumerable<InvoiceDto>>.CreateSuccess(dtos, $"{dtos.Count()} fatura(s) no per�odo.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Erro ao buscar por per�odo.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<InvoiceDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<InvoiceDto>.CreateFailure("ID inv�lido.");

                var entity = await _invoiceRepository.GetByIdAsync(id);
                if (entity == null)
                    return OperationResult<InvoiceDto>.CreateFailure("Fatura n�o encontrada.");

                var dto = _mapper.Map<InvoiceDto>(entity);
                return OperationResult<InvoiceDto>.CreateSuccess(dto, "Fatura encontrada.");
            }
            catch (Exception ex)
            {
                return OperationResult<InvoiceDto>.CreateFailure("Erro ao buscar fatura.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<InvoiceDto>> GetByNumberAsync(string invoiceNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(invoiceNumber))
                    return OperationResult<InvoiceDto>.CreateFailure("N�mero da fatura � obrigat�rio.");

                var entity = await _invoiceRepository.GetByNumberAsync(invoiceNumber);
                if (entity == null)
                    return OperationResult<InvoiceDto>.CreateFailure("Fatura n�o encontrada.");

                var dto = _mapper.Map<InvoiceDto>(entity);
                return OperationResult<InvoiceDto>.CreateSuccess(dto, "Fatura encontrada.");
            }
            catch (Exception ex)
            {
                return OperationResult<InvoiceDto>.CreateFailure("Erro ao buscar fatura por n�mero.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<InvoiceDto>>> GetByPersonIdAsync(int personId)
        {
            try
            {
                if (personId <= 0)
                    return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Cliente inv�lido.");

                var entities = await _invoiceRepository.GetByPersonIdAsync(personId);
                var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(entities);
                return OperationResult<IEnumerable<InvoiceDto>>.CreateSuccess(dtos, $"{dtos.Count()} fatura(s) do cliente.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Erro ao buscar por cliente.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<InvoiceDto>>> GetByStatusAsync(InvoiceStatus status)
        {
            try
            {
                var entities = await _invoiceRepository.GetByStatusAsync(status);
                var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(entities);
                return OperationResult<IEnumerable<InvoiceDto>>.CreateSuccess(dtos, $"{dtos.Count()} fatura(s) com status {status}.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Erro ao buscar por status.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<InvoiceDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1)
                    return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("P�gina deve ser >= 1.");
                if (pageSize < 1)
                    return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Tamanho da p�gina deve ser > 0.");

                var entities = await _invoiceRepository.GetPagedAsync(pageNumber, pageSize);
                var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(entities);
                return OperationResult<IEnumerable<InvoiceDto>>.CreateSuccess(dtos, $"P�gina {pageNumber} com {dtos.Count()} fatura(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Erro ao buscar paginado.", new[] { ex.Message });
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            try { return await _invoiceRepository.GetTotalCountAsync(); } catch { return 0; }
        }

        public async Task<bool> NumberExistsAsync(string invoiceNumber, int? excludeId = null)
        {
            try { return await _invoiceRepository.NumberExistsAsync(invoiceNumber, excludeId); } catch { return false; }
        }

        public async Task<OperationResult<InvoiceDto>> UpdateAsync(InvoiceDto invoice)
        {
            try
            {
                if (invoice == null)
                    return OperationResult<InvoiceDto>.CreateFailure("Fatura � obrigat�ria.");

                var validation = await _invoiceValidator.ValidateAsync(invoice);
                if (!validation.IsValid)
                    return OperationResult<InvoiceDto>.CreateFailure("Dados inv�lidos.", validation.Errors.Select(e => e.ErrorMessage));

                var existing = await _invoiceRepository.GetByIdAsync(invoice.InvoiceId);
                if (existing == null)
                    return OperationResult<InvoiceDto>.CreateFailure("Fatura n�o encontrada.");

                if (await _invoiceRepository.NumberExistsAsync(invoice.InvoiceNumber, invoice.InvoiceId))
                    return OperationResult<InvoiceDto>.CreateFailure("N�mero de fatura j� est� em uso.");

                _mapper.Map(invoice, existing);
                var updated = await _invoiceRepository.UpdateAsync(existing);
                var dto = _mapper.Map<InvoiceDto>(updated);
                return OperationResult<InvoiceDto>.CreateSuccess(dto, "Fatura atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<InvoiceDto>.CreateFailure("Erro ao atualizar fatura.", new[] { ex.Message });
            }
        }
    }
}
