using System.Security.Cryptography;
using VendaFlex.Core.Interfaces;

namespace VendaFlex.Infrastructure.Services
{
    /// <summary>
    /// Implementação mais segura usando PBKDF2 (recomendado para produção).
    /// Usa salt aleatório e múltiplas iterações para dificultar ataques de força bruta.
    /// </summary>
    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 32; // 256 bits
        private const int Iterations = 100000; // Recomendado OWASP 2023

        /// <summary>
        /// Gera hash PBKDF2 com salt aleatório.
        /// Formato: iterations.salt.hash (todos em Base64)
        /// </summary>
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Senha não pode ser vazia.", nameof(password));

            // Gerar salt aleatório
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Gerar hash usando PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256);

            byte[] hash = pbkdf2.GetBytes(HashSize);

            // Retornar formato: iterations.salt.hash
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Verifica senha comparando com hash PBKDF2 armazenado.
        /// </summary>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            try
            {
                // Extrair iterations, salt e hash
                var parts = hashedPassword.Split('.');
                if (parts.Length != 3)
                    return false;

                int iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] storedHash = Convert.FromBase64String(parts[2]);

                // Gerar hash da senha fornecida com o mesmo salt
                using var pbkdf2 = new Rfc2898DeriveBytes(
                    password,
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256);

                byte[] testHash = pbkdf2.GetBytes(HashSize);

                // Comparação em tempo constante para evitar timing attacks
                return CryptographicOperations.FixedTimeEquals(storedHash, testHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
