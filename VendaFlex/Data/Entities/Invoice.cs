using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    #region Stock Management


    public class Invoice : AuditableEntity
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required]
        [StringLength(50)]
        public string InvoiceNumber { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Cliente é uma Person com Type = Customer
        /// </summary>
        [Required]
        public int PersonId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingCost { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0;

        [NotMapped]
        public decimal Balance => Total - PaidAmount;

        [NotMapped]
        public bool IsFullyPaid => PaidAmount >= Total;

        [StringLength(1000)]
        public string Notes { get; set; }

        [StringLength(1000)]
        public string InternalNotes { get; set; }

        [ForeignKey(nameof(PersonId))]
        public virtual Person Person { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        public virtual ICollection<InvoiceProduct> InvoiceProducts { get; set; } = new List<InvoiceProduct>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }


    public enum InvoiceStatus
    {
        Draft = 1, // rascunho
        Confirmed = 2,
        Paid = 3, // pago
        Cancelled = 4,
        Refunded = 5, // reembolsado
        Pending = 6 // pendente
    }
    #endregion
}
