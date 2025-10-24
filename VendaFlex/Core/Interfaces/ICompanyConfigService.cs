using VendaFlex.Core.DTOs;
using VendaFlex.Core.Utils;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Interface para servi�os de configura��o da empresa.
    /// Define opera��es de gerenciamento das configura��es globais do sistema.
    /// </summary>
    public interface ICompanyConfigService
    {
        /// <summary>
        /// Obt�m a configura��o atual da empresa.
        /// Retorna configura��o padr�o se nenhuma existir.
        /// </summary>
        /// <returns>Resultado com configura��o da empresa</returns>
        Task<OperationResult<CompanyConfigDto>> GetAsync();

        /// <summary>
        /// Atualiza ou cria a configura��o da empresa (Upsert).
        /// </summary>
        /// <param name="dto">Dados da configura��o</param>
        /// <returns>Resultado com configura��o salva</returns>
        Task<OperationResult<CompanyConfigDto>> UpdateAsync(CompanyConfigDto dto);

        /// <summary>
        /// Verifica se a configura��o inicial j� foi realizada.
        /// </summary>
        /// <returns>True se a configura��o existe</returns>
        Task<bool> IsConfiguredAsync();

        /// <summary>
        /// Obt�m e incrementa o pr�ximo n�mero de fatura.
        /// </summary>
        /// <returns>Resultado com o pr�ximo n�mero dispon�vel</returns>
        Task<OperationResult<int>> GetNextInvoiceNumberAsync();

        /// <summary>
        /// Gera o n�mero completo da fatura (Prefixo + N�mero).
        /// </summary>
        /// <returns>Resultado com n�mero formatado (ex: INV-00001)</returns>
        Task<OperationResult<string>> GenerateInvoiceNumberAsync();

        /// <summary>
        /// Atualiza apenas a URL do logo.
        /// </summary>
        /// <param name="logoUrl">Novo caminho do logo</param>
        /// <returns>Resultado da opera��o</returns>
        Task<OperationResult> UpdateLogoAsync(string logoUrl);

        /// <summary>
        /// Remove o logo da empresa.
        /// </summary>
        /// <returns>Resultado da opera��o</returns>
        Task<OperationResult> RemoveLogoAsync();

        /// <summary>
        /// Ativa a configura��o da empresa.
        /// </summary>
        /// <returns>Resultado da opera��o</returns>
        Task<OperationResult> ActivateAsync();

        /// <summary>
        /// Desativa a configura��o da empresa.
        /// </summary>
        /// <returns>Resultado da opera��o</returns>
        Task<OperationResult> DeactivateAsync();
    }
}