using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendaFlex.Core.DTOs;

namespace VendaFlex.Infrastructure.Interfaces
{
    /// <summary>
    /// Serviço responsável por operações de inventário, incluindo exportação de dados
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Exporta os dados do inventário para um arquivo Excel formatado profissionalmente
        /// </summary>
        /// <param name="inventoryItems">Lista de itens do inventário</param>
        /// <param name="filePath">Caminho completo do arquivo a ser gerado</param>
        /// <param name="totalEntries">Total de entradas no inventário</param>
        /// <param name="totalExits">Total de saídas no inventário</param>
        /// <param name="totalValue">Valor total do inventário</param>
        /// <returns>Task representando a operação assíncrona</returns>
        Task ExportInventoryToExcelAsync(
            IEnumerable<InventoryItemDto> inventoryItems,
            string filePath,
            decimal totalEntries,
            decimal totalExits,
            decimal totalValue
        );

    }
}