using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendaFlex.Data.Entities
{
    public class Product : CodedEntity
    {
        [Key]
        public int ProductId { get; set; }

        [StringLength(100)]
        public string Barcode { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(500)]
        public string ShortDescription { get; set; }

        [StringLength(50)]
        public string SKU { get; set; }

        [StringLength(50)]
        public string Weight { get; set; }

        [StringLength(100)]
        public string Dimensions { get; set; }

        [Required]
        public int CategoryId { get; set; }

        /// <summary>
        /// Fornecedor é uma Person com Type = Supplier
        /// </summary>
        [Required]
        public int SupplierId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal SalePrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal? DiscountPercentage { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal? TaxRate { get; set; } = 0;

        [StringLength(500)]
        public string PhotoUrl { get; set; }

        public ProductStatus Status { get; set; } = ProductStatus.Active;

        public bool IsFeatured { get; set; } = false;

        public bool AllowBackorder { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        // Stock Control
        public bool ControlsStock { get; set; } = true;

        public int? MinimumStock { get; set; }

        public int? MaximumStock { get; set; }

        public int? ReorderPoint { get; set; }

        // Expiration Control
        public bool HasExpirationDate { get; set; } = false;

        public int? ExpirationDays { get; set; }

        public int? ExpirationWarningDays { get; set; }

        // Computed Properties
        [NotMapped]
        public decimal ProfitMargin => SalePrice > 0 ? ((SalePrice - CostPrice) / SalePrice) * 100 : 0;

        [NotMapped]
        public decimal ProfitAmount => SalePrice - CostPrice;

        [NotMapped]
        public decimal FinalPrice => SalePrice - (SalePrice * (DiscountPercentage ?? 0) / 100);

        [NotMapped]
        public bool IsLowStock => Stock != null && MinimumStock.HasValue && Stock.Quantity <= MinimumStock;

        // Navigation Properties
        [ForeignKey(nameof(CategoryId))]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Supplier é uma Person
        /// </summary>
        [ForeignKey(nameof(SupplierId))]
        public virtual Person Supplier { get; set; }

        public virtual Stock Stock { get; set; }

        public virtual ICollection<Expiration> Expirations { get; set; } = new List<Expiration>();
        public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
        public virtual ICollection<InvoiceProduct> InvoiceProducts { get; set; } = new List<InvoiceProduct>();
        public virtual ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
    }

    public enum ProductStatus
    {
        Active = 1,
        Inactive = 2,
        Discontinued = 3,
        OutOfStock = 4
    }
}
