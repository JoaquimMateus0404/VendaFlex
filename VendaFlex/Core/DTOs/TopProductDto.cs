namespace VendaFlex.Core.DTOs
{
    /// <summary>
    /// DTO para produtos mais vendidos no dashboard
    /// </summary>
    public class TopProductDto
    {
        /// <summary>
        /// Nome do produto
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Quantidade vendida
        /// </summary>
        public int QuantitySold { get; set; }

        /// <summary>
        /// Receita gerada pelo produto
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// Receita formatada (ex: "Kz 85.400,00")
        /// </summary>
        public string RevenueFormatted => $"Kz {Revenue:N2}";

        /// <summary>
        /// Percentual de progresso visual (0-100)
        /// </summary>
        public double ProgressPercentage { get; set; }
    }
}
