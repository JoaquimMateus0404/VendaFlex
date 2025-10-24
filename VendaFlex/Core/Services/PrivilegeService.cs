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
    /// Servi�o de gerenciamento de privil�gios do sistema.
    /// Respons�vel por l�gica de neg�cio relacionada a privil�gios.
    /// USA APENAS O REPOSIT�RIO - n�o acessa DbContext diretamente.
    /// </summary>
    public class PrivilegeService : IPrivilegeService
    {
        private readonly PrivilegeRepository _repository;
        private readonly IMapper _mapper;
        private readonly IValidator<PrivilegeDto> _validator;

        public PrivilegeService(
            PrivilegeRepository repository,
            IMapper mapper,
            IValidator<PrivilegeDto> validator)
        {
            _repository = repository;
            _mapper = mapper;
            _validator = validator;
        }

        #region CRUD Operations

        public async Task<OperationResult<PrivilegeDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<PrivilegeDto>.CreateFailure("ID inv�lido.");

                var entity = await _repository.GetByIdAsync(id);

                if (entity == null)
                    return OperationResult<PrivilegeDto>.CreateFailure("Privil�gio n�o encontrado.");

                var dto = _mapper.Map<PrivilegeDto>(entity);
                return OperationResult<PrivilegeDto>.CreateSuccess(dto, "Privil�gio encontrado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PrivilegeDto>.CreateFailure("Erro ao buscar privil�gio.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PrivilegeDto>>> GetAllAsync()
        {
            try
            {
                var entities = await _repository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<PrivilegeDto>>(entities);

                return OperationResult<IEnumerable<PrivilegeDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} privil�gio(s) encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PrivilegeDto>>.CreateFailure("Erro ao listar privil�gios.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<PrivilegeDto>> CreateAsync(PrivilegeDto dto)
        {
            try
            {
                // Validar DTO
                var validationResult = await _validator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return OperationResult<PrivilegeDto>.CreateFailure(
                        "Dados inv�lidos.",
                        validationResult.Errors.Select(e => e.ErrorMessage));
                }

                // Verificar duplica��o de c�digo
                var codeExists = await _repository.CodeExistsAsync(dto.Code);
                if (codeExists)
                    return OperationResult<PrivilegeDto>.CreateFailure("C�digo de privil�gio j� est� em uso.");

                // Criar entidade
                var entity = _mapper.Map<Privilege>(dto);
                var created = await _repository.AddAsync(entity);
                var resultDto = _mapper.Map<PrivilegeDto>(created);

                return OperationResult<PrivilegeDto>.CreateSuccess(resultDto, "Privil�gio criado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PrivilegeDto>.CreateFailure("Erro ao criar privil�gio.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<PrivilegeDto>> UpdateAsync(PrivilegeDto dto)
        {
            try
            {
                // Validar DTO
                var validationResult = await _validator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return OperationResult<PrivilegeDto>.CreateFailure(
                        "Dados inv�lidos.",
                        validationResult.Errors.Select(e => e.ErrorMessage));
                }

                // Verificar se existe
                var existing = await _repository.GetByIdAsync(dto.PrivilegeId);
                if (existing == null)
                    return OperationResult<PrivilegeDto>.CreateFailure("Privil�gio n�o encontrado.");

                // Verificar duplica��o de c�digo
                if (existing.Code != dto.Code)
                {
                    var codeExists = await _repository.CodeExistsAsync(dto.Code, dto.PrivilegeId);
                    if (codeExists)
                        return OperationResult<PrivilegeDto>.CreateFailure("C�digo de privil�gio j� est� em uso.");
                }

                // Atualizar
                _mapper.Map(dto, existing);
                var updated = await _repository.UpdateAsync(existing);
                var resultDto = _mapper.Map<PrivilegeDto>(updated);

                return OperationResult<PrivilegeDto>.CreateSuccess(resultDto, "Privil�gio atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PrivilegeDto>.CreateFailure("Erro ao atualizar privil�gio.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult.CreateFailure("ID inv�lido.");

                var privilege = await _repository.GetByIdAsync(id);
                if (privilege == null)
                    return OperationResult.CreateFailure("Privil�gio n�o encontrado.");

                var deleted = await _repository.DeleteAsync(id);

                return deleted
                    ? OperationResult.CreateSuccess("Privil�gio removido com sucesso.")
                    : OperationResult.CreateFailure("N�o foi poss�vel remover o privil�gio.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure("Erro ao remover privil�gio.", new[] { ex.Message });
            }
        }

        #endregion

        #region Query Operations

        public async Task<OperationResult<IEnumerable<PrivilegeDto>>> GetActiveAsync()
        {
            try
            {
                var entities = await _repository.GetActiveAsync();
                var dtos = _mapper.Map<IEnumerable<PrivilegeDto>>(entities);

                return OperationResult<IEnumerable<PrivilegeDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} privil�gio(s) ativo(s) encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PrivilegeDto>>.CreateFailure("Erro ao buscar privil�gios ativos.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PrivilegeDto>>> GetInactiveAsync()
        {
            try
            {
                var entities = await _repository.GetInactiveAsync();
                var dtos = _mapper.Map<IEnumerable<PrivilegeDto>>(entities);

                return OperationResult<IEnumerable<PrivilegeDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} privil�gio(s) inativo(s) encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PrivilegeDto>>.CreateFailure("Erro ao buscar privil�gios inativos.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PrivilegeDto>>> SearchAsync(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                    return OperationResult<IEnumerable<PrivilegeDto>>.CreateFailure("Termo de busca � obrigat�rio.");

                var entities = await _repository.SearchAsync(term);
                var dtos = _mapper.Map<IEnumerable<PrivilegeDto>>(entities);

                return OperationResult<IEnumerable<PrivilegeDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} privil�gio(s) encontrado(s) para '{term}'.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PrivilegeDto>>.CreateFailure("Erro ao buscar privil�gios.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<PrivilegeDto>> GetByCodeAsync(string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return OperationResult<PrivilegeDto>.CreateFailure("C�digo � obrigat�rio.");

                var entity = await _repository.GetByCodeAsync(code);

                if (entity == null)
                    return OperationResult<PrivilegeDto>.CreateFailure("Privil�gio n�o encontrado.");

                var dto = _mapper.Map<PrivilegeDto>(entity);
                return OperationResult<PrivilegeDto>.CreateSuccess(dto, "Privil�gio encontrado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<PrivilegeDto>.CreateFailure("Erro ao buscar privil�gio por c�digo.", new[] { ex.Message });
            }
        }

        #endregion

        #region Pagination

        public async Task<OperationResult<IEnumerable<PrivilegeDto>>> GetPagedAsync(int page, int pageSize)
        {
            try
            {
                if (page < 1)
                    return OperationResult<IEnumerable<PrivilegeDto>>.CreateFailure("P�gina deve ser maior ou igual a 1.");

                if (pageSize < 1 || pageSize > 100)
                    return OperationResult<IEnumerable<PrivilegeDto>>.CreateFailure("Tamanho da p�gina deve estar entre 1 e 100.");

                var entities = await _repository.GetPagedAsync(page, pageSize);
                var dtos = _mapper.Map<IEnumerable<PrivilegeDto>>(entities);

                return OperationResult<IEnumerable<PrivilegeDto>>.CreateSuccess(
                    dtos,
                    $"P�gina {page} retornada com {dtos.Count()} privil�gio(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PrivilegeDto>>.CreateFailure("Erro ao buscar privil�gios paginados.", new[] { ex.Message });
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            try
            {
                return await _repository.GetTotalCountAsync();
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Status Operations

        public async Task<OperationResult> ActivateAsync(int id)
        {
            try
            {
                var privilege = await _repository.GetByIdAsync(id);
                if (privilege == null)
                    return OperationResult.CreateFailure("Privil�gio n�o encontrado.");

                if (privilege.IsActive)
                    return OperationResult.CreateFailure("Privil�gio j� est� ativo.");

                privilege.IsActive = true;
                await _repository.UpdateAsync(privilege);

                return OperationResult.CreateSuccess("Privil�gio ativado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure("Erro ao ativar privil�gio.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult> DeactivateAsync(int id)
        {
            try
            {
                var privilege = await _repository.GetByIdAsync(id);
                if (privilege == null)
                    return OperationResult.CreateFailure("Privil�gio n�o encontrado.");

                if (!privilege.IsActive)
                    return OperationResult.CreateFailure("Privil�gio j� est� inativo.");

                privilege.IsActive = false;
                await _repository.UpdateAsync(privilege);

                return OperationResult.CreateSuccess("Privil�gio desativado com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure("Erro ao desativar privil�gio.", new[] { ex.Message });
            }
        }

        #endregion

        #region Validation Operations

        public async Task<bool> CodeExistsAsync(string code, int? excludePrivilegeId = null)
        {
            try
            {
                return await _repository.CodeExistsAsync(code, excludePrivilegeId);
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}