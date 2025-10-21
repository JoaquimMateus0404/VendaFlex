using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{

    #region Audit Log

    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; }

        public int? EntityId { get; set; }

        [StringLength(4000)]
        public string OldValues { get; set; }

        [StringLength(4000)]
        public string NewValues { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string IpAddress { get; set; }

        [StringLength(500)]
        public string UserAgent { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
    }

    #endregion
}
