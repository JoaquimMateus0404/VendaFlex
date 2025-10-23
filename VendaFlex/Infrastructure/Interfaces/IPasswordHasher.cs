

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Interface para serviços de hash de senha.
    /// Abstrai a lógica de criptografia para facilitar testes e mudanças de algoritmo.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Gera hash de uma senha em texto plano.
        /// </summary>
        /// <param name="password">Senha em texto plano</param>
        /// <returns>Hash da senha</returns>
        string HashPassword(string password);

        /// <summary>
        /// Verifica se uma senha em texto plano corresponde ao hash armazenado.
        /// </summary>
        /// <param name="password">Senha em texto plano</param>
        /// <param name="hashedPassword">Hash armazenado</param>
        /// <returns>True se a senha corresponder ao hash</returns>
        bool VerifyPassword(string password, string hashedPassword);
    }
}