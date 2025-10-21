namespace VendaFlex.Core.DTOs
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public int PaymentTypeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Reference { get; set; }
        public string Notes { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
