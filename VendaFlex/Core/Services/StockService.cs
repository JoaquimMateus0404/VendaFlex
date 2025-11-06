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
    /// Servi�o para controle e movimenta��o de estoque.
    /// </summary>
    public class StockService : IStockService
    {
        private readonly StockRepository _stockRepository;
        private readonly IValidator<StockDto> _stockValidator;
        private readonly IMapper _mapper;

        public StockService(
            StockRepository stockRepository,
            IValidator<StockDto> stockValidator,
            IMapper mapper)
        {
            _stockRepository = stockRepository;
            _stockValidator = stockValidator;
            _mapper = mapper;
        }

        public async Task<OperationResult<StockDto>> AddAsync(StockDto stock)
        {
            try
            {
                if (stock == null)
                    return OperationResult<StockDto>.CreateFailure("Estoque � obrigat�rio.");

                var validation = await _stockValidator.ValidateAsync(stock);
                if (!validation.IsValid)
                {
                    return OperationResult<StockDto>.CreateFailure(
                        "Dados inv�lidos.",
                        validation.Errors.Select(e => e.ErrorMessage));
                }

                var exists = await _stockRepository.ExistsAsync(stock.ProductId);
                if (exists)
                    return OperationResult<StockDto>.CreateFailure("J� existe registro de estoque para este produto.");

                var entity = _mapper.Map<Stock>(stock);
                var created = await _stockRepository.AddAsync(entity);
                var resultDto = _mapper.Map<StockDto>(created);

                return OperationResult<StockDto>.CreateSuccess(resultDto, "Estoque criado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<StockDto>.CreateFailure(
                    "Erro ao criar estoque.",
                    new[] { ex.Message });
            }
        }

        public async Task<bool> DeleteAsync(int productId)
        {
            try
            {
                return await _stockRepository.DeleteAsync(productId);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(int productId)
        {
            try
            {
                return await _stockRepository.ExistsAsync(productId);
            }
            catch
            {
                return false;
            }
        }

        public async Task<OperationResult<IEnumerable<StockDto>>> GetAllAsync()
        {
            try
            {
                var entities = await _stockRepository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<StockDto>>(entities);
                return OperationResult<IEnumerable<StockDto>>.CreateSuccess(dtos, $"{dtos.Count()} registro(s) de estoque encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<StockDto>>.CreateFailure(
                    "Erro ao listar estoques.",
                    new[] { ex.Message });
            }
        }

        public async Task<int> GetAvailableQuantityAsync(int productId)
        {
            try
            {
                return await _stockRepository.GetAvailableQuantityAsync(productId);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<OperationResult<StockDto>> GetByProductIdAsync(int productId)
        {
            try
            {
                if (productId <= 0)
                    return OperationResult<StockDto>.CreateFailure("Produto inv�lido.");

                var entity = await _stockRepository.GetByProductIdAsync(productId);
                if (entity == null)
                    return OperationResult<StockDto>.CreateFailure("Estoque n�o encontrado.");

                var dto = _mapper.Map<StockDto>(entity);
                return OperationResult<StockDto>.CreateSuccess(dto, "Estoque encontrado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<StockDto>.CreateFailure(
                    "Erro ao buscar estoque.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<StockDto>>> GetLowStockAsync()
        {
            try
            {
                var entities = await _stockRepository.GetLowStockAsync();
                var dtos = _mapper.Map<IEnumerable<StockDto>>(entities);
                return OperationResult<IEnumerable<StockDto>>.CreateSuccess(dtos, $"{dtos.Count()} produto(s) com baixo estoque.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<StockDto>>.CreateFailure(
                    "Erro ao buscar baixo estoque.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<StockDto>>> GetOutOfStockAsync()
        {
            try
            {
                var entities = await _stockRepository.GetOutOfStockAsync();
                var dtos = _mapper.Map<IEnumerable<StockDto>>(entities);
                return OperationResult<IEnumerable<StockDto>>.CreateSuccess(dtos, $"{dtos.Count()} produto(s) sem estoque.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<StockDto>>.CreateFailure(
                    "Erro ao buscar produtos sem estoque.",
                    new[] { ex.Message });
            }
        }

        public async Task<bool> ReleaseReservedQuantityAsync(int productId, int quantity)
        {
            try
            {
                if (productId <= 0 || quantity <= 0)
                    return false;

                return await _stockRepository.ReleaseReservedQuantityAsync(productId, quantity);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ReserveQuantityAsync(int productId, int quantity)
        {
            try
            {
                if (productId <= 0 || quantity <= 0)
                    return false;

                return await _stockRepository.ReserveQuantityAsync(productId, quantity);
            }
            catch
            {
                return false;
            }
        }

        public async Task<OperationResult<StockDto>> UpdateAsync(StockDto stock)
        {
            try
            {
                if (stock == null)
                    return OperationResult<StockDto>.CreateFailure("Estoque � obrigat�rio.");

                var validation = await _stockValidator.ValidateAsync(stock);
                if (!validation.IsValid)
                {
                    return OperationResult<StockDto>.CreateFailure(
                        "Dados inv�lidos.",
                        validation.Errors.Select(e => e.ErrorMessage));
                }

                var existing = await _stockRepository.GetByProductIdAsync(stock.ProductId);
                if (existing == null)
                    return OperationResult<StockDto>.CreateFailure("Estoque n�o encontrado.");

                _mapper.Map(stock, existing);
                var updated = await _stockRepository.UpdateAsync(existing);
                var resultDto = _mapper.Map<StockDto>(updated);

                return OperationResult<StockDto>.CreateSuccess(resultDto, "Estoque atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<StockDto>.CreateFailure(
                    "Erro ao atualizar estoque.",
                    new[] { ex.Message });
            }
        }

        public async Task<bool> UpdateQuantityAsync(int productId, int quantity, int? userId = null)
        {
            try
            {
                if (productId <= 0 || quantity < 0)
                    return false;

                return await _stockRepository.UpdateQuantityAsync(productId, quantity, userId);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateQuantityAsync(int productId, int quantity, int? userId, string? notes)
        {
            try
            {
                if (productId <= 0 || quantity < 0)
                    return false;

                return await _stockRepository.UpdateQuantityAsync(productId, quantity, userId, notes);
            }
            catch
            {
                return false;
            }
        }
    }
}
