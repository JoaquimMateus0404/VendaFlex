namespace VendaFlex.Core.DTOs
{
    public class StockMovementDto
    {
        public int StockMovementId { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Quantity { get; set; }
        public int? PreviousQuantity { get; set; }
        public int? NewQuantity { get; set; }
        public DateTime Date { get; set; }
        public int Type { get; set; }
        public string Notes { get; set; }
        public string Reference { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? TotalCost { get; set; }
    }
}
