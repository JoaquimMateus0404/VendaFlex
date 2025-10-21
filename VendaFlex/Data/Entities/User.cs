using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    #region User


    /// <summary>
    /// Usuário do sistema (Employee) - sempre vinculado a uma Person
    /// </summary>
    public class User : AuditableEntity
    {
        private const int MaxFailedLoginAttempts = 5;
        private const int LockoutDurationMinutes = 30;

        [Key]
        public int UserId { get; set; }

        /// <summary>
        /// Referência para Person - TODO usuário É uma pessoa
        /// </summary>
        [Required]
        public int PersonId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome de usuário deve ter entre 3 e 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "O nome de usuário pode conter apenas letras, números, pontos, hífens e underscores")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public LoginStatus Status { get; set; } = LoginStatus.Active;

        // Dados de Segurança
        public DateTime? LastLoginAt { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;

        public DateTime? LockedUntil { get; set; }

        [StringLength(100)]
        public string LastLoginIp { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey(nameof(PersonId))]
        public virtual Person? Person { get; set; }

        public virtual ICollection<UserPrivilege> UserPrivileges { get; set; } = new List<UserPrivilege>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        // Propriedades de Conveniência (acessam Person)
        [NotMapped]
        public string? FullName => Person?.Name;

        [NotMapped]
        public string? Email => Person?.Email;

        [NotMapped]
        public string? PhoneNumber => Person?.PhoneNumber;

        [NotMapped]
        public string? ProfileImageUrl => Person?.ProfileImageUrl;

        // Métodos de Negócio

        /// <summary>
        /// Verifica se o usuário está bloqueado
        /// </summary>
        [NotMapped]
        public bool IsLocked => LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

        /// <summary>
        /// Verifica se o usuário pode fazer login
        /// </summary>
        [NotMapped]
        public bool CanLogin => Status == LoginStatus.Active && !IsLocked;

        /// <summary>
        /// Registra uma tentativa de login bem-sucedida
        /// </summary>
        public void RecordSuccessfulLogin(string ipAddress)
        {
            LastLoginAt = DateTime.UtcNow;
            LastLoginIp = ipAddress ?? string.Empty;
            FailedLoginAttempts = 0;
            LockedUntil = null;
        }

        /// <summary>
        /// Registra uma tentativa de login falhada
        /// </summary>
        public void RecordFailedLogin()
        {
            FailedLoginAttempts++;

            if (FailedLoginAttempts >= MaxFailedLoginAttempts)
            {
                LockedUntil = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
            }
        }

        /// <summary>
        /// Desbloqueia o usuário manualmente
        /// </summary>
        public void Unlock()
        {
            FailedLoginAttempts = 0;
            LockedUntil = null;
            if (Status == LoginStatus.Suspended)
            {
                Status = LoginStatus.Active;
            }
        }

        /// <summary>
        /// Bloqueia o usuário
        /// </summary>
        public void Lock(int durationMinutes = 0)
        {
            if (durationMinutes > 0)
            {
                LockedUntil = DateTime.UtcNow.AddMinutes(durationMinutes);
            }
            else
            {
                // Bloqueio indefinido
                LockedUntil = DateTime.UtcNow.AddYears(100);
                Status = LoginStatus.Suspended;
            }
        }

        /// <summary>
        /// Valida se a senha atende aos requisitos mínimos
        /// </summary>
        public static bool ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Mínimo 8 caracteres
            if (password.Length < 8)
                return false;

            // Pelo menos uma letra maiúscula
            if (!password.Any(char.IsUpper))
                return false;

            // Pelo menos uma letra minúscula
            if (!password.Any(char.IsLower))
                return false;

            // Pelo menos um número
            if (!password.Any(char.IsDigit))
                return false;

            return true;
        }

        /// <summary>
        /// Valida o nome de usuário
        /// </summary>
        public static bool ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (username.Length < 3 || username.Length > 100)
                return false;

            // Apenas letras, números, pontos, hífens e underscores
            return username.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_');
        }
    }

    public enum LoginStatus
    {
        /// <summary>
        /// Usuário ativo e pode fazer login
        /// </summary>
        Active = 1,

        /// <summary>
        /// Usuário inativo (temporariamente desabilitado)
        /// </summary>
        Inactive = 2,

        /// <summary>
        /// Usuário suspenso (requer ação do administrador)
        /// </summary>
        Suspended = 3,

        /// <summary>
        /// Aguardando ativação (primeiro acesso)
        /// </summary>
        PendingActivation = 4
    }

    #endregion
}
