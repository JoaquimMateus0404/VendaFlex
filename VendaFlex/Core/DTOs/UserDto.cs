namespace VendaFlex.Core.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public int PersonId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public int Status { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public string LastLoginIp { get; set; }
    }
}
