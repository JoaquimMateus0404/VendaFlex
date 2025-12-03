using System.Diagnostics;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço para gestão de produtos, incluindo operações de estoque e pesquisa.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly ProductRepository _productRepository;
        private readonly StockRepository _stockRepository;
        private readonly IValidator<ProductDto> _productValidator;
        private readonly IValidator<StockDto> _stockValidator;
        private readonly IMapper _mapper;

        public ProductService(
            ProductRepository productRepository,
            StockRepository stockRepository,
            IValidator<ProductDto> productValidator,
            IValidator<StockDto> stockValidator,
            IMapper mapper)
        {
            _productRepository = productRepository;
            _stockRepository = stockRepository;
            _productValidator = productValidator;
            _stockValidator = stockValidator;
            _mapper = mapper;
        }

        public async Task<OperationResult<ProductDto>> AddAsync(ProductDto product)
        {
            try
            {
                if (product == null)
                    return OperationResult<ProductDto>.CreateFailure("Produto é obrigatório.");

                var validation = await _productValidator.ValidateAsync(product);
                if (!validation.IsValid)
                {
                    Debug.WriteLine($"Erro ao cadastrar produto: {string.Join(", ", validation.Errors.Select(e => e.ErrorMessage))}");
                    return OperationResult<ProductDto>.CreateFailure(
                        "Dados inválidos.",
                        validation.Errors.Select(e => e.ErrorMessage));
                }

                // Verificações de duplicidade
                if (!string.IsNullOrWhiteSpace(product.Barcode))
                {
                    var barcodeExists = await _productRepository.BarcodeExistsAsync(product.Barcode);
                    if (barcodeExists)
                        return OperationResult<ProductDto>.CreateFailure("Código de barras já está em uso.");
                }

                if (!string.IsNullOrWhiteSpace(product.SKU))
                {
                    var skuExists = await _productRepository.SKUExistsAsync(product.SKU);
                    if (skuExists)
                        return OperationResult<ProductDto>.CreateFailure("SKU já está em uso.");
                }

                if (!string.IsNullOrWhiteSpace(product.Barcode))
                {
                    // Observação: repositório usa Barcode como Code internamente
                    var codeExists = await _productRepository.CodeExistsAsync(product.Barcode);
                    if (codeExists)
                        return OperationResult<ProductDto>.CreateFailure("Código já está em uso.");
                }

                var entity = _mapper.Map<Product>(product);
                var created = await _productRepository.AddAsync(entity);
                var resultDto = _mapper.Map<ProductDto>(created);

                return OperationResult<ProductDto>.CreateSuccess(resultDto, "Produto cadastrado com sucesso.");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                Debug.WriteLine($"[PRODUCT SERVICE] DbUpdateException: {dbEx.Message}");
                Debug.WriteLine($"[PRODUCT SERVICE] InnerException: {dbEx.InnerException?.Message}");
                
                var errorMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                
                // Extrair mensagem mais amigável
                if (errorMsg.Contains("FOREIGN KEY constraint"))
                {
                    if (errorMsg.Contains("FK_Products_Categories"))
                        return OperationResult<ProductDto>.CreateFailure("Categoria inválida ou não encontrada.");
                    if (errorMsg.Contains("FK_Products_Suppliers") || errorMsg.Contains("FK_Products_People"))
                        return OperationResult<ProductDto>.CreateFailure("Fornecedor inválido ou não encontrado.");
                }
                
                if (errorMsg.Contains("UNIQUE constraint") || errorMsg.Contains("duplicate"))
                {
                    if (errorMsg.Contains("Barcode"))
                        return OperationResult<ProductDto>.CreateFailure("Código de barras já existe.");
                    if (errorMsg.Contains("SKU"))
                        return OperationResult<ProductDto>.CreateFailure("SKU já existe.");
                    if (errorMsg.Contains("Code"))
                        return OperationResult<ProductDto>.CreateFailure("Código já existe.");
                }
                
                return OperationResult<ProductDto>.CreateFailure(
                    "Erro ao cadastrar produto no banco de dados.",
                    new[] { errorMsg });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PRODUCT SERVICE] Exception: {ex.GetType().Name}");
                Debug.WriteLine($"[PRODUCT SERVICE] Message: {ex.Message}");
                Debug.WriteLine($"[PRODUCT SERVICE] StackTrace: {ex.StackTrace}");
                
                return OperationResult<ProductDto>.CreateFailure(
                    "Erro ao cadastrar produto.",
                    new[] { ex.Message });
            }
        }

        public async Task<bool> BarcodeExistsAsync(string barcode, int? excludeId = null)
        {
            try
            {
                return await _productRepository.BarcodeExistsAsync(barcode, excludeId);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                return await _productRepository.CodeExistsAsync(code, excludeId);
            }
            catch
            {
                return false;
            }
        }

        public async Task<OperationResult<bool>> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<bool>.CreateFailure("ID inválido.");

                var exists = await _productRepository.ExistsAsync(id);
                if (!exists)
                    return OperationResult<bool>.CreateFailure("Produto não encontrado.");

                var deleted = await _productRepository.DeleteAsync(id);

                return deleted
                    ? OperationResult<bool>.CreateSuccess(true, "Produto removido com sucesso.")
                    : OperationResult<bool>.CreateFailure("Não foi possível remover o produto.");
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.CreateFailure(
                    "Erro ao remover produto.",
                    new[] { ex.Message });
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _productRepository.ExistsAsync(id);
            }
            catch
            {
                return false;
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetActiveAsync()
        {
            try
            {
                var entities = await _productRepository.GetActiveAsync();
                var dtos = _mapper.Map<IEnumerable<ProductDto>>(entities);
                return OperationResult<IEnumerable<ProductDto>>.CreateSuccess(dtos, $"{dtos.Count()} produto(s) ativo(s) encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ProductDto>>.CreateFailure(
                    "Erro ao buscar produtos ativos.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetAllAsync()
        {
            try
            {
                var entities = await _productRepository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<ProductDto>>(entities);
                return OperationResult<IEnumerable<ProductDto>>.CreateSuccess(dtos, $"{dtos.Count()} produto(s) encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ProductDto>>.CreateFailure(
                    "Erro ao listar produtos.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<ProductDto>> GetByBarcodeAsync(string barcode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(barcode))
                    return OperationResult<ProductDto>.CreateFailure("Código de barras é obrigatório.");

                var entity = await _productRepository.GetByBarcodeAsync(barcode);
                if (entity == null)
                    return OperationResult<ProductDto>.CreateFailure("Produto não encontrado.");

                var dto = _mapper.Map<ProductDto>(entity);
                return OperationResult<ProductDto>.CreateSuccess(dto, "Produto encontrado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<ProductDto>.CreateFailure(
                    "Erro ao buscar produto por código de barras.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetByCategoryIdAsync(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                    return OperationResult<IEnumerable<ProductDto>>.CreateFailure("Categoria inválida.");

                var entities = await _productRepository.GetByCategoryIdAsync(categoryId);
                var dtos = _mapper.Map<IEnumerable<ProductDto>>(entities);
                return OperationResult<IEnumerable<ProductDto>>.CreateSuccess(dtos, $"{dtos.Count()} produto(s) encontrado(s) na categoria.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ProductDto>>.CreateFailure(
                    "Erro ao buscar produtos por categoria.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<ProductDto>> GetByCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return OperationResult<ProductDto>.CreateFailure("Código é obrigatório.");

                var entity = await _productRepository.GetByCodeAsync(code);
                if (entity == null)
                    return OperationResult<ProductDto>.CreateFailure("Produto não encontrado.");

                var dto = _mapper.Map<ProductDto>(entity);
                return OperationResult<ProductDto>.CreateSuccess(dto, "Produto encontrado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<ProductDto>.CreateFailure(
                    "Erro ao buscar produto por código.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<ProductDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<ProductDto>.CreateFailure("ID inválido.");

                var entity = await _productRepository.GetByIdAsync(id);
                if (entity == null)
                    return OperationResult<ProductDto>.CreateFailure("Produto não encontrado.");

                var dto = _mapper.Map<ProductDto>(entity);
                return OperationResult<ProductDto>.CreateSuccess(dto, "Produto encontrado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<ProductDto>.CreateFailure(
                    "Erro ao buscar produto por ID.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            try
            {
                if (minPrice < 0 || maxPrice < 0)
                    return OperationResult<IEnumerable<ProductDto>>.CreateFailure("Preços devem ser maiores ou iguais a 0.");
                if (minPrice > maxPrice)
                    return OperationResult<IEnumerable<ProductDto>>.CreateFailure("Preço mínimo não pode ser maior que o máximo.");

                var entities = await _productRepository.GetByPriceRangeAsync(minPrice, maxPrice);
                var dtos = _mapper.Map<IEnumerable<ProductDto>>(entities);
                return OperationResult<IEnumerable<ProductDto>>.CreateSuccess(dtos, $"{dtos.Count()} produto(s) no intervalo de preço.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ProductDto>>.CreateFailure(
                    "Erro ao buscar produtos por preço.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<ProductDto>> GetBySKUAsync(string sku)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sku))
                    return OperationResult<ProductDto>.CreateFailure("SKU é obrigatório.");

                var entity = await _productRepository.GetBySKUAsync(sku);
                if (entity == null)
                    return OperationResult<ProductDto>.CreateFailure("Produto não encontrado.");

                var dto = _mapper.Map<ProductDto>(entity);
                return OperationResult<ProductDto>.CreateSuccess(dto, "Produto encontrado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<ProductDto>.CreateFailure(
                    "Erro ao buscar produto por SKU.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetByStatusAsync(ProductStatus status)
        {
            try
            {
                var entities = await _productRepository.GetByStatusAsync(status);
                var dtos = _mapper.Map<IEnumerable<ProductDto>>(entities);
                return OperationResult<IEnumerable<ProductDto>>.CreateSuccess(dtos, $"{dtos.Count()} produto(s) com status {status}.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ProductDto>>.CreateFailure(
                    "Erro ao buscar produtos por status.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetBySupplierIdAsync(int supplierId)
        {
            try
            {
                if (supplierId <= 0)
                    return OperationResult<IEnumerable<ProductDto>>.CreateFailure("Fornecedor inválido.");

                var entities = await _productRepository.GetBySupplierIdAsync(supplierId);
                var dtos = _mapper.Map<IEnumerable<ProductDto>>(entities);
                return OperationResult<IEnumerable<ProductDto>>.CreateSuccess(dtos, $"{dtos.Count()} produto(s) encontrado(s) para o fornecedor.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ProductDto>>.CreateFailure(
                    "Erro ao buscar produtos por fornecedor.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetFeaturedAsync()
        {
            try
            {
                var entities = await _productRepository.GetFeaturedAsync();
                var dtos = _mapper.Map<IEnumerable<ProductDto>>(entities);
                return OperationResult<IEnumerable<ProductDto>>.CreateSuccess(dtos, $"{dtos.Count()} produto(s) em destaque.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ProductDto>>.CreateFailure(
                    "Erro ao buscar produtos em destaque.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetLowStockAsync()
        {
            try
            {
                var entities = await _productRepository.GetLowStockAsync();
                var dtos = _mapper.Map<IEnumerable<ProductDto>>(entities);
                return OperationResult<IEnumerable<ProductDto>>.CreateSuccess(dtos, $"{dtos.Count()} produto(s) com baixo estoque.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ProductDto>>.CreateFailure(
                    "Erro ao buscar produtos com baixo estoque.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetOutOfStockAsync()
        {
            try
            {
                var entities = await _productRepository.GetOutOfStockAsync();
                var dtos = _mapper.Map<IEnumerable<ProductDto>>(entities);
                return OperationResult<IEnumerable<ProductDto>>.CreateSuccess(dtos, $"{dtos.Count()} produto(s) sem estoque.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ProductDto>>.CreateFailure(
                    "Erro ao buscar produtos sem estoque.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1)
                    return OperationResult<IEnumerable<ProductDto>>.CreateFailure("Página deve ser maior ou igual a 1.");
                if (pageSize < 1)
                    return OperationResult<IEnumerable<ProductDto>>.CreateFailure("Tamanho da página deve ser maior que 0.");

                var entities = await _productRepository.GetPagedAsync(pageNumber, pageSize);
                var dtos = _mapper.Map<IEnumerable<ProductDto>>(entities);
                return OperationResult<IEnumerable<ProductDto>>.CreateSuccess(dtos, $"Página {pageNumber} retornada com {dtos.Count()} produto(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ProductDto>>.CreateFailure(
                    "Erro ao buscar produtos paginados.",
                    new[] { ex.Message });
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _productRepository.GetTotalCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> GetWithExpirationAsync()
        {
            try
            {
                var entities = await _productRepository.GetWithExpirationAsync();
                var dtos = _mapper.Map<IEnumerable<ProductDto>>(entities);
                return OperationResult<IEnumerable<ProductDto>>.CreateSuccess(dtos, $"{dtos.Count()} produto(s) com controle de validade.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ProductDto>>.CreateFailure(
                    "Erro ao buscar produtos com validade.",
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<ProductDto>>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return OperationResult<IEnumerable<ProductDto>>.CreateFailure("Termo de busca é obrigatório.");

                var entities = await _productRepository.SearchAsync(searchTerm);
                var dtos = _mapper.Map<IEnumerable<ProductDto>>(entities);
                return OperationResult<IEnumerable<ProductDto>>.CreateSuccess(dtos, $"{dtos.Count()} produto(s) encontrado(s) para '{searchTerm}'.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<ProductDto>>.CreateFailure(
                    "Erro ao buscar produtos.",
                    new[] { ex.Message });
            }
        }

        public async Task<bool> SKUExistsAsync(string sku, int? excludeId = null)
        {
            try
            {
                return await _productRepository.SKUExistsAsync(sku, excludeId);
            }
            catch
            {
                return false;
            }
        }

        public async Task<OperationResult<ProductDto>> UpdateAsync(ProductDto product)
        {
            try
            {
                if (product == null)
                    return OperationResult<ProductDto>.CreateFailure("Produto é obrigatório.");

                var validation = await _productValidator.ValidateAsync(product);
                if (!validation.IsValid)
                {
                    return OperationResult<ProductDto>.CreateFailure(
                        "Dados inválidos.",
                        validation.Errors.Select(e => e.ErrorMessage));
                }

                var existing = await _productRepository.GetByIdAsync(product.ProductId);
                if (existing == null)
                    return OperationResult<ProductDto>.CreateFailure("Produto não encontrado.");

                // Verificações de duplicidade (excluindo o próprio ID)
                if (!string.IsNullOrWhiteSpace(product.Barcode))
                {
                    var barcodeExists = await _productRepository.BarcodeExistsAsync(product.Barcode, product.ProductId);
                    if (barcodeExists)
                        return OperationResult<ProductDto>.CreateFailure("Código de barras já está em uso.");
                }

                if (!string.IsNullOrWhiteSpace(product.SKU))
                {
                    var skuExists = await _productRepository.SKUExistsAsync(product.SKU, product.ProductId);
                    if (skuExists)
                        return OperationResult<ProductDto>.CreateFailure("SKU já está em uso.");
                }

                if (!string.IsNullOrWhiteSpace(product.Barcode))
                {
                    var codeExists = await _productRepository.CodeExistsAsync(product.Barcode, product.ProductId);
                    if (codeExists)
                        return OperationResult<ProductDto>.CreateFailure("Código já está em uso.");
                }

                _mapper.Map(product, existing);
                var updated = await _productRepository.UpdateAsync(existing);
                var resultDto = _mapper.Map<ProductDto>(updated);

                return OperationResult<ProductDto>.CreateSuccess(resultDto, "Produto atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<ProductDto>.CreateFailure(
                    "Erro ao atualizar produto.",
                    new[] { ex.Message });
            }
        }
    }
}
