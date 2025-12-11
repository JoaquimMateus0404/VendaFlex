using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendaFlex.Core.DTOs;
using VendaFlex.Infrastructure.Interfaces;

namespace VendaFlex.Infrastructure.Services
{
    /// <summary>
    /// Implementação do serviço de inventário com exportação profissional para Excel
    /// </summary>
    public class InventoryService : IInventoryService
    {
        public async Task ExportInventoryToExcelAsync(
            IEnumerable<InventoryItemDto> inventoryItems,
            string filePath,
            decimal totalEntries,
            decimal totalExits,
            decimal totalValue)
        {
            await Task.Run(() =>
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Inventário");

                // CABEÇALHO PRINCIPAL
                CreateHeader(worksheet);

                // CABEÇALHO DAS COLUNAS
                CreateColumnHeaders(worksheet);

                // DADOS DO INVENTÁRIO
                PopulateInventoryData(worksheet, inventoryItems);

                // RODAPÉ COM TOTAIS
                CreateFooter(worksheet, inventoryItems.Count(), totalEntries, totalExits, totalValue);

                // FORMATAÇÃO FINAL
                ApplyFinalFormatting(worksheet);

                // SALVAR ARQUIVO
                workbook.SaveAs(filePath);
            });
        }

        private void CreateHeader(IXLWorksheet worksheet)
        {
            // Título principal
            worksheet.Cell("A1").Value = "RELATÓRIO DE INVENTÁRIO";
            worksheet.Range("A1:H1").Merge();

            var headerRange = worksheet.Range("A1:H1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 16;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2C3E50");
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Row(1).Height = 30;

            // Data de geração
            worksheet.Cell("A2").Value = $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}";
            worksheet.Range("A2:H2").Merge();
            worksheet.Range("A2:H2").Style.Font.Italic = true;
            worksheet.Range("A2:H2").Style.Font.FontSize = 10;
            worksheet.Range("A2:H2").Style.Font.FontColor = XLColor.FromHtml("#7F8C8D");
            worksheet.Range("A2:H2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private void CreateColumnHeaders(IXLWorksheet worksheet)
        {
            var headers = new[]
            {
                ("A4", "Código", 12),
                ("B4", "Descrição", 35),
                ("C4", "Categoria", 20),
                ("D4", "Qtd Entrada", 14),
                ("E4", "Qtd Saída", 14),
                ("F4", "Disponível", 14),
                ("G4", "Custo Unit.", 14),
                ("H4", "Valor Total", 16)
            };

            foreach (var (cell, title, width) in headers)
            {
                var headerCell = worksheet.Cell(cell);
                headerCell.Value = title;
                headerCell.Style.Font.Bold = true;
                headerCell.Style.Font.FontSize = 11;
                headerCell.Style.Font.FontColor = XLColor.White;
                headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#34495E");
                headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerCell.Style.Border.OutsideBorderColor = XLColor.White;

                worksheet.Column(cell[0] - 'A' + 1).Width = width;
            }

            worksheet.Row(4).Height = 25;
        }

        private void PopulateInventoryData(IXLWorksheet worksheet, IEnumerable<InventoryItemDto> inventoryItems)
        {
            int row = 5;
            bool alternateColor = false;

            foreach (var item in inventoryItems)
            {
                worksheet.Cell(row, 1).Value = item.InternalCode;
                worksheet.Cell(row, 2).Value = item.ProductName;
                worksheet.Cell(row, 3).Value = item.CategoryName;
                worksheet.Cell(row, 4).Value = item.TotalEntries;
                worksheet.Cell(row, 5).Value = item.TotalExits;
                worksheet.Cell(row, 6).Value = item.AvailableQuantity;
                worksheet.Cell(row, 7).Value = item.CostPrice;
                worksheet.Cell(row, 8).Value = item.TotalValue;

                // Formatação de linha
                var rowRange = worksheet.Range(row, 1, row, 8);

                // Cor alternada
                if (alternateColor)
                {
                    rowRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#ECF0F1");
                }

                // Alinhamento
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Formato de moeda
                worksheet.Cell(row, 7).Style.NumberFormat.Format = "KZs #,##0.00";
                worksheet.Cell(row, 8).Style.NumberFormat.Format = "KZs #,##0.00";

                // Formato de números
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                worksheet.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";

                // Bordas
                rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                rowRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#BDC3C7");

                alternateColor = !alternateColor;
                row++;
            }
        }

        private void CreateFooter(IXLWorksheet worksheet, int itemCount, decimal totalEntries, decimal totalExits, decimal totalValue)
        {
            int footerRow = 5 + itemCount + 2;

            // Linha de separação
            var separatorRange = worksheet.Range(footerRow - 1, 1, footerRow - 1, 8);
            separatorRange.Style.Border.TopBorder = XLBorderStyleValues.Double;
            separatorRange.Style.Border.TopBorderColor = XLColor.FromHtml("#2C3E50");

            // Célula de título "TOTAIS"
            worksheet.Cell(footerRow, 1).Value = "TOTAIS";
            worksheet.Range(footerRow, 1, footerRow, 2).Merge();
            var totalsLabelCell = worksheet.Range(footerRow, 1, footerRow, 2);
            totalsLabelCell.Style.Font.Bold = true;
            totalsLabelCell.Style.Font.FontSize = 12;
            totalsLabelCell.Style.Font.FontColor = XLColor.White;
            totalsLabelCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E74C3C");
            totalsLabelCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            totalsLabelCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // Total de Entradas
            worksheet.Cell(footerRow, 3).Value = "Total Entradas:";
            worksheet.Cell(footerRow, 4).Value = totalEntries;
            worksheet.Cell(footerRow, 4).Style.NumberFormat.Format = "#,##0.00";

            // Total de Saídas
            worksheet.Cell(footerRow, 5).Value = "Total Saídas:";
            worksheet.Cell(footerRow, 6).Value = totalExits;
            worksheet.Cell(footerRow, 6).Style.NumberFormat.Format = "#,##0.00";

            // Valor Total
            worksheet.Cell(footerRow, 7).Value = "Valor Total:";
            worksheet.Cell(footerRow, 8).Value = totalValue;
            worksheet.Cell(footerRow, 8).Style.NumberFormat.Format = "KZs #,##0.00";

            // Formatação do rodapé
            var footerRange = worksheet.Range(footerRow, 1, footerRow, 8);
            footerRange.Style.Font.Bold = true;
            footerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#FDECEA");
            footerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            footerRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#E74C3C");

            worksheet.Row(footerRow).Height = 25;
        }

        private void ApplyFinalFormatting(IXLWorksheet worksheet)
        {
            // Congelar painéis (cabeçalho fixo)
            worksheet.SheetView.FreezeRows(4);

            // Ajuste automático de colunas (opcional, já definimos manualmente)
            // worksheet.Columns().AdjustToContents();

            // Zoom padrão
            worksheet.SheetView.ZoomScale = 90;
        }
    }
}
