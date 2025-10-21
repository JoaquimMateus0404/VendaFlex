using System.Threading.Tasks;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Serviço para gerenciar armazenamento de ficheiros (logos, uploads, etc.)
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Salva um ficheiro de logo da empresa para o diretório de uploads
        /// </summary>
        /// <param name="sourcePath">Caminho de origem do ficheiro selecionado</param>
        /// <returns>Caminho absoluto do ficheiro salvo no diretório de uploads</returns>
        Task<string> SaveLogoAsync(string sourcePath);

        /// <summary>
        /// Remove um ficheiro armazenado
        /// </summary>
        /// <param name="storedPath">Caminho do ficheiro a ser removido</param>
        Task DeleteFileAsync(string storedPath);

        /// <summary>
        /// Valida se um ficheiro é uma imagem válida
        /// </summary>
        /// <param name="filePath">Caminho do ficheiro a validar</param>
        /// <returns>True se for uma imagem válida</returns>
        bool IsValidImage(string filePath);
    }
}
