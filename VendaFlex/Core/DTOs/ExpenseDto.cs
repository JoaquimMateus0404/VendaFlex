namespace VendaFlex.Core.DTOs
{
    public class ExpenseDto
    {
        public int ExpenseId { get; set; }
        public int ExpenseTypeId { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public string Title { get; set; }
        public string Notes { get; set; }
        public string Reference { get; set; }
        public string AttachmentUrl { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }
    }
}
