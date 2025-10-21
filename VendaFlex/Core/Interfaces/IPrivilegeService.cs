using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    public interface IPrivilegeService
    {
        Task<PrivilegeDto> GetByIdAsync(int id);
        Task<IEnumerable<PrivilegeDto>> GetAllAsync();
        Task<IEnumerable<PrivilegeDto>> GetActiveAsync();
        Task<IEnumerable<PrivilegeDto>> SearchAsync(string term);
        Task<PrivilegeDto> CreateAsync(PrivilegeDto dto);
        Task<PrivilegeDto> UpdateAsync(PrivilegeDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
