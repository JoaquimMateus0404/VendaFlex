using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    public interface ICompanyConfigService
    {
        Task<CompanyConfigDto> GetAsync();
        Task<CompanyConfigDto> UpdateAsync(CompanyConfigDto dto);
        Task<int> GetNextInvoiceNumberAsync();
    }
}
