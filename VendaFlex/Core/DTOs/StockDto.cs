namespace VendaFlex.Core.DTOs
{
    public class StockDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int? ReservedQuantity { get; set; }
        public DateTime LastStockUpdate { get; set; }
        public int? LastStockUpdateByUserId { get; set; }
        
        // Propriedades adicionais para exibição
        public string ProductName { get; set; }
        public int? MinimumStock { get; set; }
        public int? ReorderPoint { get; set; }
        public int AvailableQuantity => Quantity - (ReservedQuantity ?? 0);
    }
}
