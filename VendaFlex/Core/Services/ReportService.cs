using VendaFlex.Core.Interfaces;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço de geração de relatórios (exportar em PDF/Excel futuramente).
    /// </summary>
    public class ReportService : IReportService
    {
        public Task<byte[]> GenerateSalesReportAsync(DateTime startDate, DateTime endDate)
        {
            // Implementar geração real (ex: FastReport, QuestPDF, ClosedXML)
            return Task.FromResult(Array.Empty<byte>());
        }

        public Task<byte[]> GenerateStockReportAsync()
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        public Task<byte[]> GenerateExpenseReportAsync(DateTime startDate, DateTime endDate)
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        public Task<byte[]> GenerateCustomerReportAsync()
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        public Task<byte[]> GenerateProductReportAsync()
        {
            return Task.FromResult(Array.Empty<byte>());
        }
    }
}
