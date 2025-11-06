using VendaFlex.Data.Entities;

namespace VendaFlex.Core.DTOs
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Barcode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string SKU { get; set; }
        public string Weight { get; set; }
        public string Dimensions { get; set; }
        public int CategoryId { get; set; }
        public int SupplierId { get; set; }
        public decimal SalePrice { get; set; }
        public decimal CostPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? TaxRate { get; set; }
        public string PhotoUrl { get; set; }
        public ProductStatus Status { get; set; }

        public bool IsFeatured { get; set; }
        public bool AllowBackorder { get; set; }
        public int DisplayOrder { get; set; }
        public bool ControlsStock { get; set; }
        public int? MinimumStock { get; set; }
        public int? MaximumStock { get; set; }
        public int? ReorderPoint { get; set; }
        public bool HasExpirationDate { get; set; }
        public int? ExpirationDays { get; set; }
        public int? ExpirationWarningDays { get; set; }
        /// <summary>
        /// Quantidade atual disponível para venda (preenchida na camada de apresentação).
        /// </summary>
        public int CurrentStock { get; set; }
        
        // Propriedades adicionais para exibição
        public string Code { get; set; } // Código genérico (pode ser InternalCode ou ExternalCode)
        public string InternalCode { get; set; }
        public string ExternalCode { get; set; }
        public string CategoryName { get; set; }
    }
}
