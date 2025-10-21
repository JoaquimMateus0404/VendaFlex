using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto> GetByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto> CreateAsync(ProductDto dto);
        Task<ProductDto> UpdateAsync(ProductDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ProductDto>> SearchAsync(string term);
        Task<bool> UpdateStockAsync(int productId, int quantity);
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
    }
}
