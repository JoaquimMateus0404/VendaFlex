using AutoMapper;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço para despesas e seus tipos.
    /// </summary>
    public class ExpenseService : IExpenseService
    {
        private readonly IRepository<Expense> _repo;
        private readonly IRepository<ExpenseType> _types;
        private readonly IMapper _mapper;

        public ExpenseService(IRepository<Expense> repo, IRepository<ExpenseType> types, IMapper mapper)
        {
            _repo = repo;
            _types = types;
            _mapper = mapper;
        }

        public async Task<ExpenseDto> GetByIdAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id);
            return _mapper.Map<ExpenseDto>(e);
        }

        public async Task<IEnumerable<ExpenseDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<ExpenseDto>>(list);
        }

        public async Task<ExpenseDto> CreateAsync(ExpenseDto dto)
        {
            var e = _mapper.Map<Expense>(dto);
            var created = await _repo.AddAsync(e);
            return _mapper.Map<ExpenseDto>(created);
        }

        public async Task<ExpenseDto> UpdateAsync(ExpenseDto dto)
        {
            var e = _mapper.Map<Expense>(dto);
            var updated = await _repo.UpdateAsync(e);
            return _mapper.Map<ExpenseDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }

        public async Task<IEnumerable<ExpenseTypeDto>> GetExpenseTypesAsync()
        {
            var list = await _types.GetAllAsync();
            return _mapper.Map<IEnumerable<ExpenseTypeDto>>(list);
        }
    }
}
