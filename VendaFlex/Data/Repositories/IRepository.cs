using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CommerceHub.Data.Repositories
{
    /// <summary>
    /// Interface gen�rica para opera��es b�sicas de reposit�rio.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Obt�m uma entidade pelo ID.
        /// </summary>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Obt�m todas as entidades.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Busca entidades por express�o lambda.
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Adiciona uma nova entidade.
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Atualiza uma entidade existente.
        /// </summary>
        Task<T> UpdateAsync(T entity);

        /// <summary>
        /// Remove uma entidade pelo ID.
        /// </summary>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Obt�m entidades paginadas.
        /// </summary>
        Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize);
    }
}
