using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Data.Entities;

namespace VendaFlex.Data.Repositories
{
    /// <summary>
    /// Reposit�rio para opera��es de acesso a dados de usu�rios.
    /// Respons�vel apenas por interagir com o banco de dados, sem l�gica de neg�cio.
    /// </summary>
    public class UserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Basic CRUD

        /// <summary>
        /// Busca um usu�rio por ID.
        /// </summary>
        /// <param name="id">ID do usu�rio</param>
        /// <returns>Usu�rio encontrado ou null</returns>
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        /// <summary>
        /// Busca um usu�rio por ID com tracking desabilitado (melhor performance para leitura).
        /// </summary>
        /// <param name="id">ID do usu�rio</param>
        /// <returns>Usu�rio encontrado ou null</returns>
        public async Task<User?> GetByIdAsNoTrackingAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Person)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        /// <summary>
        /// Retorna todos os usu�rios do sistema.
        /// </summary>
        /// <returns>Lista de usu�rios</returns>
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.Person)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Busca usu�rios usando um predicado customizado.
        /// </summary>
        /// <param name="predicate">Express�o lambda para filtro</param>
        /// <returns>Lista de usu�rios que atendem ao predicado</returns>
        public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate)
        {
            return await _context.Users
                .Include(u => u.Person)
                .Where(predicate)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Adiciona um novo usu�rio ao banco de dados.
        /// </summary>
        /// <param name="entity">Usu�rio a ser adicionado</param>
        /// <returns>Usu�rio adicionado com ID gerado</returns>
        public async Task<User> AddAsync(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.Users.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Atualiza um usu�rio existente.
        /// Evita marcar o grafo inteiro como modificado para n�o impactar entidades relacionadas (ex: Person).
        /// </summary>
        /// <param name="entity">Usu�rio com dados atualizados</param>
        /// <returns>Usu�rio atualizado</returns>
        public async Task<User> UpdateAsync(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Se j� est� sendo rastreado, apenas garante que o estado do User esteja como Modified
            var tracked = _context.Users.Local.FirstOrDefault(u => u.UserId == entity.UserId);
            if (tracked != null)
            {
                // Copia campos simples se a inst�ncia rastreada n�o for a mesma
                if (!ReferenceEquals(tracked, entity))
                {
                    _context.Entry(tracked).CurrentValues.SetValues(entity);
                }

                _context.Entry(tracked).State = EntityState.Modified;

                // Garante que a navega��o Person (se carregada) n�o seja marcada para update
                var personRef = _context.Entry(tracked).Reference(u => u.Person);
                if (personRef.IsLoaded && personRef.TargetEntry != null)
                {
                    personRef.TargetEntry.State = EntityState.Unchanged;
                }
            }
            else
            {
                // Anexa apenas a entidade User e marca como Modified (n�o propaga para navega��es)
                _context.Users.Attach(entity);
                var entry = _context.Entry(entity);
                entry.State = EntityState.Modified;

                var personRef = entry.Reference(u => u.Person);
                if (personRef.IsLoaded && personRef.TargetEntry != null)
                {
                    personRef.TargetEntry.State = EntityState.Unchanged;
                }
            }

            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Remove um usu�rio do banco de dados.
        /// </summary>
        /// <param name="id">ID do usu�rio a ser removido</param>
        /// <returns>True se removido com sucesso, False se n�o encontrado</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Pagination

        /// <summary>
        /// Retorna usu�rios paginados.
        /// </summary>
        /// <param name="page">N�mero da p�gina (inicia em 1)</param>
        /// <param name="pageSize">Quantidade de itens por p�gina</param>
        /// <returns>Lista de usu�rios da p�gina solicitada</returns>
        public async Task<IEnumerable<User>> GetPagedAsync(int page, int pageSize)
        {
            if (page < 1)
                throw new ArgumentException("P�gina deve ser maior ou igual a 1.", nameof(page));

            if (pageSize < 1)
                throw new ArgumentException("Tamanho da p�gina deve ser maior que 0.", nameof(pageSize));

            return await _context.Users
                .Include(u => u.Person)
                .OrderBy(u => u.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna o total de usu�rios cadastrados. 
        /// </summary>
        /// <returns>Quantidade total de usu�rios</returns>
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        /// <summary>
        /// Retorna o total de usu�rios que atendem a um predicado.
        /// </summary>
        /// <param name="predicate">Express�o lambda para filtro</param>
        /// <returns>Quantidade de usu�rios que atendem ao predicado</returns>
        public async Task<int> GetCountAsync(Expression<Func<User, bool>> predicate)
        {
            return await _context.Users.CountAsync(predicate);
        }

        #endregion

        #region Authentication Queries

        /// <summary>
        /// Busca usu�rio por nome de usu�rio.
        /// </summary>
        /// <param name="username">Nome de usu�rio</param>
        /// <returns>Usu�rio encontrado ou null</returns>
        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            return await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        /// <summary>
        /// Verifica se um username j� est� em uso.
        /// </summary>
        /// <param name="username">Nome de usu�rio</param>
        /// <param name="excludeUserId">ID do usu�rio a excluir da verifica��o (�til em updates)</param>
        /// <returns>True se o username j� existe</returns>
        public async Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            var query = _context.Users.Where(u => u.Username == username);

            if (excludeUserId.HasValue)
                query = query.Where(u => u.UserId != excludeUserId.Value);

            return await query.AnyAsync();
        }

        /// <summary>
        /// Verifica se existe pelo menos um usu�rio com privil�gios de admin.
        /// </summary>
        /// <returns>True se existir usu�rio admin</returns>
        public async Task<bool> HasAdminsAsync()
        {
            
            return await _context.UserPrivileges
                .AnyAsync(up => up.Privilege != null && up.Privilege.Code == "ADMIN");
        }

        /// <summary>
        /// Busca usu�rio por email (atrav�s de Person).
        /// </summary>
        /// <param name="email">Email do usu�rio</param>
        /// <returns>Usu�rio encontrado ou null</returns>
        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Person != null && u.Person.Email == email);
        }

        #endregion

        #region Status Queries

        /// <summary>
        /// Retorna usu�rios ativos.
        /// </summary>
        /// <returns>Lista de usu�rios com status Active</returns>
        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Person)
                .Where(u => u.Status == LoginStatus.Active)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna usu�rios bloqueados.
        /// </summary>
        /// <returns>Lista de usu�rios com LockedUntil v�lido</returns>
        public async Task<IEnumerable<User>> GetLockedUsersAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Users
                .Include(u => u.Person)
                .Where(u => u.LockedUntil.HasValue && u.LockedUntil.Value > now)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Retorna usu�rios por status espec�fico.
        /// </summary>
        /// <param name="status">Status do login</param>
        /// <returns>Lista de usu�rios com o status especificado</returns>
        public async Task<IEnumerable<User>> GetByStatusAsync(LoginStatus status)
        {
            return await _context.Users
                .Include(u => u.Person)
                .Where(u => u.Status == status)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region Advanced Queries

        /// <summary>
        /// Busca usu�rios com privil�gios espec�ficos.
        /// </summary>
        /// <returns>Lista de usu�rios com seus privil�gios carregados</returns>
        public async Task<IEnumerable<User>> GetUsersWithPrivilegesAsync()
        {
            return await _context.Users
                .Include(u => u.Person)
                .Include(u => u.UserPrivileges)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Verifica se existe pelo menos um usu�rio ativo no sistema.
        /// </summary>
        /// <returns>True se existir usu�rio ativo</returns>
        public async Task<bool> HasActiveUsersAsync()
        {
            return await _context.Users
                .AnyAsync(u => u.Status == LoginStatus.Active);
        }

        /// <summary>
        /// Retorna usu�rios que falharam login recentemente.
        /// </summary>
        /// <param name="minFailedAttempts">N�mero m�nimo de tentativas falhadas</param>
        /// <returns>Lista de usu�rios com tentativas falhadas</returns>
        public async Task<IEnumerable<User>> GetUsersWithFailedAttemptsAsync(int minFailedAttempts = 3)
        {
            return await _context.Users
                .Include(u => u.Person)
                .Where(u => u.FailedLoginAttempts >= minFailedAttempts)
                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// Adiciona m�ltiplos usu�rios em uma �nica transa��o.
        /// </summary>
        /// <param name="users">Lista de usu�rios a serem adicionados</param>
        /// <returns>Task completado</returns>
        public async Task AddRangeAsync(IEnumerable<User> users)
        {
            if (users == null || !users.Any())
                throw new ArgumentException("Lista de usu�rios n�o pode ser vazia.", nameof(users));

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Atualiza m�ltiplos usu�rios em uma �nica transa��o.
        /// </summary>
        /// <param name="users">Lista de usu�rios a serem atualizados</param>
        /// <returns>Task completado</returns>
        public async Task UpdateRangeAsync(IEnumerable<User> users)
        {
            if (users == null || !users.Any())
                throw new ArgumentException("Lista de usu�rios n�o pode ser vazia.", nameof(users));

            foreach (var user in users)
            {
                _context.Users.Attach(user);
                _context.Entry(user).State = EntityState.Modified;

                var personRef = _context.Entry(user).Reference(u => u.Person);
                if (personRef.IsLoaded && personRef.TargetEntry != null)
                {
                    personRef.TargetEntry.State = EntityState.Unchanged;
                }
            }

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}