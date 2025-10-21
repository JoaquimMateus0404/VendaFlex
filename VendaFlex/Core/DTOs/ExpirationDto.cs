namespace VendaFlex.Core.DTOs
{
    public class ExpirationDto
    {
        public int ExpirationId { get; set; }
        public int ProductId { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int Quantity { get; set; }
        public string BatchNumber { get; set; }
        public string Notes { get; set; }
    }
}
