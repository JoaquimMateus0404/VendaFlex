using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    #region Payment

    #endregion


    public class Expense : AuditableEntity
    {
        [Key]
        public int ExpenseId { get; set; }

        [Required]
        public int ExpenseTypeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Value { get; set; }

        [StringLength(200)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Notes { get; set; }

        [StringLength(100)]
        public string Reference { get; set; }

        [StringLength(500)]
        public string AttachmentUrl { get; set; }

        public bool IsPaid { get; set; } = false;

        public DateTime? PaidDate { get; set; }

        [ForeignKey(nameof(ExpenseTypeId))]
        public virtual ExpenseType ExpenseType { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }
    }

   
}
