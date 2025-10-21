using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    public interface IExpenseService
    {
        Task<ExpenseDto> GetByIdAsync(int id);
        Task<IEnumerable<ExpenseDto>> GetAllAsync();
        Task<ExpenseDto> CreateAsync(ExpenseDto dto);
        Task<ExpenseDto> UpdateAsync(ExpenseDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ExpenseTypeDto>> GetExpenseTypesAsync();
    }
}
