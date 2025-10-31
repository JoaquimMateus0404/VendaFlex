namespace VendaFlex.Core.DTOs
{
    /// <summary>
    /// DTO para métricas do dashboard (cards de KPIs)
    /// </summary>
    public class DashboardMetricDto
    {
        /// <summary>
        /// Título da métrica (ex: "Receita Total")
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Valor principal (ex: "Kz 4.523.000,00")
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Ícone do Material Design (ex: "CashMultiple")
        /// </summary>
        public string IconKind { get; set; } = string.Empty;

        /// <summary>
        /// Cor de fundo do ícone (ex: "#FEE2E2")
        /// </summary>
        public string IconBackgroundColor { get; set; } = string.Empty;

        /// <summary>
        /// Cor do ícone (ex: "#EF4444")
        /// </summary>
        public string IconColor { get; set; } = string.Empty;

        /// <summary>
        /// Texto de mudança/comparação (ex: "↑ 12.5%")
        /// </summary>
        public string ChangeText { get; set; } = string.Empty;

        /// <summary>
        /// Cor da mudança (verde para positivo, vermelho para negativo)
        /// </summary>
        public string ChangeColor { get; set; } = "#10B981";

        /// <summary>
        /// Texto descritivo adicional (ex: "vs. mês passado")
        /// </summary>
        public string DescriptionText { get; set; } = string.Empty;
    }
}
