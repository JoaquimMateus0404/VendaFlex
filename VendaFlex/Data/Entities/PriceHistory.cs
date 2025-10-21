using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    public class PriceHistory : AuditableEntity
    {
        [Key]
        public int PriceHistoryId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OldSalePrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NewSalePrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OldCostPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NewCostPrice { get; set; }

        [StringLength(500)]
        public string Reason { get; set; }

        public DateTime ChangeDate { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }
    }

    
}
