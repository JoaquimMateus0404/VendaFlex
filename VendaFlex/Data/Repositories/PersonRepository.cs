using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Repositório para operações relacionadas a pessoas (clientes, fornecedores, etc.).
    /// </summary>
    public class PersonRepository : IRepository<Person>
    {
        private readonly ApplicationDbContext _context;

        public PersonRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Person> GetByIdAsync(int id)
        {
            return await _context.Persons.FindAsync(id);
        }

        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            return await _context.Persons.ToListAsync();
        }

        public async Task<IEnumerable<Person>> FindAsync(System.Linq.Expressions.Expression<System.Func<Person, bool>> predicate)
        {
            return await _context.Persons.Where(predicate).ToListAsync();
        }

        public async Task<Person> AddAsync(Person entity)
        {
            _context.Persons.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Person> UpdateAsync(Person entity)
        {
            _context.Persons.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person == null) return false;
            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Person>> GetPagedAsync(int page, int pageSize)
        {
            return await _context.Persons
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Busca pessoas por nome ou documento.
        /// </summary>
        public async Task<IEnumerable<Person>> SearchAsync(string term)
        {
            return await _context.Persons
                .Where(p => p.Name.Contains(term) || p.TaxId.Contains(term) || p.Email.Contains(term))
                .ToListAsync();
        }
    }
}
