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
    /// Serviço de gerenciamento de privilégios de usuários.
    /// Responsável por concessão e revogação de privilégios.
    /// USA APENAS REPOSITÓRIOS - não acessa DbContext diretamente.
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
                    return OperationResult<UserPrivilegeDto>.CreateFailure("ID inválido.");

                var entity = await _repository.GetByIdAsync(id);

                if (entity == null)
                    return OperationResult<UserPrivilegeDto>.CreateFailure("Privilégio de usuário não encontrado.");

                var dto = _mapper.Map<UserPrivilegeDto>(entity);
                return OperationResult<UserPrivilegeDto>.CreateSuccess(dto);
            }
            catch (Exception ex)
            {
                return OperationResult<UserPrivilegeDto>.CreateFailure("Erro ao buscar privilégio de usuário.", new[] { ex.Message });
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
                    $"{dtos.Count()} privilégio(s) de usuário encontrado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateFailure("Erro ao listar privilégios.", new[] { ex.Message });
            }
        }

        #endregion

        #region Query Operations

        public async Task<OperationResult<IEnumerable<UserPrivilegeDto>>> GetByUserAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateFailure("ID do usuário inválido.");

                var entities = await _repository.GetByUserAsync(userId);
                var dtos = _mapper.Map<IEnumerable<UserPrivilegeDto>>(entities);

                return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} privilégio(s) encontrado(s) para o usuário.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateFailure("Erro ao buscar privilégios do usuário.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<UserPrivilegeDto>>> GetByPrivilegeAsync(int privilegeId)
        {
            try
            {
                if (privilegeId <= 0)
                    return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateFailure("ID do privilégio inválido.");

                var entities = await _repository.GetByPrivilegeAsync(privilegeId);
                var dtos = _mapper.Map<IEnumerable<UserPrivilegeDto>>(entities);

                return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} usuário(s) com este privilégio.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<UserPrivilegeDto>>.CreateFailure("Erro ao buscar usuários com o privilégio.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<PrivilegeDto>>> GetUserPrivilegesDetailsAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    return OperationResult<IEnumerable<PrivilegeDto>>.CreateFailure("ID do usuário inválido.");

                var entities = await _repository.GetUserPrivilegesDetailsAsync(userId);
                var dtos = _mapper.Map<IEnumerable<PrivilegeDto>>(entities);

                return OperationResult<IEnumerable<PrivilegeDto>>.CreateSuccess(
                    dtos,
                    $"{dtos.Count()} privilégio(s) detalhado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<PrivilegeDto>>.CreateFailure("Erro ao buscar detalhes dos privilégios.", new[] { ex.Message });
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
                        "Dados inválidos.",
                        validationResult.Errors.Select(e => e.ErrorMessage));
                }

                // Verificar se usuário existe
                var userExists = await _userRepository.GetByIdAsync(dto.UserId);
                if (userExists == null)
                    return OperationResult<UserPrivilegeDto>.CreateFailure("Usuário não encontrado.");

                // Verificar se privilégio existe
                var privilegeExists = await _privilegeRepository.GetByIdAsync(dto.PrivilegeId);
                if (privilegeExists == null)
                    return OperationResult<UserPrivilegeDto>.CreateFailure("Privilégio não encontrado.");

                // Verificar se privilégio está ativo
                if (!privilegeExists.IsActive)
                    return OperationResult<UserPrivilegeDto>.CreateFailure("Privilégio está inativo.");

                // Verificar se já existe
                var alreadyExists = await _repository.ExistsAsync(dto.UserId, dto.PrivilegeId);
                if (alreadyExists)
                    return OperationResult<UserPrivilegeDto>.CreateFailure("Usuário já possui este privilégio.");

                // Conceder privilégio
                var entity = _mapper.Map<UserPrivilege>(dto);
                entity.GrantedAt = DateTime.UtcNow;

                var created = await _repository.AddAsync(entity);
                var resultDto = _mapper.Map<UserPrivilegeDto>(created);

                return OperationResult<UserPrivilegeDto>.CreateSuccess(resultDto, "Privilégio concedido com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<UserPrivilegeDto>.CreateFailure("Erro ao conceder privilégio.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult> GrantMultipleAsync(int userId, IEnumerable<int> privilegeIds, int? grantedByUserId = null)
        {
            try
            {
                if (userId <= 0)
                    return OperationResult.CreateFailure("ID do usuário inválido.");

                if (privilegeIds == null || !privilegeIds.Any())
                    return OperationResult.CreateFailure("Lista de privilégios está vazia.");

                // Verificar usuário
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return OperationResult.CreateFailure("Usuário não encontrado.");

                var granted = 0;
                var skipped = 0;
                var errors = new List<string>();

                foreach (var privilegeId in privilegeIds)
                {
                    // Verificar se privilégio existe e está ativo
                    var privilege = await _privilegeRepository.GetByIdAsync(privilegeId);
                    if (privilege == null || !privilege.IsActive)
                    {
                        skipped++;
                        continue;
                    }

                    // Verificar se já existe
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

                return OperationResult.CreateSuccess($"{granted} privilégio(s) concedido(s). {skipped} ignorado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure("Erro ao conceder privilégios múltiplos.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult> RevokeAsync(int userPrivilegeId)
        {
            try
            {
                if (userPrivilegeId <= 0)
                    return OperationResult.CreateFailure("ID inválido.");

                var deleted = await _repository.DeleteAsync(userPrivilegeId);

                return deleted
                    ? OperationResult.CreateSuccess("Privilégio revogado com sucesso.")
                    : OperationResult.CreateFailure("Privilégio de usuário não encontrado.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure("Erro ao revogar privilégio.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult> RevokeByUserAndPrivilegeAsync(int userId, int privilegeId)
        {
            try
            {
                if (userId <= 0)
                    return OperationResult.CreateFailure("ID do usuário inválido.");

                if (privilegeId <= 0)
                    return OperationResult.CreateFailure("ID do privilégio inválido.");

                var deleted = await _repository.DeleteByUserAndPrivilegeAsync(userId, privilegeId);

                return deleted
                    ? OperationResult.CreateSuccess("Privilégio revogado com sucesso.")
                    : OperationResult.CreateFailure("Usuário não possui este privilégio.");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure("Erro ao revogar privilégio.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult> RevokeAllFromUserAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    return OperationResult.CreateFailure("ID do usuário inválido.");

                var count = await _repository.DeleteAllFromUserAsync(userId);

                return OperationResult.CreateSuccess($"{count} privilégio(s) revogado(s).");
            }
            catch (Exception ex)
            {
                return OperationResult.CreateFailure("Erro ao revogar todos os privilégios.", new[] { ex.Message });
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
                    $"Top {results.Count()} usuários com mais privilégios.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<(int UserId, int PrivilegeCount)>>.CreateFailure(
                    "Erro ao buscar estatísticas.",
                    new[] { ex.Message });
            }
        }

        #endregion
    }
}