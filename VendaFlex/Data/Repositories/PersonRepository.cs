using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Repositório para operações de acesso a dados de pessoas (clientes, fornecedores, funcionários).
    /// Responsável apenas por interagir com o banco de dados, sem lógica de negócio.
    /// </summary>
    public class PersonRepository
    {
        private readonly ApplicationDbContext _context;

        public PersonRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Basic CRUD

        /// <summary>
        /// Busca uma pessoa por ID.
        /// </summary>
        public async Task<Person?> GetByIdAsync(int id)
        {
            return await _context.Persons.FindAsync(id);
        }

        /// <summary>
        /// Busca uma pessoa por ID sem tracking (melhor performance para leitura).
        /// </summary>
        public async Task<Person?> GetByIdAsNoTrackingAsync(int id)
        {
            return await _context.Persons
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PersonId == id);
        }

        /// <summary>
        /// Retorna todas as pessoas.
        /// </summary>
        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            return await _context.Persons
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca pessoas usando um predicado customizado.
        /// </summary>
        public async Task<IEnumerable<Person>> FindAsync(Expression<Func<Person, bool>> predicate)
        {
            return await _context.Persons
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Adiciona uma nova pessoa.
        /// </summary>
        public async Task<Person> AddAsync(Person entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.Persons.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Atualiza uma pessoa existente.
        /// </summary>
        public async Task<Person> UpdateAsync(Person entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Persons.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Remove uma pessoa do banco de dados.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var person = await _context.Persons.FindAsync(id);
            if (person == null)
                return false;

            _context.Persons.Remove(person);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Pagination

        /// <summary>
        /// Retorna pessoas paginadas.
        /// </summary>
        public async Task<IEnumerable<Person>> GetPagedAsync(int page, int pageSize)
        {
            if (page < 1)
                throw new ArgumentException("Página deve ser maior ou igual a 1.", nameof(page));

            if (pageSize < 1)
                throw new ArgumentException("Tamanho da página deve ser maior que 0.", nameof(pageSize));

            return await _context.Persons
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna o total de pessoas cadastradas.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Persons.CountAsync();
        }

        /// <summary>
        /// Retorna o total de pessoas que atendem a um predicado.
        /// </summary>
        public async Task<int> GetCountAsync(Expression<Func<Person, bool>> predicate)
        {
            return await _context.Persons.CountAsync(predicate);
        }

        #endregion

        #region Search Operations

        /// <summary>
        /// Busca pessoas por termo (nome, email, documento).
        /// </summary>
        public async Task<IEnumerable<Person>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Enumerable.Empty<Person>();

            term = term.ToLower();

            return await _context.Persons
                .Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Email != null && p.Email.ToLower().Contains(term)) ||
                    (p.TaxId != null && p.TaxId.Contains(term)) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(term)) ||
                    (p.MobileNumber != null && p.MobileNumber.Contains(term)))
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca pessoa por email.
        /// </summary>
        public async Task<Person?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _context.Persons
                .FirstOrDefaultAsync(p => p.Email == email);
        }

        /// <summary>
        /// Busca pessoa por documento fiscal.
        /// </summary>
        public async Task<Person?> GetByTaxIdAsync(string taxId)
        {
            if (string.IsNullOrWhiteSpace(taxId))
                return null;

            return await _context.Persons
                .FirstOrDefaultAsync(p => p.TaxId == taxId);
        }

        /// <summary>
        /// Busca pessoa por número de identificação.
        /// </summary>
        public async Task<Person?> GetByIdentificationNumberAsync(string identificationNumber)
        {
            if (string.IsNullOrWhiteSpace(identificationNumber))
                return null;

            return await _context.Persons
                .FirstOrDefaultAsync(p => p.IdentificationNumber == identificationNumber);
        }

        #endregion

        #region Query by Type

        /// <summary>
        /// Retorna apenas clientes (Customer ou Both).
        /// </summary>
        public async Task<IEnumerable<Person>> GetCustomersAsync()
        {
            return await _context.Persons
                .Where(p => p.Type == PersonType.Customer || p.Type == PersonType.Both)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna apenas fornecedores (Supplier ou Both).
        /// </summary>
        public async Task<IEnumerable<Person>> GetSuppliersAsync()
        {
            return await _context.Persons
                .Where(p => p.Type == PersonType.Supplier || p.Type == PersonType.Both)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna apenas funcionários.
        /// </summary>
        public async Task<IEnumerable<Person>> GetEmployeesAsync()
        {
            return await _context.Persons
                .Where(p => p.Type == PersonType.Employee)
                .Include(p => p.User) // Include User para funcionários
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna pessoas por tipo específico.
        /// </summary>
        public async Task<IEnumerable<Person>> GetByTypeAsync(PersonType type)
        {
            return await _context.Persons
                .Where(p => p.Type == type)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region Status Operations

        /// <summary>
        /// Retorna apenas pessoas ativas.
        /// </summary>
        public async Task<IEnumerable<Person>> GetActiveAsync()
        {
            return await _context.Persons
                .Where(p => p.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna apenas pessoas inativas.
        /// </summary>
        public async Task<IEnumerable<Person>> GetInactiveAsync()
        {
            return await _context.Persons
                .Where(p => !p.IsActive)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Verifica se um email já está em uso.
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email, int? excludePersonId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var query = _context.Persons.Where(p => p.Email == email);

            if (excludePersonId.HasValue)
                query = query.Where(p => p.PersonId != excludePersonId.Value);

            return await query.AnyAsync();
        }

        /// <summary>
        /// Verifica se um documento fiscal já está em uso.
        /// </summary>
        public async Task<bool> TaxIdExistsAsync(string taxId, int? excludePersonId = null)
        {
            if (string.IsNullOrWhiteSpace(taxId))
                return false;

            var query = _context.Persons.Where(p => p.TaxId == taxId);

            if (excludePersonId.HasValue)
                query = query.Where(p => p.PersonId != excludePersonId.Value);

            return await query.AnyAsync();
        }

        #endregion

        #region Financial Operations

        /// <summary>
        /// Retorna clientes com saldo devedor (CurrentBalance > 0).
        /// </summary>
        public async Task<IEnumerable<Person>> GetCustomersWithDebtAsync()
        {
            return await _context.Persons
                .Where(p => (p.Type == PersonType.Customer || p.Type == PersonType.Both)
                         && p.CurrentBalance > 0)
                .OrderByDescending(p => p.CurrentBalance)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna clientes próximos ou acima do limite de crédito.
        /// </summary>
        public async Task<IEnumerable<Person>> GetCustomersNearCreditLimitAsync(decimal percentageThreshold = 90)
        {
            return await _context.Persons
                .Where(p => (p.Type == PersonType.Customer || p.Type == PersonType.Both)
                         && p.CreditLimit > 0
                         && (p.CurrentBalance / p.CreditLimit) * 100 >= percentageThreshold)
                .OrderByDescending(p => (p.CurrentBalance / p.CreditLimit) * 100)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// Adiciona múltiplas pessoas em uma única transação.
        /// </summary>
        public async Task AddRangeAsync(IEnumerable<Person> persons)
        {
            if (persons == null || !persons.Any())
                throw new ArgumentException("Lista de pessoas não pode ser vazia.", nameof(persons));

            await _context.Persons.AddRangeAsync(persons);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Atualiza múltiplas pessoas em uma única transação.
        /// </summary>
        public async Task UpdateRangeAsync(IEnumerable<Person> persons)
        {
            if (persons == null || !persons.Any())
                throw new ArgumentException("Lista de pessoas não pode ser vazia.", nameof(persons));

            _context.Persons.UpdateRange(persons);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}