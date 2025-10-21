using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    public interface IUserPrivilegeService
    {
        Task<UserPrivilegeDto> GetByIdAsync(int id);
        Task<IEnumerable<UserPrivilegeDto>> GetAllAsync();
        Task<IEnumerable<UserPrivilegeDto>> GetByUserAsync(int userId);
        Task<UserPrivilegeDto> GrantAsync(UserPrivilegeDto dto);
        Task<bool> RevokeAsync(int userPrivilegeId);
    }
}
