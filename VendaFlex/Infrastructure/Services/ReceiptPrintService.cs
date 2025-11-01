using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;

namespace VendaFlex.Infrastructure.Services
{
    public class ReceiptPrintService : IReceiptPrintService
    {
        private readonly IProductService _productService;

        public ReceiptPrintService(IProductService productService)
        {
            _productService = productService;
        }

        // Imprimir fatura
        // Carregar dados necessários e construir o documento
        public async Task PrintAsync(CompanyConfigDto cfg, InvoiceDto invoice, IEnumerable<InvoiceProductDto> items, string format)
        {
            // Carregar mapa de produtos para nomes/descrições (mínimo de chamadas)
            var productMap = new Dictionary<int, ProductDto>();
            foreach (var pid in items.Select(i => i.ProductId).Distinct())
            {
                try
                {
                    var res = await _productService.GetByIdAsync(pid);
                    if (res.Success && res.Data != null)
                    {
                        productMap[pid] = res.Data;
                    }
                }
                catch
                {
                    // Ignorar falhas individuais
                }
            }

            FlowDocument doc;
            var formatType = ParseFormat(format);
            if (formatType == CompanyConfig.InvoiceFormatType.Rolo)
            {
                doc = BuildRollDocument(cfg, invoice, items, productMap);
            }
            else
            {
                doc = BuildA4Document(cfg, invoice, items, productMap);
            }

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    // Ajustar o documento ao tamanho da impressora
                    doc.PageHeight = pd.PrintableAreaHeight;
                    doc.PageWidth = pd.PrintableAreaWidth;
                    doc.PagePadding = new Thickness(40);
                    doc.ColumnWidth = double.PositiveInfinity;

                    var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                    pd.PrintDocument(paginator, $"VendaFlex - Fatura {invoice.InvoiceNumber}");
                }
            });
        }

        // Analisar o formato da fatura a partir da string
        private static CompanyConfig.InvoiceFormatType ParseFormat(string format)
        {
            if (Enum.TryParse<CompanyConfig.InvoiceFormatType>(format, out var ft))
                return ft;
            return CompanyConfig.InvoiceFormatType.A4;
        }

        // Construir documento para formato A4
        private FlowDocument BuildA4Document(CompanyConfigDto cfg, InvoiceDto invoice, IEnumerable<InvoiceProductDto> items, IDictionary<int, ProductDto> productMap)
        {
            var doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 12,
                PagePadding = new Thickness(40),
                ColumnWidth = double.PositiveInfinity
            };

            // Cabeçalho
            var header = new Paragraph
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 16,
                FontWeight = FontWeights.Bold
            };
            header.Inlines.Add(new Run(cfg.CompanyName ?? "Empresa"));
            doc.Blocks.Add(header);

            var subHeader = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 10) };
            subHeader.Inlines.Add(new Run($"NIF: {cfg.TaxId}    Tel: {cfg.PhoneNumber}"));
            doc.Blocks.Add(subHeader);

            var invInfo = new Paragraph { Margin = new Thickness(0, 10, 0, 10) };
            invInfo.Inlines.Add(new Run($"Fatura: {invoice.InvoiceNumber}    Data: {invoice.Date:dd/MM/yyyy HH:mm}"));
            doc.Blocks.Add(invInfo);

            // Tabela de Itens
            var table = new Table();
            table.Columns.Add(new TableColumn { Width = new GridLength(50) });   // Qtd
            table.Columns.Add(new TableColumn { Width = new GridLength(300) });  // Descrição
            table.Columns.Add(new TableColumn { Width = new GridLength(100) });  // Preço
            table.Columns.Add(new TableColumn { Width = new GridLength(100) });  // Desc
            table.Columns.Add(new TableColumn { Width = new GridLength(120) });  // Total

            var headerGroup = new TableRowGroup();
            var headerRow = new TableRow();
            headerRow.Cells.Add(HeaderCell("Qtd"));
            headerRow.Cells.Add(HeaderCell("Descrição"));
            headerRow.Cells.Add(HeaderCell("Preço"));
            headerRow.Cells.Add(HeaderCell("Desc"));
            headerRow.Cells.Add(HeaderCell("Total"));
            headerGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerGroup);

            var body = new TableRowGroup();
            foreach (var it in items)
            {
                var name = productMap.TryGetValue(it.ProductId, out var p) ? p.Name : $"Produto {it.ProductId}";
                var qty = it.Quantity;
                var unit = it.UnitPrice;
                var discPct = it.DiscountPercentage;
                var lineBase = unit * qty;
                var discVal = Math.Round(lineBase * (discPct / 100m), 2);
                var lineTotal = Math.Max(lineBase - discVal, 0);

                var row = new TableRow();
                row.Cells.Add(BodyCell(qty.ToString()));
                row.Cells.Add(BodyCell(name));
                row.Cells.Add(BodyCell(unit.ToString("N2")));
                row.Cells.Add(BodyCell(discVal > 0 ? $"{discPct:N2}%" : "-"));
                row.Cells.Add(BodyCell(lineTotal.ToString("N2")));
                body.Rows.Add(row);
            }
            table.RowGroups.Add(body);

            doc.Blocks.Add(table);

            // Totais
            var totals = new Paragraph { TextAlignment = TextAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
            totals.Inlines.Add(new Run($"Subtotal: {invoice.SubTotal:N2}\n"));
            totals.Inlines.Add(new Run($"Descontos: {invoice.DiscountAmount:N2}\n"));
            totals.Inlines.Add(new Run($"Imposto: {invoice.TaxAmount:N2}\n"));
            totals.Inlines.Add(new Run($"Total: {invoice.Total:N2}\n") { FontWeight = FontWeights.Bold });
            if (invoice.PaidAmount > 0)
            {
                var change = Math.Max(invoice.PaidAmount - invoice.Total, 0);
                totals.Inlines.Add(new Run($"Pago: {invoice.PaidAmount:N2}\n"));
                totals.Inlines.Add(new Run($"Troco: {change:N2}"));
            }
            doc.Blocks.Add(totals);

            if (!string.IsNullOrWhiteSpace(cfg.InvoiceFooterText))
            {
                var footer = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 20, 0, 0), FontSize = 10 };
                footer.Inlines.Add(new Run(cfg.InvoiceFooterText));
                doc.Blocks.Add(footer);
            }

            return doc;
        }

        // Construir documento para formato rolo (80mm)
        private FlowDocument BuildRollDocument(CompanyConfigDto cfg, InvoiceDto invoice, IEnumerable<InvoiceProductDto> items, IDictionary<int, ProductDto> productMap)
        {
            // 80mm ~ 302 DIP (96 dpi)
            const double rollWidth = 302;

            var doc = new FlowDocument
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11,
                PagePadding = new Thickness(10),
                ColumnWidth = double.PositiveInfinity,
                PageWidth = rollWidth
            };

            var header = new Paragraph { TextAlignment = TextAlignment.Center, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 5) };
            header.Inlines.Add(new Run(cfg.CompanyName ?? "Empresa"));
            doc.Blocks.Add(header);

            var invInfo = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 5) };
            invInfo.Inlines.Add(new Run($"Fatura: {invoice.InvoiceNumber}\n{invoice.Date:dd/MM/yyyy HH:mm}"));
            doc.Blocks.Add(invInfo);

            // Cabeçalho simples
            doc.Blocks.Add(new Paragraph(new Run("QTD  DESCRIÇÃO              TOTAL")) { FontWeight = FontWeights.Bold });
            doc.Blocks.Add(new Paragraph(new Run(new string('-', 34))) { Margin = new Thickness(0, 0, 0, 2) });

            foreach (var it in items)
            {
                var name = productMap.TryGetValue(it.ProductId, out var p) ? p.Name : $"Produto {it.ProductId}";
                var qty = it.Quantity;
                var unit = it.UnitPrice;
                var lineBase = unit * qty;
                var discVal = Math.Round(lineBase * (it.DiscountPercentage / 100m), 2);
                var lineTotal = Math.Max(lineBase - discVal, 0);

                var line = $"{qty,3}  {Truncate(name, 18),-18}  {lineTotal,8:N2}";
                doc.Blocks.Add(new Paragraph(new Run(line)) { Margin = new Thickness(0, 0, 0, 0) });
            }

            doc.Blocks.Add(new Paragraph(new Run(new string('-', 34))) { Margin = new Thickness(0, 2, 0, 2) });

            var totals = new Paragraph { TextAlignment = TextAlignment.Right };
            totals.Inlines.Add(new Run($"Subtotal: {invoice.SubTotal:N2}\n"));
            totals.Inlines.Add(new Run($"Desc: {invoice.DiscountAmount:N2}\n"));
            totals.Inlines.Add(new Run($"Imposto: {invoice.TaxAmount:N2}\n"));
            totals.Inlines.Add(new Run($"Total: {invoice.Total:N2}\n") { FontWeight = FontWeights.Bold });
            if (invoice.PaidAmount > 0)
            {
                var change = Math.Max(invoice.PaidAmount - invoice.Total, 0);
                totals.Inlines.Add(new Run($"Pago: {invoice.PaidAmount:N2}\n"));
                totals.Inlines.Add(new Run($"Troco: {change:N2}"));
            }
            doc.Blocks.Add(totals);

            if (!string.IsNullOrWhiteSpace(cfg.InvoiceFooterText))
            {
                var footer = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 10, 0, 0), FontSize = 10 };
                footer.Inlines.Add(new Run(cfg.InvoiceFooterText));
                doc.Blocks.Add(footer);
            }

            return doc;
        }

        private static TableCell HeaderCell(string text)
        {
            return new TableCell(new Paragraph(new Run(text))
            {
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(2)
            });
        }

        private static TableCell BodyCell(string text)
        {
            return new TableCell(new Paragraph(new Run(text)) { Margin = new Thickness(2) });
        }

        private static string Truncate(string? s, int len)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Length <= len ? s : s.Substring(0, len);
        }
    }
}
