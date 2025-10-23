using System;
using System.IO;
using System.Threading.Tasks;
using VendaFlex.Core.Interfaces;
using VendaFlex.Infrastructure.Interfaces;

namespace VendaFlex.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB
        private readonly string _uploadsDirectory;

        public FileStorageService()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _uploadsDirectory = Path.Combine(localAppData, "VendaFlex", "uploads");

            // Garantir que o diretório existe
            Directory.CreateDirectory(_uploadsDirectory);
        }

        public async Task<string> SaveLogoAsync(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                throw new ArgumentException("Caminho de origem não pode ser vazio", nameof(sourcePath));

            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Ficheiro de origem não encontrado", sourcePath);

            // Validar tamanho
            var fileInfo = new FileInfo(sourcePath);
            if (fileInfo.Length > MaxFileSizeBytes)
            {
                throw new InvalidOperationException(
                    $"Ficheiro excede o tamanho máximo permitido de {MaxFileSizeBytes / (1024 * 1024)} MB");
            }

            // Validar se é imagem
            if (!IsValidImage(sourcePath))
            {
                throw new InvalidOperationException("O ficheiro não é uma imagem válida");
            }

            // Gerar nome único preservando a extensão
            var extension = Path.GetExtension(sourcePath);
            var fileName = Path.GetFileNameWithoutExtension(sourcePath);
            var uniqueName = $"{fileName}_{Guid.NewGuid():N}{extension}";
            var destinationPath = Path.Combine(_uploadsDirectory, uniqueName);

            // Copiar ficheiro de forma assíncrona
            using (var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            using (var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }

            return destinationPath;
        }

        public async Task DeleteFileAsync(string storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
                return;

            if (File.Exists(storedPath))
            {
                await Task.Run(() => File.Delete(storedPath));
            }
        }

        public bool IsValidImage(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                // Verificar extensão
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                    return false;

                // Verificar magic bytes (assinatura do ficheiro)
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var header = new byte[8];
                    stream.Read(header, 0, 8);

                    // PNG: 89 50 4E 47 0D 0A 1A 0A
                    if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
                        return true;

                    // JPEG: FF D8 FF
                    if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
