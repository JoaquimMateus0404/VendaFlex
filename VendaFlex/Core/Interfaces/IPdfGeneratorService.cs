using VendaFlex.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VendaFlex.Core.Interfaces
{
    /// <summary>
    /// Serviço para geração de documentos PDF
    /// </summary>
    public interface IPdfGeneratorService
    {
        /// <summary>
        /// Gera um PDF da fatura e salva no caminho especificado
        /// </summary>
        /// <param name="companyConfig">Configurações da empresa</param>
        /// <param name="invoice">Dados da fatura</param>
        /// <param name="items">Itens da fatura</param>
        /// <param name="customer">Dados do cliente (opcional)</param>
        /// <param name="filePath">Caminho onde o PDF será salvo</param>
        Task GenerateInvoicePdfAsync(
            CompanyConfigDto companyConfig,
            InvoiceDto invoice,
            IEnumerable<InvoiceProductDto> items,
            PersonDto? customer,
            string filePath);
    }
}
