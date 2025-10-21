using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    public interface IAuditLogService
    {
        Task<AuditLogDto> GetByIdAsync(int id);
        Task<IEnumerable<AuditLogDto>> GetAllAsync();
        Task<IEnumerable<AuditLogDto>> GetByUserAsync(int userId);
        Task<IEnumerable<AuditLogDto>> GetByEntityAsync(string entityName, int entityId);
        Task<bool> RegisterLogAsync(AuditLogDto dto);
    }
}
