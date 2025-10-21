using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    public class Expiration : AuditableEntity
    {
        [Key]
        public int ExpirationId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }

        [Required]
        public int Quantity { get; set; }

        [StringLength(100)]
        public string BatchNumber { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [NotMapped]
        public bool IsExpired => ExpirationDate < DateTime.Now;

        [NotMapped]
        public bool IsNearExpiration => ExpirationDate <= DateTime.Now.AddDays(30);

        [ForeignKey(nameof(ProductId))]
        public virtual Product Product { get; set; }
    }

   
}
