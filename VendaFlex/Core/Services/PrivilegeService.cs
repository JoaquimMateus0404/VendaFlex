using AutoMapper;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    public class PrivilegeService : IPrivilegeService
    {
        private readonly IRepository<Privilege> _repo;
        private readonly IMapper _mapper;
        public PrivilegeService(IRepository<Privilege> repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

        public async Task<PrivilegeDto> GetByIdAsync(int id) => _mapper.Map<PrivilegeDto>(await _repo.GetByIdAsync(id));
        public async Task<IEnumerable<PrivilegeDto>> GetAllAsync() => _mapper.Map<IEnumerable<PrivilegeDto>>(await _repo.GetAllAsync());
        public async Task<IEnumerable<PrivilegeDto>> GetActiveAsync() => _mapper.Map<IEnumerable<PrivilegeDto>>(await _repo.FindAsync(p => p.IsActive));
        public async Task<IEnumerable<PrivilegeDto>> SearchAsync(string term) => _mapper.Map<IEnumerable<PrivilegeDto>>(await _repo.FindAsync(p => p.Name.Contains(term) || p.Code.Contains(term)));
        public async Task<PrivilegeDto> CreateAsync(PrivilegeDto dto) => _mapper.Map<PrivilegeDto>(await _repo.AddAsync(_mapper.Map<Privilege>(dto)));
        public async Task<PrivilegeDto> UpdateAsync(PrivilegeDto dto) => _mapper.Map<PrivilegeDto>(await _repo.UpdateAsync(_mapper.Map<Privilege>(dto)));
        public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);
    }
}
