using AutoMapper;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço para gestão de pessoas (clientes, fornecedores, funcionários).
    /// </summary>
    public class PersonService : IPersonService
    {
        private readonly IRepository<Person> _repo;
        private readonly IMapper _mapper;

        public PersonService(IRepository<Person> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<PersonDto> GetByIdAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id);
            return _mapper.Map<PersonDto>(e);
        }

        public async Task<IEnumerable<PersonDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<PersonDto>>(list);
        }

        public async Task<PersonDto> CreateAsync(PersonDto dto)
        {
            var e = _mapper.Map<Person>(dto);
            var created = await _repo.AddAsync(e);
            return _mapper.Map<PersonDto>(created);
        }

        public async Task<PersonDto> UpdateAsync(PersonDto dto)
        {
            var e = _mapper.Map<Person>(dto);
            var updated = await _repo.UpdateAsync(e);
            return _mapper.Map<PersonDto>(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }

        public async Task<IEnumerable<PersonDto>> SearchAsync(string term)
        {
            var list = await _repo.FindAsync(p => p.Name.Contains(term) || p.Email.Contains(term) || p.TaxId.Contains(term));
            return _mapper.Map<IEnumerable<PersonDto>>(list);
        }
    }
}
