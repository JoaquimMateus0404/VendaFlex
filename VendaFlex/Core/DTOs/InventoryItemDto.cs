namespace VendaFlex.Core.DTOs
{
    /// <summary>
    /// DTO para exibição de itens do inventário com entradas, saídas e valores.
    /// </summary>
    public class InventoryItemDto
    {
        public int ProductId { get; set; }
        public string InternalCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        
        /// <summary>
        /// Total de quantidade de entradas no estoque.
        /// </summary>
        public decimal TotalEntries { get; set; }
        
        /// <summary>
        /// Total de quantidade de saídas do estoque.
        /// </summary>
        public decimal TotalExits { get; set; }
        
        /// <summary>
        /// Quantidade disponível (entradas - saídas).
        /// </summary>
        public decimal AvailableQuantity => TotalEntries - TotalExits;
        
        /// <summary>
        /// Custo unitário do produto.
        /// </summary>
        public decimal CostPrice { get; set; }
        
        /// <summary>
        /// Valor total do estoque disponível (AvailableQuantity * CostPrice).
        /// </summary>
        public decimal TotalValue => AvailableQuantity * CostPrice;
    }
}
