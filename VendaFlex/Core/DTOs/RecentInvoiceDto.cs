using System;

namespace VendaFlex.Core.DTOs
{
    /// <summary>
    /// DTO para faturas recentes no dashboard
    /// </summary>
    public class RecentInvoiceDto
    {
        /// <summary>
        /// Número da fatura (ex: "INV-2025-042")
        /// </summary>
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>
        /// Nome do cliente
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Data de emissão
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// Data formatada (ex: "28/10/2025")
        /// </summary>
        public string IssueDateFormatted => IssueDate.ToString("dd/MM/yyyy");

        /// <summary>
        /// Valor total da fatura
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Valor formatado (ex: "Kz 24.500,00")
        /// </summary>
        public string TotalAmountFormatted => $"Kz {TotalAmount:N2}";

        /// <summary>
        /// Status da fatura (Pago, Pendente, Vencido)
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Cor do status
        /// </summary>
        public string StatusColor
        {
            get
            {
                return Status switch
                {
                    "Pago" => "#10B981",
                    "Pendente" => "#F59E0B",
                    "Vencido" => "#EF4444",
                    _ => "#6B7280"
                };
            }
        }

        /// <summary>
        /// Cor de fundo do status
        /// </summary>
        public string StatusBackgroundColor
        {
            get
            {
                return Status switch
                {
                    "Pago" => "#D1FAE5",
                    "Pendente" => "#FEF3C7",
                    "Vencido" => "#FEE2E2",
                    _ => "#F3F4F6"
                };
            }
        }
    }
}
