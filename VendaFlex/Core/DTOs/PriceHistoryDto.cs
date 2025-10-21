namespace VendaFlex.Core.DTOs
{
    public class PriceHistoryDto
    {
        public int PriceHistoryId { get; set; }
        public int ProductId { get; set; }
        public decimal OldSalePrice { get; set; }
        public decimal NewSalePrice { get; set; }
        public decimal OldCostPrice { get; set; }
        public decimal NewCostPrice { get; set; }
        public string Reason { get; set; }
        public DateTime ChangeDate { get; set; }
    }
}
