using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto> UpdateAsync(UserDto dto);
        Task<bool> DeleteAsync(int id);

        // Autenticação e credenciais
        Task<UserDto?> LoginAsync(string username, string password);
        Task<UserDto> RegisterAsync(UserDto dto, string password);
        Task<bool> ChangePasswordAsync(int userId, string newPassword);
        Task<bool> ResetPasswordAsync(string email);
        Task<bool> LogoutAsync(int userId);

        // Seguran�a de conta
        Task<bool> LockUserAsync(int userId);
        Task<bool> UnlockUserAsync(int userId);
    }
}
