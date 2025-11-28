namespace VendaFlex.Core.DTOs
{
    public class InvoiceProductDto
    {
        public int InvoiceProductId { get; set; }
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxRate { get; set; }

        // Propriedades adicionais para exibição
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;

        // Propriedades calculadas
        public decimal SubTotal => Quantity * UnitPrice;
        public decimal Discount => SubTotal * (DiscountPercentage / 100m);
        public decimal TaxAmount => (SubTotal - Discount) * (TaxRate / 100m);
        public decimal Total => SubTotal - Discount + TaxAmount;
    }
}
