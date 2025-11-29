using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;

namespace VendaFlex.Infrastructure.Services
{
    /// <summary>
    /// Serviço para geração de PDFs usando QuestPDF
    /// </summary>
    public class PdfGeneratorService : IPdfGeneratorService
    {
        private readonly IProductService _productService;

        public PdfGeneratorService(IProductService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            
            // Configurar licença do QuestPDF (Community - gratuita)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task GenerateInvoicePdfAsync(
            CompanyConfigDto companyConfig,
            InvoiceDto invoice,
            IEnumerable<InvoiceProductDto> items,
            PersonDto? customer,
            string filePath)
        {
            try
            {
                // Carregar informações dos produtos
                var itemsList = await LoadProductInformationAsync(items);

                // Gerar o PDF
                await Task.Run(() =>
                {
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(40);
                            page.DefaultTextStyle(x => x.FontSize(10));

                            page.Header().Element(c => ComposeHeader(c, companyConfig, invoice));
                            page.Content().Element(c => ComposeContent(c, companyConfig, invoice, itemsList, customer));
                            page.Footer().Element(c => ComposeFooter(c, companyConfig));
                        });
                    })
                    .GeneratePdf(filePath);
                });
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Erro ao gerar PDF: {ex.Message}", ex);
            }
        }

        private async Task<List<InvoiceProductDto>> LoadProductInformationAsync(IEnumerable<InvoiceProductDto> items)
        {
            var result = new List<InvoiceProductDto>();

            foreach (var item in items)
            {
                var productResult = await _productService.GetByIdAsync(item.ProductId);
                if (productResult.Success && productResult.Data != null)
                {
                    item.ProductName = productResult.Data.Name;
                    item.ProductCode = productResult.Data.Code ?? productResult.Data.SKU ?? string.Empty;
                }
                else
                {
                    item.ProductName = $"Produto {item.ProductId}";
                    item.ProductCode = string.Empty;
                }
                result.Add(item);
            }

            return result;
        }

        private void ComposeHeader(IContainer container, CompanyConfigDto company, InvoiceDto invoice)
        {
            container.Row(row =>
            {
                // Coluna da esquerda - Logo e informações da empresa
                row.RelativeItem().Column(column =>
                {
                    // Logo (se existir)
                    if (!string.IsNullOrWhiteSpace(company.LogoUrl) && File.Exists(company.LogoUrl))
                    {
                        column.Item().MaxHeight(60).Image(company.LogoUrl);
                        column.Item().PaddingVertical(5);
                    }

                    // Nome da empresa
                    column.Item().Text(company.CompanyName)
                        .FontSize(16)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    // Informações da empresa
                    column.Item().PaddingTop(3).Text(text =>
                    {
                        text.Span($"NIF: {company.TaxId}").FontSize(9);
                        text.EmptyLine();
                        
                        if (!string.IsNullOrWhiteSpace(company.Address))
                        {
                            text.Span(company.Address).FontSize(8);
                            text.EmptyLine();
                        }
                        
                        if (!string.IsNullOrWhiteSpace(company.City))
                        {
                            var cityLine = company.City;
                            if (!string.IsNullOrWhiteSpace(company.PostalCode))
                                cityLine += $" - CEP: {company.PostalCode}";
                            text.Span(cityLine).FontSize(8);
                            text.EmptyLine();
                        }
                        
                        text.Span($"Tel: {company.PhoneNumber}").FontSize(8);
                        text.EmptyLine();
                        text.Span($"Email: {company.Email}").FontSize(8);
                    });
                });

                // Coluna da direita - Informações da fatura
                row.RelativeItem().Column(column =>
                {
                    column.Item().AlignRight().Text("FATURA")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    column.Item().PaddingTop(5).Background(Colors.Grey.Lighten3)
                        .Padding(10).AlignRight().Text(text =>
                    {
                        text.Span("Nº: ").FontSize(9);
                        text.Span(invoice.InvoiceNumber).FontSize(11).Bold();
                        text.EmptyLine();
                        text.Span("Data: ").FontSize(9);
                        text.Span(invoice.Date.ToString("dd/MM/yyyy HH:mm")).FontSize(9);
                        text.EmptyLine();
                        
                        if (invoice.DueDate.HasValue)
                        {
                            text.Span("Vencimento: ").FontSize(9);
                            text.Span(invoice.DueDate.Value.ToString("dd/MM/yyyy")).FontSize(9);
                            text.EmptyLine();
                        }

                        text.Span("Status: ").FontSize(9);
                        text.Span(GetStatusText(invoice.Status))
                            .FontSize(9)
                            .Bold()
                            .FontColor(GetStatusColor(invoice.Status));
                    });
                });
            });
        }

        private void ComposeContent(IContainer container, CompanyConfigDto company, InvoiceDto invoice, 
            List<InvoiceProductDto> items, PersonDto? customer)
        {
            container.PaddingVertical(20).Column(column =>
            {
                // Informações do cliente
                if (customer != null && company.IncludeCustomerData)
                {
                    column.Item().Element(c => ComposeCustomerInfo(c, customer));
                    column.Item().PaddingBottom(15);
                }

                // Tabela de itens
                column.Item().Element(c => ComposeItemsTable(c, items, company.CurrencySymbol));

                column.Item().PaddingTop(15);

                // Totais
                column.Item().Element(c => ComposeTotals(c, invoice, company.CurrencySymbol));

                // Notas/Observações
                if (!string.IsNullOrWhiteSpace(invoice.Notes))
                {
                    column.Item().PaddingTop(20).Element(c => ComposeNotes(c, invoice.Notes));
                }
            });
        }

        private void ComposeCustomerInfo(IContainer container, PersonDto customer)
        {
            container.Background(Colors.Grey.Lighten4).Padding(10).Column(column =>
            {
                column.Item().Text("CLIENTE").FontSize(11).Bold();
                column.Item().PaddingTop(5).Text(text =>
                {
                    text.Span(customer.Name).FontSize(10).Bold();
                    text.EmptyLine();
                    
                    if (!string.IsNullOrWhiteSpace(customer.TaxId))
                    {
                        text.Span($"NIF: {customer.TaxId}").FontSize(9);
                        text.EmptyLine();
                    }
                    
                    if (!string.IsNullOrWhiteSpace(customer.PhoneNumber))
                    {
                        text.Span($"Tel: {customer.PhoneNumber}").FontSize(9);
                        text.EmptyLine();
                    }
                    
                    if (!string.IsNullOrWhiteSpace(customer.Email))
                    {
                        text.Span($"Email: {customer.Email}").FontSize(9);
                        text.EmptyLine();
                    }
                    
                    if (!string.IsNullOrWhiteSpace(customer.Address))
                    {
                        var fullAddress = customer.Address;
                        if (!string.IsNullOrWhiteSpace(customer.City))
                            fullAddress += $", {customer.City}";
                        if (!string.IsNullOrWhiteSpace(customer.State))
                            fullAddress += $" - {customer.State}";
                        if (!string.IsNullOrWhiteSpace(customer.PostalCode))
                            fullAddress += $", CEP: {customer.PostalCode}";
                        
                        text.Span($"Endereço: {fullAddress}").FontSize(9);
                    }
                });
            });
        }

        private void ComposeItemsTable(IContainer container, List<InvoiceProductDto> items, string currencySymbol)
        {
            container.Table(table =>
            {
                // Definir colunas
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);  // #
                    columns.RelativeColumn(3);    // Descrição
                    columns.ConstantColumn(50);   // Qtd
                    columns.ConstantColumn(70);   // Preço
                    columns.ConstantColumn(50);   // Desc%
                    columns.ConstantColumn(80);   // Total
                });

                // Cabeçalho
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#").FontSize(9).Bold();
                    header.Cell().Element(CellStyle).Text("DESCRIÇÃO").FontSize(9).Bold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("QTD").FontSize(9).Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("PREÇO").FontSize(9).Bold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("DESC%").FontSize(9).Bold();
                    header.Cell().Element(CellStyle).AlignRight().Text("TOTAL").FontSize(9).Bold();

                    static IContainer CellStyle(IContainer container) =>
                        container.Background(Colors.Blue.Darken2)
                            .Padding(5)
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Darken1);
                });

                // Corpo
                int index = 1;
                foreach (var item in items)
                {
                    var lineTotal = item.UnitPrice * item.Quantity;
                    var discountAmount = lineTotal * (item.DiscountPercentage / 100m);
                    var finalTotal = lineTotal - discountAmount;

                    table.Cell().Element(RowCellStyle).Text($"{index}").FontSize(9);
                    table.Cell().Element(RowCellStyle).Column(col =>
                    {
                        col.Item().Text(item.ProductName).FontSize(9).Bold();
                        if (!string.IsNullOrWhiteSpace(item.ProductCode))
                            col.Item().Text($"Cód: {item.ProductCode}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                    table.Cell().Element(RowCellStyle).AlignCenter().Text($"{item.Quantity:N2}").FontSize(9);
                    table.Cell().Element(RowCellStyle).AlignRight().Text($"{currencySymbol} {item.UnitPrice:N2}").FontSize(9);
                    table.Cell().Element(RowCellStyle).AlignCenter().Text($"{item.DiscountPercentage:N1}%").FontSize(9);
                    table.Cell().Element(RowCellStyle).AlignRight().Text($"{currencySymbol} {finalTotal:N2}").FontSize(9).Bold();

                    index++;

                    static IContainer RowCellStyle(IContainer container) =>
                        container.BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten2)
                            .Padding(5);
                }
            });
        }

        private void ComposeTotals(IContainer container, InvoiceDto invoice, string currencySymbol)
        {
            container.AlignRight().Column(column =>
            {
                column.Spacing(3);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Subtotal:").FontSize(10);
                    row.ConstantItem(100).AlignRight().Text($"{currencySymbol} {invoice.SubTotal:N2}").FontSize(10);
                });

                if (invoice.DiscountAmount > 0)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Desconto:").FontSize(10).FontColor(Colors.Red.Medium);
                        row.ConstantItem(100).AlignRight().Text($"- {currencySymbol} {invoice.DiscountAmount:N2}")
                            .FontSize(10).FontColor(Colors.Red.Medium);
                    });
                }

                if (invoice.ShippingCost > 0)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Frete:").FontSize(10);
                        row.ConstantItem(100).AlignRight().Text($"{currencySymbol} {invoice.ShippingCost:N2}").FontSize(10);
                    });
                }

                if (invoice.TaxAmount > 0)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Imposto:").FontSize(10);
                        row.ConstantItem(100).AlignRight().Text($"{currencySymbol} {invoice.TaxAmount:N2}").FontSize(10);
                    });
                }

                column.Item().PaddingTop(5).BorderTop(2).BorderColor(Colors.Blue.Darken2).PaddingTop(8)
                    .Row(row =>
                    {
                        row.RelativeItem().Text("TOTAL:").FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
                        row.ConstantItem(100).AlignRight().Text($"{currencySymbol} {invoice.Total:N2}")
                            .FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
                    });

                if (invoice.PaidAmount > 0)
                {
                    column.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Text("Valor Pago:").FontSize(10).FontColor(Colors.Green.Medium);
                        row.ConstantItem(100).AlignRight().Text($"{currencySymbol} {invoice.PaidAmount:N2}")
                            .FontSize(10).Bold().FontColor(Colors.Green.Medium);
                    });

                    var balance = invoice.Total - invoice.PaidAmount;
                    if (balance > 0)
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Saldo Restante:").FontSize(10).FontColor(Colors.Orange.Medium);
                            row.ConstantItem(100).AlignRight().Text($"{currencySymbol} {balance:N2}")
                                .FontSize(10).Bold().FontColor(Colors.Orange.Medium);
                        });
                    }
                    else if (balance < 0)
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Troco:").FontSize(10).FontColor(Colors.Blue.Medium);
                            row.ConstantItem(100).AlignRight().Text($"{currencySymbol} {Math.Abs(balance):N2}")
                                .FontSize(10).Bold().FontColor(Colors.Blue.Medium);
                        });
                    }
                }
            });
        }

        private void ComposeNotes(IContainer container, string notes)
        {
            container.Background(Colors.Yellow.Lighten4).Padding(10).Column(column =>
            {
                column.Item().Text("OBSERVAÇÕES").FontSize(10).Bold();
                column.Item().PaddingTop(5).Text(notes).FontSize(9);
            });
        }

        private void ComposeFooter(IContainer container, CompanyConfigDto company)
        {
            container.AlignCenter().Column(column =>
            {
                if (!string.IsNullOrWhiteSpace(company.InvoiceFooterText))
                {
                    column.Item().Text(company.InvoiceFooterText)
                        .FontSize(8)
                        .Italic()
                        .FontColor(Colors.Grey.Darken1);
                }

                column.Item().PaddingTop(10).Text(text =>
                {
                    text.Span("Documento gerado em: ").FontSize(7).FontColor(Colors.Grey.Medium);
                    text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(7).FontColor(Colors.Grey.Medium);
                });
            });
        }

        private string GetStatusText(Data.Entities.InvoiceStatus status)
        {
            return status switch
            {
                Data.Entities.InvoiceStatus.Draft => "Rascunho",
                Data.Entities.InvoiceStatus.Pending => "Pendente",
                Data.Entities.InvoiceStatus.Confirmed => "Confirmada",
                Data.Entities.InvoiceStatus.Paid => "Paga",
                Data.Entities.InvoiceStatus.Cancelled => "Cancelada",
                _ => "Desconhecido"
            };
        }

        private string GetStatusColor(Data.Entities.InvoiceStatus status)
        {
            return status switch
            {
                Data.Entities.InvoiceStatus.Draft => Colors.Grey.Darken1,
                Data.Entities.InvoiceStatus.Pending => Colors.Orange.Medium,
                Data.Entities.InvoiceStatus.Confirmed => Colors.Blue.Medium,
                Data.Entities.InvoiceStatus.Paid => Colors.Green.Medium,
                Data.Entities.InvoiceStatus.Cancelled => Colors.Red.Medium,
                _ => Colors.Grey.Medium
            };
        }
    }
}
