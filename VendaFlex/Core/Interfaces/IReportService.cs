using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> GenerateSalesReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateStockReportAsync();
        Task<byte[]> GenerateExpenseReportAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateCustomerReportAsync();
        Task<byte[]> GenerateProductReportAsync();
    }
}
