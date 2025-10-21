using AutoMapper;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    public class UserPrivilegeService : IUserPrivilegeService
    {
        private readonly IRepository<UserPrivilege> _repo;
        private readonly IMapper _mapper;
        public UserPrivilegeService(IRepository<UserPrivilege> repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

        public async Task<UserPrivilegeDto> GetByIdAsync(int id) => _mapper.Map<UserPrivilegeDto>(await _repo.GetByIdAsync(id));
        public async Task<IEnumerable<UserPrivilegeDto>> GetAllAsync() => _mapper.Map<IEnumerable<UserPrivilegeDto>>(await _repo.GetAllAsync());
        public async Task<IEnumerable<UserPrivilegeDto>> GetByUserAsync(int userId) => _mapper.Map<IEnumerable<UserPrivilegeDto>>(await _repo.FindAsync(up => up.UserId == userId));
        public async Task<UserPrivilegeDto> GrantAsync(UserPrivilegeDto dto) => _mapper.Map<UserPrivilegeDto>(await _repo.AddAsync(_mapper.Map<UserPrivilege>(dto)));
        public async Task<bool> RevokeAsync(int userPrivilegeId) => await _repo.DeleteAsync(userPrivilegeId);
    }
}
