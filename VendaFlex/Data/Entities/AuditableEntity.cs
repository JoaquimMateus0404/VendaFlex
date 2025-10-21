using System.ComponentModel.DataAnnotations;

namespace VendaFlex.Data.Entities
{
    /// <summary>
    /// Classe base para auditoria de entidades
    /// </summary>
    public abstract class AuditableEntity
    {
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public int? CreatedByUserId { get; set; }

        public int? UpdatedByUserId { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }
    }
}
