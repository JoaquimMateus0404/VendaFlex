namespace VendaFlex.Core.DTOs
{
    public class InvoiceDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime Date { get; set; }
        public DateTime? DueDate { get; set; }
        public int PersonId { get; set; }
        public int UserId { get; set; }
        public int Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public string Notes { get; set; }
        public string InternalNotes { get; set; }
    }
}
