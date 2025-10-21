using AutoMapper;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço de usuários: autenticação, gestão e segurança.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IRepository<User> _users;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _db;

        public UserService(IRepository<User> users, IMapper mapper, ApplicationDbContext db)
        {
            _users = users;
            _mapper = mapper;
            _db = db;
        }

        public async Task<UserDto> GetByIdAsync(int id)
        {
            var entity = await _users.GetByIdAsync(id);
            return _mapper.Map<UserDto>(entity);
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var list = await _users.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(list);
        }

        public async Task<UserDto> UpdateAsync(UserDto dto)
        {
            var entity = _mapper.Map<User>(dto);
            var updated = await _users.UpdateAsync(entity);
            return _mapper.Map<UserDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _users.DeleteAsync(id);
        }

        // Autenticação e credenciais
        public async Task<UserDto?> LoginAsync(string username, string password)
        {
            try
            {
                // Buscar usuário pelo username (incluindo Person para ter dados completos)
                var user = await _db.Users
                    .Include(u => u.Person)
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user == null)
                    return null;

                // Verificar se o usuário pode fazer login
                if (!user.CanLogin)
                    return null;

                // Verificar senha
                var hash = HashPassword(password);
                if (user.PasswordHash != hash)
                {
                    // Registrar tentativa falhada
                    user.RecordFailedLogin();
                    await _db.SaveChangesAsync();
                    return null;
                }

                // Login bem-sucedido - registrar
                user.RecordSuccessfulLogin(GetCurrentIpAddress());
                await _db.SaveChangesAsync();

                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                // Log do erro
                Console.WriteLine($"Erro no login: {ex.Message}");
                return null;
            }
        }

        public async Task<UserDto> RegisterAsync(UserDto dto, string password)
        {
            // Validar senha
            if (!User.ValidatePasswordStrength(password))
            {
                throw new ArgumentException("A senha não atende aos requisitos mínimos de segurança. " +
                    "Deve ter no mínimo 8 caracteres, incluindo maiúsculas, minúsculas e números.");
            }

            // Validar username
            if (!User.ValidateUsername(dto.Username))
            {
                throw new ArgumentException("Nome de usuário inválido. " +
                    "Deve ter entre 3 e 100 caracteres e conter apenas letras, números, pontos, hífens e underscores.");
            }

            // Verificar se o username já existe
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Nome de usuário já está em uso.");
            }

            var entity = _mapper.Map<User>(dto);
            entity.PasswordHash = HashPassword(password);
            
            // Garantir valores padrão para campos obrigatórios no banco
            entity.LastLoginIp ??= string.Empty;
            entity.Status = LoginStatus.Active;
            entity.FailedLoginAttempts = 0;
            
            _db.Users.Add(entity);
            await _db.SaveChangesAsync();
            return _mapper.Map<UserDto>(entity);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            // Validar nova senha
            if (!User.ValidatePasswordStrength(newPassword))
            {
                throw new ArgumentException("A senha não atende aos requisitos mínimos de segurança.");
            }

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;
            
            user.PasswordHash = HashPassword(newPassword);
            await _db.SaveChangesAsync();
            return true;
        }

        public Task<bool> ResetPasswordAsync(string email)
        {
            // Implementar envio de token e reset
            return Task.FromResult(true);
        }

        public Task<bool> LogoutAsync(int userId)
        {
            // Implementar invalidação de sessão/token
            return Task.FromResult(true);
        }

        // Segurança de conta
        public async Task<bool> LockUserAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;
            
            user.Lock(); // Bloqueio indefinido
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlockUserAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;
            
            user.Unlock();
            await _db.SaveChangesAsync();
            return true;
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        private static string GetCurrentIpAddress()
        {
            try
            {
                // Obter IP local (em produção, usar HttpContext ou similar)
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                var ipAddress = host.AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                return ipAddress?.ToString() ?? "127.0.0.1";
            }
            catch
            {
                return "127.0.0.1";
            }
        }
    }
}
