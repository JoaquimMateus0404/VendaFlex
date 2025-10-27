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
        
        private readonly IMapper _mapper;

        public Task<IEnumerable<AuditLogDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityName, int entityId)
        {
            throw new NotImplementedException();
        }

        public Task<AuditLogDto> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<AuditLogDto>> GetByUserAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RegisterLogAsync(AuditLogDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
