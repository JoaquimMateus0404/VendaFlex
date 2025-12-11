using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Entities;
using System.Linq.Expressions;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Interface para serviços de gerenciamento de usuários do sistema.
    /// Define operações CRUD, autenticação, segurança e consultas avançadas.
    /// </summary>
    public interface IUserService
    {
        #region CRUD Operations

        /// <summary>
        /// Busca um usuário por ID.
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <returns>Resultado com UserDto se encontrado, ou falha se não existir</returns>
        Task<OperationResult<UserDto>> GetByIdAsync(int id);

        /// <summary>
        /// Busca um usuário por ID sem tracking (otimizado para leitura).
        /// </summary>
        /// <param name="id">ID do usuário</param>
        /// <returns>Resultado com UserDto se encontrado, ou falha se não existir</returns>
        Task<OperationResult<UserDto>> GetByIdAsNoTrackingAsync(int id);

        /// <summary>
        /// Retorna todos os usuários do sistema.
        /// </summary>
        /// <returns>Resultado com lista de usuários</returns>
        Task<OperationResult<IEnumerable<UserDto>>> GetAllAsync();

        /// <summary>
        /// Busca usuários usando um predicado customizado.
        /// </summary>
        /// <param name="predicate">Expressão lambda para filtro</param>
        /// <returns>Resultado com lista de usuários que atendem ao predicado</returns>
        Task<OperationResult<IEnumerable<UserDto>>> FindAsync(Expression<Func<User, bool>> predicate);

        /// <summary>
        /// Atualiza os dados de um usuário existente.
        /// </summary>
        /// <param name="dto">Dados atualizados do usuário</param>
        /// <returns>Resultado com UserDto atualizado ou falha</returns>
        Task<OperationResult<UserDto>> UpdateAsync(UserDto dto);

        /// <summary>
        /// Remove um usuário do sistema.
        /// </summary>
        /// <param name="id">ID do usuário a ser removido</param>
        /// <returns>Resultado indicando sucesso ou falha da operação</returns>
        Task<OperationResult> DeleteAsync(int id);

        #endregion

        #region Pagination

        /// <summary>
        /// Retorna usuários paginados.
        /// </summary>
        /// <param name="page">Número da página (inicia em 1)</param>
        /// <param name="pageSize">Quantidade de itens por página</param>
        /// <returns>Resultado com lista paginada de usuários</returns>
        Task<OperationResult<IEnumerable<UserDto>>> GetPagedAsync(int page, int pageSize);

        /// <summary>
        /// Retorna o total de usuários cadastrados.
        /// </summary>
        /// <returns>Quantidade total de usuários</returns>
        Task<int> GetTotalCountAsync();

        /// <summary>
        /// Retorna o total de usuários que atendem a um predicado.
        /// </summary>
        /// <param name="predicate">Expressão lambda para filtro</param>
        /// <returns>Resultado com contagem de usuários</returns>
        Task<OperationResult<int>> GetCountAsync(Expression<Func<User, bool>> predicate);

        #endregion

        #region Authentication & Credentials

        /// <summary>
        /// Autentica um usuário no sistema.
        /// </summary>
        /// <param name="username">Nome de usuário</param>
        /// <param name="password">Senha em texto plano</param>
        /// <returns>Resultado com UserDto se autenticado, ou falha com mensagem de erro</returns>
        Task<OperationResult<UserDto>> LoginAsync(string username, string password);

        /// <summary>
        /// Registra um novo usuário no sistema.
        /// </summary>
        /// <param name="dto">Dados do usuário</param>
        /// <param name="password">Senha em texto plano</param>
        /// <returns>Resultado com UserDto criado ou falha com validações</returns>
        Task<OperationResult<UserDto>> RegisterAsync(UserDto dto, string password);

        /// <summary>
        /// Altera a senha de um usuário.
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="currentPassword">Senha atual para validação</param>
        /// <param name="newPassword">Nova senha</param>
        /// <returns>Resultado indicando sucesso ou falha</returns>
        Task<OperationResult> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        /// <summary>
        /// Inicia processo de recuperação de senha via email.
        /// </summary>
        /// <param name="email">Email do usuário</param>
        /// <returns>Resultado indicando se o email foi enviado</returns>
        Task<OperationResult> ResetPasswordAsync(string email);

        /// <summary>
        /// Realiza logout do usuário (invalida sessão/token).
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <returns>Resultado da operação de logout</returns>
        Task<OperationResult> LogoutAsync(int userId);

        #endregion

        #region Account Security

        /// <summary>
        /// Bloqueia uma conta de usuário indefinidamente ou por tempo determinado.
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <param name="durationMinutes">Duração do bloqueio em minutos (0 = indefinido)</param>
        /// <returns>Resultado da operação de bloqueio</returns>
        Task<OperationResult> LockUserAsync(int userId, int durationMinutes = 0);

        /// <summary>
        /// Desbloqueia uma conta de usuário previamente bloqueada.
        /// </summary>
        /// <param name="userId">ID do usuário</param>
        /// <returns>Resultado da operação de desbloqueio</returns>
        Task<OperationResult> UnlockUserAsync(int userId);

        #endregion

        #region Query Methods

        /// <summary>
        /// Busca um usuário por nome de usuário.
        /// </summary>
        /// <param name="username">Nome de usuário</param>
        /// <returns>Resultado com UserDto se encontrado</returns>
        Task<OperationResult<UserDto>> GetByUsernameAsync(string username);

        /// <summary>
        /// Busca um usuário por email.
        /// </summary>
        /// <param name="email">Email do usuário</param>
        /// <returns>Resultado com UserDto se encontrado</returns>
        Task<OperationResult<UserDto>> GetByEmailAsync(string email);

        /// <summary>
        /// Busca um usuário por PersonId.
        /// </summary>
        /// <param name="personId">ID da pessoa</param>
        /// <returns>Resultado com UserDto se encontrado</returns>
        Task<OperationResult<UserDto>> GetByPersonIdAsync(int personId);

        /// <summary>
        /// Verifica se um username já está em uso.
        /// </summary>
        /// <param name="username">Nome de usuário a verificar</param>
        /// <param name="excludeUserId">ID do usuário a excluir da verificação (útil em updates)</param>
        /// <returns>True se o username já existe</returns>
        Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null);

        /// <summary>
        /// Retorna usuários ativos do sistema.
        /// </summary>
        /// <returns>Resultado com lista de usuários ativos</returns>
        Task<OperationResult<IEnumerable<UserDto>>> GetActiveUsersAsync();

        /// <summary>
        /// Retorna usuários bloqueados.
        /// </summary>
        /// <returns>Resultado com lista de usuários bloqueados</returns>
        Task<OperationResult<IEnumerable<UserDto>>> GetLockedUsersAsync();

        /// <summary>
        /// Retorna usuários por status específico.
        /// </summary>
        /// <param name="status">Status do login</param>
        /// <returns>Resultado com lista de usuários do status especificado</returns>
        Task<OperationResult<IEnumerable<UserDto>>> GetByStatusAsync(LoginStatus status);

        /// <summary>
        /// Retorna usuários que falharam login recentemente.
        /// </summary>
        /// <param name="minFailedAttempts">Número mínimo de tentativas falhadas</param>
        /// <returns>Resultado com lista de usuários com tentativas falhadas</returns>
        Task<OperationResult<IEnumerable<UserDto>>> GetUsersWithFailedAttemptsAsync(int minFailedAttempts = 3);

        /// <summary>
        /// Retorna usuários com seus privilégios carregados.
        /// </summary>
        /// <returns>Resultado com lista de usuários com privilégios</returns>
        Task<OperationResult<IEnumerable<UserDto>>> GetUsersWithPrivilegesAsync();

        #endregion

        #region Batch Operations

        /// <summary>
        /// Adiciona múltiplos usuários em uma única transação.
        /// </summary>
        /// <param name="dtos">Lista de usuários a serem adicionados</param>
        /// <param name="passwords">Lista de senhas correspondentes</param>
        /// <returns>Resultado com lista de usuários criados</returns>
        Task<OperationResult<IEnumerable<UserDto>>> AddRangeAsync(IEnumerable<UserDto> dtos, IEnumerable<string> passwords);

        /// <summary>
        /// Atualiza múltiplos usuários em uma única transação.
        /// </summary>
        /// <param name="dtos">Lista de usuários a serem atualizados</param>
        /// <returns>Resultado com lista de usuários atualizados</returns>
        Task<OperationResult<IEnumerable<UserDto>>> UpdateRangeAsync(IEnumerable<UserDto> dtos);

        #endregion

        #region System Verification

        /// <summary>
        /// Verifica se existe pelo menos um administrador no sistema.
        /// </summary>
        /// <returns>True se existir administrador, false caso contrário</returns>
        Task<bool> HasAdminsAsync();

        /// <summary>
        /// Verifica se existe pelo menos um usuário ativo no sistema.
        /// </summary>
        /// <returns>True se existir usuário ativo</returns>
        Task<bool> HasActiveUsersAsync();

        #endregion
    }
}