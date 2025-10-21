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
    }
}
