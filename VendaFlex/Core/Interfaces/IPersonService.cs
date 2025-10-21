using VendaFlex.Core.DTOs;

namespace VendaFlex.Core.Interfaces
{
    public interface IPersonService
    {
        Task<PersonDto> GetByIdAsync(int id);
        Task<IEnumerable<PersonDto>> GetAllAsync();
        Task<PersonDto> CreateAsync(PersonDto dto);
        Task<PersonDto> UpdateAsync(PersonDto dto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<PersonDto>> SearchAsync(string term);
    }
}
