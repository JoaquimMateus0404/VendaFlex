using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    #region Product Management

    #endregion


    public class Stock
    {
        [Key]
        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; } = 0;

        public int? ReservedQuantity { get; set; } = 0;

        [NotMapped]
        public int AvailableQuantity => Quantity - (ReservedQuantity ?? 0);

        public DateTime LastStockUpdate { get; set; } = DateTime.UtcNow;

        public int? LastStockUpdateByUserId { get; set; }

        public virtual Product Product { get; set; }
    }

    
}
