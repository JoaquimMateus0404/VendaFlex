namespace VendaFlex.Core.DTOs
{
    public class SalesPointDto
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = string.Empty; // dd/MM
        public decimal Value { get; set; }
        public string ValueFormatted => $"Kz {Value:N0}";
        // Altura do gráfico em pixels (pré-calculada no VM para simplicidade)
        public double BarHeight { get; set; }
    }
}
