using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Interface para serviços de configuração da empresa.
    /// Define operações de gerenciamento das configurações globais do sistema.
    /// </summary>
    public interface ICompanyConfigService
    {
        /// <summary>
        /// Obtém a configuração atual da empresa.
        /// Retorna configuração padrão se nenhuma existir.
        /// </summary>
        /// <returns>Resultado com configuração da empresa</returns>
        Task<OperationResult<CompanyConfigDto>> GetAsync();

        /// <summary>
        /// Atualiza ou cria a configuração da empresa (Upsert).
        /// </summary>
        /// <param name="dto">Dados da configuração</param>
        /// <returns>Resultado com configuração salva</returns>
        Task<OperationResult<CompanyConfigDto>> UpdateAsync(CompanyConfigDto dto);

        /// <summary>
        /// Verifica se a configuração inicial já foi realizada.
        /// </summary>
        /// <returns>True se a configuração existe</returns>
        Task<bool> IsConfiguredAsync();

        /// <summary>
        /// Obtém e incrementa o próximo número de fatura.
        /// </summary>
        /// <returns>Resultado com o próximo número disponível</returns>
        Task<OperationResult<int>> GetNextInvoiceNumberAsync();

        /// <summary>
        /// Gera o número completo da fatura (Prefixo + Número).
        /// </summary>
        /// <returns>Resultado com número formatado (ex: INV-00001)</returns>
        Task<OperationResult<string>> GenerateInvoiceNumberAsync();

        /// <summary>
        /// Atualiza apenas a URL do logo.
        /// </summary>
        /// <param name="logoUrl">Novo caminho do logo</param>
        /// <returns>Resultado da operação</returns>
        Task<OperationResult> UpdateLogoAsync(string logoUrl);

        /// <summary>
        /// Remove o logo da empresa.
        /// </summary>
        /// <returns>Resultado da operação</returns>
        Task<OperationResult> RemoveLogoAsync();

        /// <summary>
        /// Ativa a configuração da empresa.
        /// </summary>
        /// <returns>Resultado da operação</returns>
        Task<OperationResult> ActivateAsync();

        /// <summary>
        /// Desativa a configuração da empresa.
        /// </summary>
        /// <returns>Resultado da operação</returns>
        Task<OperationResult> DeactivateAsync();
    }
}