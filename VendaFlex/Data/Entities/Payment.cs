using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    #region Invoice & Sales

    #endregion


    public class Payment : AuditableEntity
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public int PaymentTypeId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string Reference { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public bool IsConfirmed { get; set; } = true;

        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice Invoice { get; set; }

        [ForeignKey(nameof(PaymentTypeId))]
        public virtual PaymentType PaymentType { get; set; }
    }

    
}
