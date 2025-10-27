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
        private readonly IMapper _mapper;

        public Task<ExpenseDto> CreateAsync(ExpenseDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ExpenseDto>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ExpenseDto> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ExpenseTypeDto>> GetExpenseTypesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ExpenseDto> UpdateAsync(ExpenseDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
