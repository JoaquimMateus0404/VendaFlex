using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    public class InvoiceProduct
    {
        [Key]
        public int InvoiceProductId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxRate { get; set; } = 0;

        [NotMapped]
        public decimal SubTotal => Quantity * UnitPrice;

        [NotMapped]
        public decimal DiscountAmount => SubTotal * (DiscountPercentage / 100);

        [NotMapped]
        public decimal TaxAmount => (SubTotal - DiscountAmount) * (TaxRate / 100);

        [NotMapped]
        public decimal Total => SubTotal - DiscountAmount + TaxAmount;

        [ForeignKey(nameof(InvoiceId))]
        public virtual Invoice Invoice { get; set; }

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }
    }

  
}
