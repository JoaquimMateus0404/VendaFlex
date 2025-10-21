using AutoMapper;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço para registros de auditoria.
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly IRepository<AuditLog> _repo;
        private readonly IMapper _mapper;

        public AuditLogService(IRepository<AuditLog> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<AuditLogDto> GetByIdAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id);
            return _mapper.Map<AuditLogDto>(e);
        }

        public async Task<IEnumerable<AuditLogDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<AuditLogDto>>(list);
        }

        public async Task<IEnumerable<AuditLogDto>> GetByUserAsync(int userId)
        {
            var list = await _repo.FindAsync(l => l.UserId == userId);
            return _mapper.Map<IEnumerable<AuditLogDto>>(list.OrderByDescending(l => l.Timestamp));
        }

        public async Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityName, int entityId)
        {
            var list = await _repo.FindAsync(l => l.EntityName == entityName && l.EntityId == entityId);
            return _mapper.Map<IEnumerable<AuditLogDto>>(list.OrderByDescending(l => l.Timestamp));
        }

        public async Task<bool> RegisterLogAsync(AuditLogDto dto)
        {
            var e = _mapper.Map<AuditLog>(dto);
            await _repo.AddAsync(e);
            return true;
        }
    }
}
