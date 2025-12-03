using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    public class InvoiceProductService : IInvoiceProductService
    {
        private readonly InvoiceProductRepository _invoiceProductRepository;
        private readonly IValidator<InvoiceProductDto> _invoiceProductValidator;
        private readonly IMapper _mapper;
        public InvoiceProductService(
            InvoiceProductRepository invoiceProductRepository,
            IValidator<InvoiceProductDto> invoiceProductValidator,
            IMapper mapper)
        {
            _invoiceProductRepository = invoiceProductRepository;
            _invoiceProductValidator = invoiceProductValidator;
            _mapper = mapper;
        }

        public async Task<OperationResult<InvoiceProductDto>> AddAsync(InvoiceProductDto item)
        {
            try
            {
                if (item == null)
                    return OperationResult<InvoiceProductDto>.CreateFailure("Item de fatura é obrigatório.");

                var validation = await _invoiceProductValidator.ValidateAsync(item);
                if (!validation.IsValid)
                    return OperationResult<InvoiceProductDto>.CreateFailure("Dados inválidos.", validation.Errors.Select(e => e.ErrorMessage));

                var duplicate = await _invoiceProductRepository.ExistsProductInInvoiceAsync(item.InvoiceId, item.ProductId);
                if (duplicate)
                    return OperationResult<InvoiceProductDto>.CreateFailure("Produto já adicionado nesta fatura.");

                var entity = _mapper.Map<VendaFlex.Data.Entities.InvoiceProduct>(item);
                var created = await _invoiceProductRepository.AddAsync(entity);
                var dto = _mapper.Map<InvoiceProductDto>(created);
                return OperationResult<InvoiceProductDto>.CreateSuccess(dto, "Item adicionado à fatura com sucesso.");
            }
            catch (DbUpdateException ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return OperationResult<InvoiceProductDto>.CreateFailure("Erro ao adicionar item à fatura.", new[] { $"DbUpdateException: {innerMessage}" });
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return OperationResult<InvoiceProductDto>.CreateFailure("Erro ao adicionar item à fatura.", new[] { innerMessage });
            }
        }

        public async Task<OperationResult<bool>> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<bool>.CreateFailure("ID inválido.");

                var exists = await _invoiceProductRepository.ExistsAsync(id);
                if (!exists)
                    return OperationResult<bool>.CreateFailure("Item da fatura não encontrado.");

                var deleted = await _invoiceProductRepository.DeleteAsync(id);
                return deleted
                    ? OperationResult<bool>.CreateSuccess(true, "Item removido com sucesso.")
                    : OperationResult<bool>.CreateFailure("Não foi possível remover o item.");
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.CreateFailure("Erro ao remover item da fatura.", new[] { ex.Message });
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try { return await _invoiceProductRepository.ExistsAsync(id); } catch { return false; }
        }

        public async Task<bool> ExistsProductInInvoiceAsync(int invoiceId, int productId, int? excludeId = null)
        {
            try { return await _invoiceProductRepository.ExistsProductInInvoiceAsync(invoiceId, productId, excludeId); } catch { return false; }
        }

        public async Task<OperationResult<InvoiceProductDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<InvoiceProductDto>.CreateFailure("ID inválido.");

                var entity = await _invoiceProductRepository.GetByIdAsync(id);
                if (entity == null)
                    return OperationResult<InvoiceProductDto>.CreateFailure("Item da fatura não encontrado.");

                var dto = _mapper.Map<InvoiceProductDto>(entity);
                return OperationResult<InvoiceProductDto>.CreateSuccess(dto, "Item encontrado.");
            }
            catch (Exception ex)
            {
                return OperationResult<InvoiceProductDto>.CreateFailure("Erro ao buscar item.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<InvoiceProductDto>>> GetByInvoiceIdAsync(int invoiceId)
        {
            try
            {
                if (invoiceId <= 0)
                    return OperationResult<IEnumerable<InvoiceProductDto>>.CreateFailure("Fatura inválida.");

                var entities = await _invoiceProductRepository.GetByInvoiceIdAsync(invoiceId);
                var dtos = _mapper.Map<IEnumerable<InvoiceProductDto>>(entities);
                return OperationResult<IEnumerable<InvoiceProductDto>>.CreateSuccess(dtos, $"{dtos.Count()} item(ns) da fatura.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<InvoiceProductDto>>.CreateFailure("Erro ao listar itens da fatura.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<InvoiceProductDto>> UpdateAsync(InvoiceProductDto item)
        {
            try
            {
                if (item == null)
                    return OperationResult<InvoiceProductDto>.CreateFailure("Item de fatura é obrigatório.");

                var validation = await _invoiceProductValidator.ValidateAsync(item);
                if (!validation.IsValid)
                    return OperationResult<InvoiceProductDto>.CreateFailure("Dados inválidos.", validation.Errors.Select(e => e.ErrorMessage));

                var existing = await _invoiceProductRepository.GetByIdAsync(item.InvoiceProductId);
                if (existing == null)
                    return OperationResult<InvoiceProductDto>.CreateFailure("Item da fatura não encontrado.");

                var duplicate = await _invoiceProductRepository.ExistsProductInInvoiceAsync(item.InvoiceId, item.ProductId, item.InvoiceProductId);
                if (duplicate)
                    return OperationResult<InvoiceProductDto>.CreateFailure("Produto já adicionado nesta fatura.");

                // Map to new entity instance and update
                var entity = _mapper.Map<VendaFlex.Data.Entities.InvoiceProduct>(item);
                var updated = await _invoiceProductRepository.UpdateAsync(entity);
                var dto = _mapper.Map<InvoiceProductDto>(updated);
                return OperationResult<InvoiceProductDto>.CreateSuccess(dto, "Item atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<InvoiceProductDto>.CreateFailure("Erro ao atualizar item da fatura.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<TopProductDto>>> GetTopSellingProductsAsync(int top)
        {
            try
            {
                if (top <= 0) top = 5;
                var data = await _invoiceProductRepository.GetTopSellingProductsAsync(top);
                return OperationResult<IEnumerable<TopProductDto>>.CreateSuccess(data, $"Top {data.Count()} produtos carregados.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<TopProductDto>>.CreateFailure("Erro ao obter produtos mais vendidos.", new[] { ex.Message });
            }
        }
    }
}
