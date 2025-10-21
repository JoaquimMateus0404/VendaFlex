using AutoMapper;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço para gestão de produtos, incluindo operações de estoque e pesquisa.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _repo;
        private readonly IRepository<Stock> _stocks;
        private readonly IMapper _mapper;

        public ProductService(IRepository<Product> repo, IRepository<Stock> stocks, IMapper mapper)
        {
            _repo = repo;
            _stocks = stocks;
            _mapper = mapper;
        }

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id);
            return _mapper.Map<ProductDto>(e);
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(list);
        }

        public async Task<ProductDto> CreateAsync(ProductDto dto)
        {
            var e = _mapper.Map<Product>(dto);
            var created = await _repo.AddAsync(e);
            return _mapper.Map<ProductDto>(created);
        }

        public async Task<ProductDto> UpdateAsync(ProductDto dto)
        {
            var e = _mapper.Map<Product>(dto);
            var updated = await _repo.UpdateAsync(e);
            return _mapper.Map<ProductDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }

        public async Task<IEnumerable<ProductDto>> SearchAsync(string term)
        {
            var list = await _repo.FindAsync(p => p.Name.Contains(term) || p.Barcode.Contains(term) || p.InternalCode.Contains(term));
            return _mapper.Map<IEnumerable<ProductDto>>(list);
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            var stock = await _stocks.GetByIdAsync(productId);
            if (stock == null)
            {
                stock = new Stock { ProductId = productId, Quantity = quantity };
                await _stocks.AddAsync(stock);
                return true;
            }
            stock.Quantity = quantity;
            await _stocks.UpdateAsync(stock);
            return true;
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
        {
            var list = await _repo.FindAsync(p => p.MinimumStock.HasValue && p.Stock != null && p.Stock.Quantity <= p.MinimumStock);
            return _mapper.Map<IEnumerable<ProductDto>>(list);
        }
    }
}
