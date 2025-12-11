using VendaFlex.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VendaFlex.Core.Interfaces
{
    public interface IReceiptPrintService
    {
        Task PrintAsync(CompanyConfigDto cfg, InvoiceDto invoice, IEnumerable<InvoiceProductDto> items, string format);
        
        /// <summary>
        /// Gera um relatório PDF completo das vendas diárias do usuário
        /// </summary>
        Task<string> GenerateDailySalesReportPdfAsync(
            CompanyConfigDto companyConfig,
            string userName,
            int userId,
            DateTime reportDate,
            List<InvoiceDto> invoices,
            Dictionary<int, List<InvoiceProductDto>> invoiceProducts,
            List<(string PaymentTypeName, decimal Amount, int Count)> paymentsByType);
    }
}
