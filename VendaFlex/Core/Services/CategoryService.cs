using AutoMapper;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço de categorias de produtos.
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Category> _repo;
        private readonly IMapper _mapper;

        public CategoryService(IRepository<Category> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id);
            return _mapper.Map<CategoryDto>(e);
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(list);
        }

        public async Task<CategoryDto> CreateAsync(CategoryDto dto)
        {
            var e = _mapper.Map<Category>(dto);
            var created = await _repo.AddAsync(e);
            return _mapper.Map<CategoryDto>(created);
        }

        public async Task<CategoryDto> UpdateAsync(CategoryDto dto)
        {
            var e = _mapper.Map<Category>(dto);
            var updated = await _repo.UpdateAsync(e);
            return _mapper.Map<CategoryDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}
