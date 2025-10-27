using AutoMapper;
using FluentValidation;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço de categorias de produtos.
    /// Responsável por operações de criação, leitura, atualização, exclusão e consulta de categorias.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<CategoryDto> _categoryValidator;

        public CategoryService(CategoryRepository categoryRepository, IMapper mapper, IValidator<CategoryDto> categoryValidator)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _categoryValidator = categoryValidator;
        }

        /// <summary>
        /// Adiciona uma nova categoria ao sistema.
        /// </summary>
        public async Task<OperationResult<CategoryDto>> AddAsync(CategoryDto category)
        {
            try
            {
                if (category == null)
                    return OperationResult<CategoryDto>.CreateFailure("Categoria é obrigatória.");

                var validation = await _categoryValidator.ValidateAsync(category);
                if (!validation.IsValid)
                {
                    return OperationResult<CategoryDto>.CreateFailure(
                        "Dados inválidos.",
                        validation.Errors.Select(e => e.ErrorMessage));
                }

                if (!string.IsNullOrWhiteSpace(category.Code))
                {
                    var codeExists = await _categoryRepository.CodeExistsAsync(category.Code);
                    if (codeExists)
                        return OperationResult<CategoryDto>.CreateFailure("Código já está em uso.");
                }

                var entity = _mapper.Map<VendaFlex.Data.Entities.Category>(category);
                var created = await _categoryRepository.AddAsync(entity);
                var resultDto = _mapper.Map<CategoryDto>(created);

                return OperationResult<CategoryDto>.CreateSuccess(resultDto, "Categoria criada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<CategoryDto>.CreateFailure(
                    "Erro ao criar categoria.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Verifica se já existe uma categoria com o código informado.
        /// Pode excluir um ID específico da verificação (útil para edição).
        /// </summary>
        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
        {
            try
            {
                return await _categoryRepository.CodeExistsAsync(code, excludeId);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Remove uma categoria com base no seu ID.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                return await _categoryRepository.DeleteAsync(id);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica se uma categoria existe com base no ID.
        /// </summary>
        public async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _categoryRepository.ExistsAsync(id);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Retorna todas as categorias ativas (não arquivadas ou desativadas).
        /// </summary>
        public async Task<OperationResult<IEnumerable<CategoryDto>>> GetActiveAsync()
        {
            try
            {
                var entities = await _categoryRepository.GetActiveAsync();
                var dtos = _mapper.Map<IEnumerable<CategoryDto>>(entities);
                return OperationResult<IEnumerable<CategoryDto>>.CreateSuccess(dtos, $"{dtos.Count()} categoria(s) ativa(s) encontrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<CategoryDto>>.CreateFailure(
                    "Erro ao buscar categorias ativas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna todas as categorias cadastradas, ativas ou não.
        /// </summary>
        public async Task<OperationResult<IEnumerable<CategoryDto>>> GetAllAsync()
        {
            try
            {
                var entities = await _categoryRepository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<CategoryDto>>(entities);
                return OperationResult<IEnumerable<CategoryDto>>.CreateSuccess(dtos, $"{dtos.Count()} categoria(s) encontrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<CategoryDto>>.CreateFailure(
                    "Erro ao listar categorias.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Busca uma categoria pelo seu código único.
        /// </summary>
        public async Task<OperationResult<CategoryDto>> GetByCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return OperationResult<CategoryDto>.CreateFailure("Código é obrigatório.");

                var entity = await _categoryRepository.GetByCodeAsync(code);
                if (entity == null)
                    return OperationResult<CategoryDto>.CreateFailure("Categoria não encontrada.");

                var dto = _mapper.Map<CategoryDto>(entity);
                return OperationResult<CategoryDto>.CreateSuccess(dto, "Categoria encontrada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<CategoryDto>.CreateFailure(
                    "Erro ao buscar categoria por código.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Busca uma categoria pelo seu ID.
        /// </summary>
        public async Task<OperationResult<CategoryDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<CategoryDto>.CreateFailure("ID inválido.");

                var entity = await _categoryRepository.GetByIdAsync(id);
                if (entity == null)
                    return OperationResult<CategoryDto>.CreateFailure("Categoria não encontrada.");

                var dto = _mapper.Map<CategoryDto>(entity);
                return OperationResult<CategoryDto>.CreateSuccess(dto, "Categoria encontrada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<CategoryDto>.CreateFailure(
                    "Erro ao buscar categoria por ID.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna todas as categorias filhas de uma categoria pai específica.
        /// </summary>
        public async Task<IEnumerable<CategoryDto>> GetByParentIdAsync(int? parentId)
        {
            try
            {
                var entities = await _categoryRepository.GetByParentIdAsync(parentId);
                return _mapper.Map<IEnumerable<CategoryDto>>(entities);
            }
            catch
            {
                return Enumerable.Empty<CategoryDto>();
            }
        }

        /// <summary>
        /// Retorna uma lista paginada de categorias.
        /// </summary>
        public async Task<OperationResult<IEnumerable<CategoryDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1)
                    return OperationResult<IEnumerable<CategoryDto>>.CreateFailure("Página deve ser maior ou igual a 1.");
                if (pageSize < 1)
                    return OperationResult<IEnumerable<CategoryDto>>.CreateFailure("Tamanho da página deve ser maior que 0.");

                var entities = await _categoryRepository.GetPagedAsync(pageNumber, pageSize);
                var dtos = _mapper.Map<IEnumerable<CategoryDto>>(entities);
                return OperationResult<IEnumerable<CategoryDto>>.CreateSuccess(dtos, $"Página {pageNumber} com {dtos.Count()} categoria(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<CategoryDto>>.CreateFailure(
                    "Erro ao buscar categorias paginadas.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Retorna a quantidade de produtos associados a uma categoria.
        /// </summary>
        public async Task<int> GetProductCountAsync(int categoryId)
        {
            try
            {
                return await _categoryRepository.GetProductCountAsync(categoryId);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Retorna o número total de categorias cadastradas.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _categoryRepository.GetTotalCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Verifica se uma categoria possui produtos associados.
        /// </summary>
        public async Task<bool> HasProductsAsync(int categoryId)
        {
            try
            {
                return await _categoryRepository.HasProductsAsync(categoryId);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica se uma categoria possui subcategorias.
        /// </summary>
        public async Task<bool> HasSubcategoriesAsync(int categoryId)
        {
            try
            {
                return await _categoryRepository.HasChildCategoriesAsync(categoryId);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Realiza uma busca textual por categorias com base em um termo.
        /// </summary>
        public async Task<OperationResult<IEnumerable<CategoryDto>>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return OperationResult<IEnumerable<CategoryDto>>.CreateFailure("Termo de busca é obrigatório.");

                var entities = await _categoryRepository.SearchAsync(searchTerm);
                var dtos = _mapper.Map<IEnumerable<CategoryDto>>(entities);
                return OperationResult<IEnumerable<CategoryDto>>.CreateSuccess(dtos, $"{dtos.Count()} categoria(s) encontrada(s) para '{searchTerm}'.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<CategoryDto>>.CreateFailure(
                    "Erro ao buscar categorias.",
                    new[] { ex.Message });
            }
        }

        /// <summary>
        /// Atualiza os dados de uma categoria existente.
        /// </summary>
        public async Task<OperationResult<CategoryDto>> UpdateAsync(CategoryDto category)
        {
            try
            {
                if (category == null)
                    return OperationResult<CategoryDto>.CreateFailure("Categoria é obrigatória.");

                var validation = await _categoryValidator.ValidateAsync(category);
                if (!validation.IsValid)
                {
                    return OperationResult<CategoryDto>.CreateFailure(
                        "Dados inválidos.",
                        validation.Errors.Select(e => e.ErrorMessage));
                }

                var existing = await _categoryRepository.GetByIdAsync(category.CategoryId);
                if (existing == null)
                    return OperationResult<CategoryDto>.CreateFailure("Categoria não encontrada.");

                if (!string.IsNullOrWhiteSpace(category.Code))
                {
                    var codeExists = await _categoryRepository.CodeExistsAsync(category.Code, category.CategoryId);
                    if (codeExists)
                        return OperationResult<CategoryDto>.CreateFailure("Código já está em uso.");
                }

                _mapper.Map(category, existing);
                var updated = await _categoryRepository.UpdateAsync(existing);
                var resultDto = _mapper.Map<CategoryDto>(updated);

                return OperationResult<CategoryDto>.CreateSuccess(resultDto, "Categoria atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<CategoryDto>.CreateFailure(
                    "Erro ao atualizar categoria.",
                    new[] { ex.Message });
            }
        }
    }
}
