using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CommerceHub.Data.Repositories
{
    /// <summary>
    /// Interface genérica para operações básicas de repositório.
    /// </summary>
    /// <typeparam name="T">Tipo da entidade.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Obtém uma entidade pelo ID.
        /// </summary>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Obtém todas as entidades.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Busca entidades por expressão lambda.
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
        /// Obtém entidades paginadas.
        /// </summary>
        Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize);
    }
}
