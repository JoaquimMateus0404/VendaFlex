namespace VendaFlex.Core.DTOs
{
    public class StockDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int? ReservedQuantity { get; set; }
        public DateTime LastStockUpdate { get; set; }
        public int? LastStockUpdateByUserId { get; set; }
    }
}
