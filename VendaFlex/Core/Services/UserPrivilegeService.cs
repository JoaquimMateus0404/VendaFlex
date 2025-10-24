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
    /// Servi�o de gerenciamento de privil�gios de usu�rios.
    /// Respons�vel por concess�o e revoga��o de privil�gios.
    /// USA APENAS REPOSIT�RIOS - n�o acessa DbContext diretamente.
    /// </summary>
    public class UserPrivilegeService : IUserPrivilegeService
    {
        private readonly UserPrivilegeRepository _repository;
        private readonly UserRepository _userRepository;
        private readonly PrivilegeRepository _privilegeRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<UserPrivilegeDto> _validator;

        public UserPrivilegeService(
            UserPrivilegeRepository repository,
            UserRepository userRepository,
            PrivilegeRepository privilegeRepository,
            IMapper mapper,
            IValidator<UserPrivilegeDto> validator)
        {
            _repository = repository;
            _userRepository = userRepository;
            _privilegeRepository = privilegeRepository;
            _mapper = mapper;
            _validator = validator;
        }

        #region CRUD Operations

        public async Task<OperationResult<UserPrivilegeDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<UserPrivilegeDto>.CreateFailure("ID inv�lido.");

                var entity = await _repository.GetByIdAsync(id);

                if (entity == null)
                    return OperationResult<UserPrivilegeDto>.CreateFailure("Privil�gio de usu�rio n�o encontrado.");

                var dto = _mapper.Map<UserPrivilegeDto>(entity);
                return OperationResult<UserPrivilegeDto>.CreateSuccess(dto);
            }
            catch (Exception ex)
            {
                return OperationResult<UserPrivilegeDto>.CreateFailure("Erro ao buscar privil�gio de usu�rio.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<UserPrivilegeDto>>> GetAllAsync()
        {
            try
            {
                var entities = await _repository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<UserPrivilegeDto>>(entities);

                return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} privil�gio(s) de usu�rio encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateFailure("Erro ao listar privil�gios.", new[] { ex.Message });
            }
        }

        #endregion

        #region Query Operations

        public async Task<OperationResult<IEnumerable<UserPrivilegeDto>>> GetByUserAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateFailure("ID do usu�rio inv�lido.");

                var entities = await _repository.GetByUserAsync(userId);
                var dtos = _mapper.Map<IEnumerable<UserPrivilegeDto>>(entities);

                return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} privil�gio(s) encontrado(s) para o usu�rio.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateFailure("Erro ao buscar privil�gios do usu�rio.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<UserPrivilegeDto>>> GetByPrivilegeAsync(int privilegeId)
        {
            try
            {
                if (privilegeId <= 0)
                    return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateFailure("ID do privil�gio inv�lido.");

                var entities = await _repository.GetByPrivilegeAsync(privilegeId);
                var dtos = _mapper.Map<IEnumerable<UserPrivilegeDto>>(entities);

                return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} usu�rio(s) com este privil�gio.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateFailure("Erro ao buscar usu�rios com o privil�gio.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PrivilegeDto>>> GetUserPrivilegesDetailsAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    return OperationResult<IEnumerable<PrivilegeDto>>.CreateFailure("ID do usu�rio inv�lido.");

                var entities = await _repository.GetUserPrivilegesDetailsAsync(userId);
                var dtos = _mapper.Map<IEnumerable<PrivilegeDto>>(entities);

                return OperationResult<IEnumerable<PrivilegeDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} privil�gio(s) detalhado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PrivilegeDto>>.CreateFailure("Erro ao buscar detalhes dos privil�gios.", new[] { ex.Message });
            }
        }

        #endregion

        #region Grant & Revoke Operations

        public async Task<OperationResult<UserPrivilegeDto>> GrantAsync(UserPrivilegeDto dto)
        {
            try
            {
                // Validar DTO
                var validationResult = await _validator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return OperationResult<UserPrivilegeDto>.CreateFailure(
                        "Dados inv�lidos.",
                        validationResult.Errors.Select(e => e.ErrorMessage));
                }

                // Verificar se usu�rio existe
                var userExists = await _userRepository.GetByIdAsync(dto.UserId);
                if (userExists == null)
                    return OperationResult<UserPrivilegeDto>.CreateFailure("Usu�rio n�o encontrado.");

                // Verificar se privil�gio existe
                var privilegeExists = await _privilegeRepository.GetByIdAsync(dto.PrivilegeId);
                if (privilegeExists == null)
                    return OperationResult<UserPrivilegeDto>.CreateFailure("Privil�gio n�o encontrado.");

                // Verificar se privil�gio est� ativo
                if (!privilegeExists.IsActive)
                    return OperationResult<UserPrivilegeDto>.CreateFailure("Privil�gio est� inativo.");

                // Verificar se j� existe
                var alreadyExists = await _repository.ExistsAsync(dto.UserId, dto.PrivilegeId);
                if (alreadyExists)
                    return OperationResult<UserPrivilegeDto>.CreateFailure("Usu�rio j� possui este privil�gio.");

                // Conceder privil�gio
                var entity = _mapper.Map<UserPrivilege>(dto);
                entity.GrantedAt = DateTime.UtcNow;

                var created = await _repository.AddAsync(entity);
                var resultDto = _mapper.Map<UserPrivilegeDto>(created);

                return OperationResult<UserPrivilegeDto>.CreateSuccess(resultDto, "Privil�gio concedido com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<UserPrivilegeDto>.CreateFailure("Erro ao conceder privil�gio.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult> GrantMultipleAsync(int userId, IEnumerable<int> privilegeIds, int? grantedByUserId = null)
        {
            try
            {
                if (userId <= 0)
                    return OperationResult.CreateFailure("ID do usu�rio inv�lido.");

                if (privilegeIds == null || !privilegeIds.Any())
                    return OperationResult.CreateFailure("Lista de privil�gios est� vazia.");

                // Verificar usu�rio
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return OperationResult.CreateFailure("Usu�rio n�o encontrado.");

                var granted = 0;
                var skipped = 0;
                var errors = new List<string>();

                foreach (var privilegeId in privilegeIds)
                {
                    // Verificar se privil�gio existe e est� ativo
                    var privilege = await _privilegeRepository.GetByIdAsync(privilegeId);
                    if (privilege == null || !privilege.IsActive)
                    {
                        skipped++;
                        continue;
                    }

                    // Verificar se j� existe
                    var exists = await _repository.ExistsAsync(userId, privilegeId);
                    if (exists)
                    {
                        skipped++;
                        continue;
                    }

                    // Conceder
                    var userPrivilege = new UserPrivilege
                    {
                        UserId = userId,
                        PrivilegeId = privilegeId,
                        GrantedAt = DateTime.UtcNow,
                        GrantedByUserId = grantedByUserId
                    };

                    await _repository.AddAsync(userPrivilege);
                    granted++;
                }

                return OperationResult.CreateSuccess($"{granted} privil�gio(s) concedido(s). {skipped} ignorado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure("Erro ao conceder privil�gios m�ltiplos.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult> RevokeAsync(int userPrivilegeId)
        {
            try
            {
                if (userPrivilegeId <= 0)
                    return OperationResult.CreateFailure("ID inv�lido.");

                var deleted = await _repository.DeleteAsync(userPrivilegeId);

                return deleted
                    ? OperationResult.CreateSuccess("Privil�gio revogado com sucesso.")
                    : OperationResult.CreateFailure("Privil�gio de usu�rio n�o encontrado.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure("Erro ao revogar privil�gio.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult> RevokeByUserAndPrivilegeAsync(int userId, int privilegeId)
        {
            try
            {
                if (userId <= 0)
                    return OperationResult.CreateFailure("ID do usu�rio inv�lido.");

                if (privilegeId <= 0)
                    return OperationResult.CreateFailure("ID do privil�gio inv�lido.");

                var deleted = await _repository.DeleteByUserAndPrivilegeAsync(userId, privilegeId);

                return deleted
                    ? OperationResult.CreateSuccess("Privil�gio revogado com sucesso.")
                    : OperationResult.CreateFailure("Usu�rio n�o possui este privil�gio.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure("Erro ao revogar privil�gio.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult> RevokeAllFromUserAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    return OperationResult.CreateFailure("ID do usu�rio inv�lido.");

                var count = await _repository.DeleteAllFromUserAsync(userId);

                return OperationResult.CreateSuccess($"{count} privil�gio(s) revogado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure("Erro ao revogar todos os privil�gios.", new[] { ex.Message });
            }
        }

        #endregion

        #region Verification Operations

        public async Task<bool> UserHasPrivilegeAsync(int userId, int privilegeId)
        {
            try
            {
                return await _repository.UserHasPrivilegeAsync(userId, privilegeId);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UserHasPrivilegeByCodeAsync(int userId, string privilegeCode)
        {
            try
            {
                return await _repository.UserHasPrivilegeByCodeAsync(userId, privilegeCode);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(int userId, int privilegeId)
        {
            try
            {
                return await _repository.ExistsAsync(userId, privilegeId);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Statistics

        public async Task<int> GetUserPrivilegeCountAsync(int userId)
        {
            try
            {
                return await _repository.GetUserPrivilegeCountAsync(userId);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<OperationResult<IEnumerable<(int UserId, int PrivilegeCount)>>> GetTopUsersWithMostPrivilegesAsync(int topCount = 10)
        {
            try
            {
                if (topCount < 1)
                    return OperationResult<IEnumerable<(int UserId, int PrivilegeCount)>>.CreateFailure("Quantidade deve ser maior que zero.");

                var results = await _repository.GetTopUsersWithMostPrivilegesAsync(topCount);

                return OperationResult<IEnumerable<(int UserId, int PrivilegeCount)>>.CreateSuccess(
                    results,
                    $"Top {results.Count()} usu�rios com mais privil�gios.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<(int UserId, int PrivilegeCount)>>.CreateFailure(
                    "Erro ao buscar estat�sticas.",
                    new[] { ex.Message });
            }
        }

        #endregion
    }
}