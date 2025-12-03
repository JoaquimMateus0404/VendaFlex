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
using VendaFlex.ViewModels.Reports;

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
                            page.DefaultTextStyle(x => x.FontSize(8));

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
            container.Column(column =>
            {
                // Barra superior colorida
                column.Item()
                    .Height(8)
                    .Background(Colors.Blue.Darken2);

                column.Item().PaddingTop(15).Row(row =>
                {
                    // Coluna da esquerda - Logo e informações da empresa
                    row.RelativeItem(3).Column(col =>
                    {
                        // Logo (se existir)
                        if (!string.IsNullOrWhiteSpace(company.LogoUrl) && File.Exists(company.LogoUrl))
                        {
                            col.Item().MaxHeight(70).MaxWidth(200).Image(company.LogoUrl);
                            col.Item().PaddingVertical(8);
                        }

                        // Nome da empresa com destaque
                        col.Item().Text(company.CompanyName)
                            .FontSize(9)
                            .Bold()
                            .FontColor(Colors.Blue.Darken3);

                        // Linha decorativa
                        col.Item().PaddingTop(3).PaddingBottom(5)
                            .Height(2)
                            .Width(60)
                            .Background(Colors.Orange.Medium);

                        // Informações da empresa em grid
                        col.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span("NIF: ").FontSize(9).SemiBold();
                            text.Span(company.TaxId ?? "N/A").FontSize(9).FontColor(Colors.Grey.Darken2);
                            text.EmptyLine();
                            
                            if (!string.IsNullOrWhiteSpace(company.Address))
                            {
                                text.Span("Endereço: ").FontSize(8).SemiBold();
                                text.Span(company.Address).FontSize(8).FontColor(Colors.Grey.Darken2);
                                text.EmptyLine();
                            }
                            
                            if (!string.IsNullOrWhiteSpace(company.City))
                            {
                                var cityLine = company.City;
                                if (!string.IsNullOrWhiteSpace(company.PostalCode))
                                    cityLine += $", {company.PostalCode}";
                                text.Span(cityLine).FontSize(8).FontColor(Colors.Grey.Darken2);
                                text.EmptyLine();
                            }
                            
                            text.Span("Tel: ").FontSize(8).SemiBold();
                            text.Span(company.PhoneNumber ?? "N/A").FontSize(8).FontColor(Colors.Grey.Darken2);
                            text.Span("  |  ").FontSize(8).FontColor(Colors.Grey.Medium);
                            text.Span("Email: ").FontSize(8).SemiBold();
                            text.Span(company.Email ?? "N/A").FontSize(8).FontColor(Colors.Blue.Medium);
                        });
                    });

                    // Coluna da direita - Card de informações da fatura
                    row.RelativeItem(2).Column(col =>
                    {
                        // Card com sombra simulada
                        col.Item().AlignRight().Column(cardCol =>
                        {
                            // Título do documento
                            cardCol.Item()
                                .Background(Colors.Blue.Darken2)
                                .Padding(6)
                                .AlignCenter()
                                .Text("FATURA")
                                .FontSize(22)
                                .Bold()
                                .FontColor(Colors.White);

                            // Conteúdo do card
                            cardCol.Item()
                                .Border(1)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Background(Colors.Grey.Lighten4)
                                .Padding(15)
                                .Column(infoCol =>
                                {
                                    // Número da fatura
                                    infoCol.Item().Row(r =>
                                    {
                                        r.RelativeItem().Text("Nº Fatura:").FontSize(9).FontColor(Colors.Grey.Darken1);
                                        r.RelativeItem().AlignRight().Text(invoice.InvoiceNumber)
                                            .FontSize(9)
                                            .Bold()
                                            .FontColor(Colors.Blue.Darken3);
                                    });

                                    infoCol.Item().PaddingVertical(2)
                                        .Height(1)
                                        .Background(Colors.Grey.Lighten1);

                                    // Data de emissão
                                    infoCol.Item().PaddingTop(5).Row(r =>
                                    {
                                        r.RelativeItem().Text("Data Emissão:").FontSize(9).FontColor(Colors.Grey.Darken1);
                                        r.RelativeItem().AlignRight().Text(invoice.Date.ToString("dd/MM/yyyy"))
                                            .FontSize(10)
                                            .SemiBold()
                                            .FontColor(Colors.Grey.Darken3);
                                    });

                                    // Data de vencimento
                                    if (invoice.DueDate.HasValue)
                                    {
                                        infoCol.Item().PaddingTop(3).Row(r =>
                                        {
                                            r.RelativeItem().Text("Vencimento:").FontSize(9).FontColor(Colors.Grey.Darken1);
                                            r.RelativeItem().AlignRight().Text(invoice.DueDate.Value.ToString("dd/MM/yyyy"))
                                                .FontSize(10)
                                                .SemiBold()
                                                .FontColor(Colors.Orange.Darken1);
                                        });
                                    }

                                    infoCol.Item().PaddingVertical(5)
                                        .Height(1)
                                        .Background(Colors.Grey.Lighten1);

                                    // Status com badge
                                    infoCol.Item().PaddingTop(5).Row(r =>
                                    {
                                        r.RelativeItem().Text("Status:").FontSize(9).FontColor(Colors.Grey.Darken1);
                                        r.RelativeItem().AlignRight().Background(GetStatusColor(invoice.Status))
                                            .PaddingVertical(2)
                                            .PaddingHorizontal(4)
                                            .Text(GetStatusText(invoice.Status))
                                            .FontSize(9)
                                            .Bold()
                                            .FontColor(Colors.White);
                                    });
                                });
                        });
                    });
                });

                // Linha separadora decorativa
                column.Item().PaddingTop(15).Row(r =>
                {
                    r.RelativeItem().Height(3).Background(Colors.Blue.Darken2);
                    r.ConstantItem(20).Height(3).Background(Colors.Orange.Medium);
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
            container.Decoration(decoration =>
            {
                decoration.Before()
                    .Border(2)
                    .BorderColor(Colors.Blue.Lighten2);

                decoration.Content()
                    .Column(column =>
                    {
                        // Cabeçalho
                        column.Item()
                            .Background(Colors.Blue.Lighten3)
                            .PaddingVertical(8)
                            .PaddingHorizontal(10)
                            .Row(row =>
                            {
                                row.RelativeItem().Text("📋 DADOS DO CLIENTE")
                                    .FontSize(9)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken3);
                            });

                        // Conteúdo
                        column.Item()
                            .Background(Colors.Blue.Lighten5)
                            .PaddingVertical(12)
                            .PaddingHorizontal(15)
                            .Column(innerCol =>
                            {
                                // Nome
                                innerCol.Item().Text(customer.Name)
                                    .FontSize(9)
                                    .Bold()
                                    .FontColor(Colors.Grey.Darken3);

                                innerCol.Item().PaddingTop(8).Row(row =>
                                {
                                    // Coluna esquerda
                                    row.RelativeItem().Column(col =>
                                    {
                                        if (!string.IsNullOrWhiteSpace(customer.TaxId))
                                        {
                                            col.Item().Text(text =>
                                            {
                                                text.Span("NIF: ").FontSize(9).SemiBold().FontColor(Colors.Grey.Darken1);
                                                text.Span(customer.TaxId).FontSize(9).FontColor(Colors.Grey.Darken2);
                                            });
                                        }

                                        if (!string.IsNullOrWhiteSpace(customer.PhoneNumber))
                                        {
                                            col.Item().PaddingTop(3).Text(text =>
                                            {
                                                text.Span("Tel: ").FontSize(9).SemiBold().FontColor(Colors.Grey.Darken1);
                                                text.Span(customer.PhoneNumber).FontSize(9).FontColor(Colors.Grey.Darken2);
                                            });
                                        }

                                        if (!string.IsNullOrWhiteSpace(customer.Email))
                                        {
                                            col.Item().PaddingTop(3).Text(text =>
                                            {
                                                text.Span("Email: ").FontSize(9).SemiBold().FontColor(Colors.Grey.Darken1);
                                                text.Span(customer.Email).FontSize(9).FontColor(Colors.Blue.Medium);
                                            });
                                        }
                                    });

                                    // Coluna direita
                                    row.RelativeItem().Column(col =>
                                    {
                                        if (!string.IsNullOrWhiteSpace(customer.Address))
                                        {
                                            var fullAddress = customer.Address;
                                            if (!string.IsNullOrWhiteSpace(customer.City))
                                                fullAddress += $", {customer.City}";
                                            if (!string.IsNullOrWhiteSpace(customer.State))
                                                fullAddress += $" - {customer.State}";
                                            if (!string.IsNullOrWhiteSpace(customer.PostalCode))
                                                fullAddress += $", {customer.PostalCode}";

                                            col.Item().Text(text =>
                                            {
                                                text.Span("Endereço: ").FontSize(9).SemiBold().FontColor(Colors.Grey.Darken1);
                                                text.EmptyLine();
                                                text.Span(fullAddress).FontSize(9).FontColor(Colors.Grey.Darken2);
                                            });
                                        }
                                    });
                                });
                            });
                    });
            });
        }

        private void ComposeItemsTable(IContainer container, List<InvoiceProductDto> items, string currencySymbol)
        {
            container.Decoration(decoration =>
            {
                // Borda ao redor da tabela
                decoration.Before()
                    .Border(2)
                    .BorderColor(Colors.Blue.Darken2);

                decoration.Content()
                    .Table(table =>
                    {
                        // Definir colunas com espaçamento melhor
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);  // #
                            columns.RelativeColumn(4);    // Descrição
                            columns.ConstantColumn(60);   // Qtd
                            columns.ConstantColumn(80);   // Preço
                            columns.ConstantColumn(55);   // Desc%
                            columns.ConstantColumn(90);   // Total
                        });

                        // Cabeçalho com gradiente simulado
                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).AlignCenter().Text("#")
                                .FontSize(10).Bold().FontColor(Colors.White);
                            header.Cell().Element(HeaderCellStyle).Text("DESCRIÇÃO DO PRODUTO")
                                .FontSize(10).Bold().FontColor(Colors.White);
                            header.Cell().Element(HeaderCellStyle).AlignCenter().Text("QTD")
                                .FontSize(10).Bold().FontColor(Colors.White);
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("PREÇO UNIT.")
                                .FontSize(10).Bold().FontColor(Colors.White);
                            header.Cell().Element(HeaderCellStyle).AlignCenter().Text("DESC %")
                                .FontSize(10).Bold().FontColor(Colors.White);
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("TOTAL")
                                .FontSize(10).Bold().FontColor(Colors.White);

                            static IContainer HeaderCellStyle(IContainer container) =>
                                container.Background(Colors.Blue.Darken2)
                                    .PaddingVertical(8)
                                    .PaddingHorizontal(10)
                                    .BorderBottom(3)
                                    .BorderColor(Colors.Orange.Medium);
                        });

                        // Corpo com linhas alternadas
                        int index = 1;
                        bool isAlternate = false;
                        foreach (var item in items)
                        {
                            var lineTotal = item.UnitPrice * item.Quantity;
                            var discountAmount = lineTotal * (item.DiscountPercentage / 100m);
                            var finalTotal = lineTotal - discountAmount;

                            var backgroundColor = isAlternate ? Colors.Grey.Lighten4 : Colors.White;

                            // Número
                            table.Cell().Element(c => RowCellStyle(c, backgroundColor))
                                .AlignCenter()
                                .Background(Colors.Blue.Lighten4)
                                .Text($"{index}")
                                .FontSize(9)
                                .SemiBold()
                                .FontColor(Colors.Blue.Darken3);

                            // Descrição
                            table.Cell().Element(c => RowCellStyle(c, backgroundColor))
                                .Column(col =>
                                {
                                    col.Item().Text(item.ProductName)
                                        .FontSize(10)
                                        .SemiBold()
                                        .FontColor(Colors.Grey.Darken3);
                                    if (!string.IsNullOrWhiteSpace(item.ProductCode))
                                        col.Item().PaddingTop(2).Text($"Código: {item.ProductCode}")
                                            .FontSize(8)
                                            .Italic()
                                            .FontColor(Colors.Grey.Medium);
                                });

                            // Quantidade
                            table.Cell().Element(c => RowCellStyle(c, backgroundColor))
                                .AlignCenter()
                                .Text($"{item.Quantity:N0}")
                                .FontSize(10)
                                .SemiBold();

                            // Preço Unitário
                            table.Cell().Element(c => RowCellStyle(c, backgroundColor))
                                .AlignRight()
                                .Text($"{currencySymbol} {item.UnitPrice:N2}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken2);

                            // Desconto
                            table.Cell().Element(c => RowCellStyle(c, backgroundColor))
                                .AlignCenter()
                                .Text(item.DiscountPercentage > 0 ? $"{item.DiscountPercentage:N1}%" : "-")
                                .FontSize(9)
                                .FontColor(item.DiscountPercentage > 0 ? Colors.Red.Medium : Colors.Grey.Medium);

                            // Total
                            table.Cell().Element(c => RowCellStyle(c, backgroundColor))
                                .AlignRight()
                                .Background(Colors.Blue.Lighten5)
                                .Text($"{currencySymbol} {finalTotal:N2}")
                                .FontSize(9)
                                .Bold()
                                .FontColor(Colors.Blue.Darken3);

                            index++;
                            isAlternate = !isAlternate;
                        }

                        static IContainer RowCellStyle(IContainer container, string backgroundColor) =>
                            container.Background(backgroundColor)
                                .BorderBottom(1)
                                .BorderColor(Colors.Grey.Lighten1)
                                .PaddingVertical(6)
                                .PaddingHorizontal(8);
                    });
            });
        }

        private void ComposeTotals(IContainer container, InvoiceDto invoice, string currencySymbol)
        {
            container.AlignRight().Width(280).Column(column =>
            {
                // Card de totais com borda
                column.Item()
                    .Border(2)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Background(Colors.Grey.Lighten5)
                    .Padding(15)
                    .Column(innerColumn =>
                    {
                        innerColumn.Spacing(5);

                        // Subtotal
                        innerColumn.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Subtotal:")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken2);
                            row.ConstantItem(110).AlignRight().Text($"{currencySymbol} {invoice.SubTotal:N2}")
                                .FontSize(9)
                                .SemiBold();
                        });

                        // Desconto (se houver)
                        if (invoice.DiscountAmount > 0)
                        {
                            innerColumn.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Desconto:")
                                    .FontSize(9)
                                    .FontColor(Colors.Red.Darken1);
                                row.ConstantItem(110).AlignRight().Text($"- {currencySymbol} {invoice.DiscountAmount:N2}")
                                    .FontSize(9)
                                    .SemiBold()
                                    .FontColor(Colors.Red.Darken1);
                            });
                        }

                        // Frete (se houver)
                        if (invoice.ShippingCost > 0)
                        {
                            innerColumn.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Frete:")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken2);
                                row.ConstantItem(110).AlignRight().Text($"{currencySymbol} {invoice.ShippingCost:N2}")
                                    .FontSize(9)
                                    .SemiBold();
                            });
                        }

                        // Imposto (se houver)
                        if (invoice.TaxAmount > 0)
                        {
                            innerColumn.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Imposto:")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken2);
                                row.ConstantItem(110).AlignRight().Text($"{currencySymbol} {invoice.TaxAmount:N2}")
                                    .FontSize(9)
                                    .SemiBold();
                            });
                        }

                        // Linha separadora
                        innerColumn.Item().PaddingVertical(5)
                            .Height(2)
                            .Background(Colors.Blue.Darken2);

                        // Total com destaque
                        innerColumn.Item()
                            .Background(Colors.Blue.Darken2)
                            .PaddingVertical(8)
                            .PaddingHorizontal(10)
                            .Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL A PAGAR:")
                                    .FontSize(13)
                                    .Bold()
                                    .FontColor(Colors.White);
                                row.ConstantItem(120).AlignRight().Text($"{currencySymbol} {invoice.Total:N2}")
                                    .FontSize(9)
                                    .Bold()
                                    .FontColor(Colors.Orange.Lighten2);
                            });

                        // Informações de pagamento
                        if (invoice.PaidAmount > 0)
                        {
                            innerColumn.Item().PaddingTop(10).Row(row =>
                            {
                                row.RelativeItem().Text("Valor Pago:")
                                    .FontSize(10)
                                    .SemiBold()
                                    .FontColor(Colors.Green.Darken1);
                                row.ConstantItem(110).AlignRight().Text($"{currencySymbol} {invoice.PaidAmount:N2}")
                                    .FontSize(10)
                                    .Bold()
                                    .FontColor(Colors.Green.Darken1);
                            });

                            var balance = invoice.Total - invoice.PaidAmount;
                            if (balance > 0)
                            {
                                innerColumn.Item().PaddingTop(3)
                                    .Background(Colors.Orange.Lighten4)
                                    .PaddingVertical(5)
                                    .PaddingHorizontal(8)
                                    .Row(row =>
                                    {
                                        row.RelativeItem().Text("Saldo Restante:")
                                            .FontSize(9)
                                            .Bold()
                                            .FontColor(Colors.Orange.Darken2);
                                        row.ConstantItem(110).AlignRight().Text($"{currencySymbol} {balance:N2}")
                                            .FontSize(9)
                                            .Bold()
                                            .FontColor(Colors.Orange.Darken3);
                                    });
                            }
                            else if (balance < 0)
                            {
                                innerColumn.Item().PaddingTop(3)
                                    .Background(Colors.Green.Lighten4)
                                    .PaddingVertical(5)
                                    .PaddingHorizontal(8)
                                    .Row(row =>
                                    {
                                        row.RelativeItem().Text("Troco:")
                                            .FontSize(9)
                                            .Bold()
                                            .FontColor(Colors.Green.Darken2);
                                        row.ConstantItem(110).AlignRight().Text($"{currencySymbol} {Math.Abs(balance):N2}")
                                            .FontSize(9)
                                            .Bold()
                                            .FontColor(Colors.Green.Darken3);
                                    });
                            }
                        }
                    });
            });
        }

        private void ComposeNotes(IContainer container, string notes)
        {
            container.Background(Colors.Yellow.Lighten4).Padding(5).Column(column =>
            {
                column.Item().Text("OBSERVAÇÕES").FontSize(10).Bold();
                column.Item().PaddingTop(5).Text(notes).FontSize(9);
            });
        }

        private void ComposeFooter(IContainer container, CompanyConfigDto company)
        {
            container.Column(column =>
            {
                // Linha decorativa superior
                column.Item().Row(row =>
                {
                    row.RelativeItem().Height(2).Background(Colors.Blue.Darken2);
                    row.ConstantItem(20).Height(2).Background(Colors.Orange.Medium);
                });

                column.Item().PaddingTop(10).AlignCenter().Column(innerCol =>
                {
                    // Texto do rodapé personalizado
                    if (!string.IsNullOrWhiteSpace(company.InvoiceFooterText))
                    {
                        innerCol.Item()
                            .Background(Colors.Grey.Lighten4)
                            .PaddingVertical(5)
                            .PaddingHorizontal(8)
                            .Text(company.InvoiceFooterText)
                            .FontSize(9)
                            .Italic()
                            .FontColor(Colors.Grey.Darken2)
                            .AlignCenter();

                        innerCol.Item().PaddingTop(8);
                    }

                    // Informações de geração do documento
                    innerCol.Item().Row(row =>
                    {
                        row.RelativeItem().AlignLeft().Text(text =>
                        {
                            text.Span("💼 ").FontSize(8);
                            text.Span(company.CompanyName ?? "VendaFlex")
                                .FontSize(8)
                                .SemiBold()
                                .FontColor(Colors.Blue.Darken2);
                        });

                        row.RelativeItem().AlignCenter().Text(text =>
                        {
                            text.Span("📄 Documento gerado em: ").FontSize(7).FontColor(Colors.Grey.Darken1);
                            text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                                .FontSize(7)
                                .SemiBold()
                                .FontColor(Colors.Grey.Darken2);
                        });

                        row.RelativeItem().AlignRight().Text(text =>
                        {
                            text.Span("Página ").FontSize(7).FontColor(Colors.Grey.Darken1);
                            text.CurrentPageNumber().FontSize(7).SemiBold().FontColor(Colors.Grey.Darken2);
                            text.Span(" de ").FontSize(7).FontColor(Colors.Grey.Darken1);
                            text.TotalPages().FontSize(7).SemiBold().FontColor(Colors.Grey.Darken2);
                        });
                    });

                    // Barra inferior colorida fina
                    innerCol.Item().PaddingTop(5).Height(3).Background(Colors.Blue.Lighten2);
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

        // New Report Methods Below

        #region Sales By Period Report (Vendas por Período)

        public Task GenerateSalesByPeriodReportAsync(CompanyConfigDto companyConfig, IEnumerable<SalesByPeriodDto> salesData,
            DateTime startDate, DateTime endDate, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                        page.Header().Element(c => ComposeModernReportHeader(c, companyConfig, "Vendas por Período",
                            "Análise temporal de vendas e performance", startDate, endDate));
                        page.Content().Element(c => ComposeModernSalesByPeriodContent(c, salesData));
                        page.Footer().Element(c => ComposeProfessionalFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        private void ComposeModernSalesByPeriodContent(IContainer container, IEnumerable<SalesByPeriodDto> salesData)
        {
            var data = salesData.ToList();
            var totalValue = data.Sum(s => s.TotalValue);
            var totalQuantity = data.Sum(s => s.InvoiceCount);
            var avgTicket = totalQuantity > 0 ? totalValue / totalQuantity : 0;
            var avgDaily = data.Any() ? totalValue / data.Count : 0;

            container.Column(column =>
            {
                // KPIs Principais
                column.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "💰", "Total de Vendas",
                        $"{totalValue:N2} Kz", Colors.Green.Darken1, Colors.Green.Lighten4));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "📋", "Total de Faturas",
                        totalQuantity.ToString(), Colors.Blue.Darken1, Colors.Blue.Lighten4));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "🎯", "Ticket Médio",
                        $"{avgTicket:N2} Kz", Colors.Purple.Darken1, Colors.Purple.Lighten4));
                });

                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "📊", "Média Diária",
                        $"{avgDaily:N2} Kz", Colors.Orange.Darken1, Colors.Orange.Lighten4));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "📈", "Dias Analisados",
                        data.Count.ToString(), Colors.Teal.Darken1, Colors.Teal.Lighten4));
                    row.ConstantItem(15);
                    row.RelativeItem(); // Espaço vazio
                });

                // Título da seção
                column.Item().PaddingTop(30).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(4).Height(24).Background(Colors.Blue.Medium);
                        row.AutoItem().PaddingLeft(10).Text("ANÁLISE TEMPORAL DETALHADA")
                            .FontSize(13).Bold().FontColor(Colors.Grey.Darken3);
                    });
                    col.Item().PaddingTop(2).Height(2).Background(Colors.Blue.Lighten3);
                });

                // Tabela moderna
                column.Item().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken1).Padding(6).Text("Data")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken1).Padding(6).AlignRight().Text("Faturas")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken1).Padding(6).AlignRight().Text("Valor Total (Kz)")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken1).Padding(6).AlignRight().Text("Ticket Médio (Kz)")
                            .FontSize(10).Bold().FontColor(Colors.White);
                    });

                    int rowIndex = 0;
                    foreach (var sale in data)
                    {
                        var avgTicketDay = sale.InvoiceCount > 0 ? sale.TotalValue / sale.InvoiceCount : 0;
                        var bgColor = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                        var isHighPerformance = sale.TotalValue > avgDaily * 1.2m;

                        if (isHighPerformance)
                            bgColor = Colors.Green.Lighten5;

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).Row(row =>
                            {
                                row.AutoItem().Text("📅 ").FontSize(9);
                                row.AutoItem().Text(sale.DateFormatted).FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);
                            });

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text(sale.InvoiceCount.ToString())
                            .FontSize(9).Bold().FontColor(Colors.Blue.Medium);

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text($"{sale.TotalValue:N2}")
                            .FontSize(9).Bold().FontColor(Colors.Green.Darken1);

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Row(row =>
                            {
                                row.AutoItem().Text($"{avgTicketDay:N2}").FontSize(9).FontColor(Colors.Grey.Darken2);
                                if (isHighPerformance)
                                {
                                    row.AutoItem().PaddingLeft(5).Text("🔥").FontSize(10);
                                }
                            });

                        rowIndex++;
                    }
                });

                // Análise de Tendências
                column.Item().PaddingTop(30).Background(Colors.Blue.Lighten5).Border(1)
                    .BorderColor(Colors.Blue.Lighten2).Padding(20).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.AutoItem().Text("📈").FontSize(24);
                            row.AutoItem().PaddingLeft(10).Text("ANÁLISE DE TENDÊNCIAS")
                        .FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                        });

                        col.Item().PaddingTop(12).PaddingLeft(10).Column(insights =>
                        {
                            var bestDay = data.OrderByDescending(d => d.TotalValue).FirstOrDefault();
                            var worstDay = data.OrderBy(d => d.TotalValue).FirstOrDefault();

                            if (bestDay != null)
                            {
                                insights.Item().PaddingVertical(4).Row(row =>
                                {
                                    row.AutoItem().Width(6).Height(6).Background(Colors.Green.Medium).AlignMiddle();
                                    row.AutoItem().PaddingLeft(8).Text($"Melhor dia: {bestDay.DateFormatted} com {bestDay.TotalValue:N2} Kz")
                                .FontSize(10).FontColor(Colors.Grey.Darken2);
                                });
                            }

                            if (worstDay != null)
                            {
                                insights.Item().PaddingVertical(4).Row(row =>
                                {
                                    row.AutoItem().Width(6).Height(6).Background(Colors.Orange.Medium).AlignMiddle();
                                    row.AutoItem().PaddingLeft(8).Text($"Dia mais fraco: {worstDay.DateFormatted} com {worstDay.TotalValue:N2} Kz")
                                .FontSize(10).FontColor(Colors.Grey.Darken2);
                                });
                            }

                            insights.Item().PaddingVertical(4).Row(row =>
                            {
                                row.AutoItem().Width(6).Height(6).Background(Colors.Blue.Medium).AlignMiddle();
                                row.AutoItem().PaddingLeft(8).Text($"Média diária: {avgDaily:N2} Kz")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);
                            });
                        });
                    });
            });
        }

        #endregion

        #region Top Products Report (Top Produtos)

        public Task GenerateTopProductsReportAsync(CompanyConfigDto companyConfig, IEnumerable<TopProductDto> topProducts,
            DateTime startDate, DateTime endDate, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                        page.Header().Element(c => ComposeModernReportHeader(c, companyConfig, "Top Produtos Mais Vendidos",
                            "Ranking de performance de produtos", startDate, endDate));
                        page.Content().Element(c => ComposeModernTopProductsContent(c, topProducts));
                        page.Footer().Element(c => ComposeProfessionalFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        private void ComposeModernTopProductsContent(IContainer container, IEnumerable<TopProductDto> topProducts)
        {
            var products = topProducts.ToList();
            var totalRevenue = products.Sum(p => p.Revenue);
            var totalQuantity = products.Sum(p => p.QuantitySold);
            var avgRevenue = products.Any() ? totalRevenue / products.Count : 0;

            container.Column(column =>
            {
                // KPIs
                column.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "💎", "Receita Total",
                        $"{totalRevenue:N2} Kz", Colors.Green.Darken1, Colors.Green.Lighten4));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "📦", "Unidades Vendidas",
                        totalQuantity.ToString(), Colors.Blue.Darken1, Colors.Blue.Lighten4));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "📊", "Produtos Analisados",
                        products.Count.ToString(), Colors.Purple.Darken1, Colors.Purple.Lighten4));
                });

                // Pódio (Top 3)
                if (products.Any())
                {
                    column.Item().PaddingTop(30).Background(Colors.Yellow.Lighten4).Border(2)
                        .BorderColor(Colors.Yellow.Darken1).Padding(20).Column(podiumCol =>
                        {
                            podiumCol.Item().AlignCenter().Text("🏆 PÓDIO DOS CAMPEÕES 🏆")
                        .FontSize(9).Bold().FontColor(Colors.Orange.Darken2);

                            podiumCol.Item().PaddingTop(15).Row(row =>
                            {
                                var top3 = products.Take(3).ToList();

                                // 2º Lugar
                                if (top3.Count >= 2)
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().AlignCenter().Text("🥈").FontSize(40);
                                        col.Item().PaddingTop(5).AlignCenter().Text("2º LUGAR")
                                    .FontSize(10).Bold().FontColor(Colors.Grey.Darken2);
                                        col.Item().PaddingTop(5).AlignCenter().Text(top3[1].ProductName)
                                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                                        col.Item().PaddingTop(5).AlignCenter().Text($"{top3[1].Revenue:N2} Kz")
                                    .FontSize(9).Bold().FontColor(Colors.Green.Darken1);
                                    });
                                }

                                row.ConstantItem(10);

                                // 1º Lugar
                                if (top3.Count >= 1)
                                {
                                    row.RelativeItem().Background(Colors.Yellow.Lighten3).Border(2)
                                .BorderColor(Colors.Yellow.Darken2).Padding(15).Column(col =>
                                    {
                                        col.Item().AlignCenter().Text("🥇").FontSize(50);
                                        col.Item().PaddingTop(5).AlignCenter().Text("1º LUGAR")
                                    .FontSize(9).Bold().FontColor(Colors.Orange.Darken2);
                                        col.Item().PaddingTop(5).AlignCenter().Text(top3[0].ProductName)
                                    .FontSize(10).Bold().FontColor(Colors.Grey.Darken2);
                                        col.Item().PaddingTop(5).AlignCenter().Text($"{top3[0].Revenue:N2} Kz")
                                    .FontSize(13).Bold().FontColor(Colors.Green.Darken1);
                                    });
                                }

                                row.ConstantItem(10);

                                // 3º Lugar
                                if (top3.Count >= 3)
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().AlignCenter().Text("🥉").FontSize(40);
                                        col.Item().PaddingTop(5).AlignCenter().Text("3º LUGAR")
                                    .FontSize(10).Bold().FontColor(Colors.Grey.Darken2);
                                        col.Item().PaddingTop(5).AlignCenter().Text(top3[2].ProductName)
                                    .FontSize(9).FontColor(Colors.Grey.Darken1);
                                        col.Item().PaddingTop(5).AlignCenter().Text($"{top3[2].Revenue:N2} Kz")
                                    .FontSize(9).Bold().FontColor(Colors.Green.Darken1);
                                    });
                                }
                            });
                        });
                }

                // Título da tabela
                column.Item().PaddingTop(30).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(4).Height(24).Background(Colors.Purple.Medium);
                        row.AutoItem().PaddingLeft(10).Text("RANKING COMPLETO")
                            .FontSize(13).Bold().FontColor(Colors.Grey.Darken3);
                    });
                    col.Item().PaddingTop(2).Height(2).Background(Colors.Purple.Lighten3);
                });

                // Tabela completa
                column.Item().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Purple.Darken1).Padding(6).AlignCenter().Text("Rank")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Purple.Darken1).Padding(6).Text("Produto")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Purple.Darken1).Padding(6).AlignRight().Text("Qtd Vendida")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Purple.Darken1).Padding(6).AlignRight().Text("Receita (Kz)")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Purple.Darken1).Padding(6).AlignRight().Text("% Total")
                            .FontSize(10).Bold().FontColor(Colors.White);
                    });

                    int rank = 1;
                    foreach (var product in products)
                    {
                        var percentage = totalRevenue > 0 ? (product.Revenue / totalRevenue) * 100 : 0;
                        var bgColor = rank <= 3 ? Colors.Yellow.Lighten5 :
                                     rank % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        var medal = rank == 1 ? "🥇" : rank == 2 ? "🥈" : rank == 3 ? "🥉" : "";

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignCenter().Text($"{medal} {rank}")
                            .FontSize(9).Bold().FontColor(Colors.Grey.Darken2);

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).Text(product.ProductName).FontSize(9).FontColor(Colors.Grey.Darken2);

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text(product.QuantitySold.ToString())
                            .FontSize(9).Bold().FontColor(Colors.Blue.Medium);

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text($"{product.Revenue:N2}")
                            .FontSize(9).Bold().FontColor(Colors.Green.Darken1);

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Background(Colors.Purple.Medium)
                            .Padding(4).Text($"{percentage:N1}%").FontSize(8).Bold().FontColor(Colors.White);

                        rank++;
                    }
                });
            });
        }

        #endregion

        #region Sales By Customer Report (Vendas por Cliente)

        public Task GenerateSalesByCustomerReportAsync(CompanyConfigDto companyConfig, IEnumerable<SalesByCustomerDto> salesByCustomer,
            DateTime startDate, DateTime endDate, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                        page.Header().Element(c => ComposeModernReportHeader(c, companyConfig, "Vendas por Cliente",
                            "Análise de performance e comportamento de clientes", startDate, endDate));
                        page.Content().Element(c => ComposeModernSalesByCustomerContent(c, salesByCustomer));
                        page.Footer().Element(c => ComposeProfessionalFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        private void ComposeModernSalesByCustomerContent(IContainer container, IEnumerable<SalesByCustomerDto> salesByCustomer)
        {
            var customers = salesByCustomer.ToList();
            var totalValue = customers.Sum(c => c.TotalValue);
            var totalInvoices = customers.Sum(c => c.InvoiceCount);
            var avgTicket = totalInvoices > 0 ? totalValue / totalInvoices : 0;
            var avgPerCustomer = customers.Any() ? totalValue / customers.Count : 0;

            container.Column(column =>
            {
                // KPIs
                column.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "👥", "Total de Clientes",
                        customers.Count.ToString(), Colors.Purple.Darken1, Colors.Purple.Lighten4));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "💰", "Valor Total",
                        $"{totalValue:N2} Kz", Colors.Green.Darken1, Colors.Green.Lighten4));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "🎯", "Ticket Médio",
                        $"{avgTicket:N2} Kz", Colors.Blue.Darken1, Colors.Blue.Lighten4));
                });

                // Segmentação de clientes
                column.Item().PaddingTop(30).Background(Colors.Purple.Lighten5).Border(1)
                    .BorderColor(Colors.Purple.Lighten2).Padding(20).Column(segmentCol =>
                    {
                        segmentCol.Item().Row(row =>
                        {
                            row.AutoItem().Text("📊").FontSize(24);
                            row.AutoItem().PaddingLeft(10).Text("SEGMENTAÇÃO DE CLIENTES")
                        .FontSize(9).Bold().FontColor(Colors.Purple.Darken2);
                        });

                        segmentCol.Item().PaddingTop(15).Row(row =>
                        {
                            var vip = customers.Where(c => c.TotalValue > avgPerCustomer * 2).Count();
                            var premium = customers.Where(c => c.TotalValue > avgPerCustomer && c.TotalValue <= avgPerCustomer * 2).Count();
                            var regular = customers.Where(c => c.TotalValue <= avgPerCustomer).Count();

                            row.RelativeItem().Background(Colors.Yellow.Lighten4).Border(1)
                        .BorderColor(Colors.Yellow.Darken1).Padding(15).Column(col =>
                            {
                                col.Item().AlignCenter().Text("⭐ VIP").FontSize(9).Bold().FontColor(Colors.Orange.Darken2);
                                col.Item().PaddingTop(5).AlignCenter().Text(vip.ToString())
                            .FontSize(9).Bold().FontColor(Colors.Orange.Darken1);
                                col.Item().PaddingTop(3).AlignCenter().Text(">200% da média")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                            });

                            row.ConstantItem(10);

                            row.RelativeItem().Background(Colors.Blue.Lighten4).Border(1)
                        .BorderColor(Colors.Blue.Darken1).Padding(15).Column(col =>
                            {
                                col.Item().AlignCenter().Text("💎 Premium").FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                                col.Item().PaddingTop(5).AlignCenter().Text(premium.ToString())
                            .FontSize(9).Bold().FontColor(Colors.Blue.Darken1);
                                col.Item().PaddingTop(3).AlignCenter().Text("Acima da média")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                            });

                            row.ConstantItem(10);

                            row.RelativeItem().Background(Colors.Grey.Lighten4).Border(1)
                        .BorderColor(Colors.Grey.Darken1).Padding(15).Column(col =>
                            {
                                col.Item().AlignCenter().Text("👤 Regular").FontSize(9).Bold().FontColor(Colors.Grey.Darken2);
                                col.Item().PaddingTop(5).AlignCenter().Text(regular.ToString())
                            .FontSize(9).Bold().FontColor(Colors.Grey.Darken1);
                                col.Item().PaddingTop(3).AlignCenter().Text("Até a média")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                            });
                        });
                    });

                // Título da tabela
                column.Item().PaddingTop(30).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(4).Height(24).Background(Colors.Purple.Medium);
                        row.AutoItem().PaddingLeft(10).Text("DETALHAMENTO POR CLIENTE")
                            .FontSize(13).Bold().FontColor(Colors.Grey.Darken3);
                    });
                    col.Item().PaddingTop(2).Height(2).Background(Colors.Purple.Lighten3);
                });

                // Tabela
                column.Item().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Purple.Darken1).Padding(6).Text("Cliente")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Purple.Darken1).Padding(6).AlignRight().Text("Compras")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Purple.Darken1).Padding(6).AlignRight().Text("Total (Kz)")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Purple.Darken1).Padding(6).AlignRight().Text("Ticket Médio")
                            .FontSize(10).Bold().FontColor(Colors.White);
                    });

                    int rowIndex = 0;
                    foreach (var customer in customers.OrderByDescending(c => c.TotalValue))
                    {
                        var avgTicketCust = customer.InvoiceCount > 0 ? customer.TotalValue / customer.InvoiceCount : 0;
                        var bgColor = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        var segment = customer.TotalValue > avgPerCustomer * 2 ? "⭐" :
                                     customer.TotalValue > avgPerCustomer ? "💎" : "👤";

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).Row(row =>
                            {
                                row.AutoItem().Text($"{segment} ").FontSize(9);
                                row.AutoItem().Text(customer.CustomerName).FontSize(9).FontColor(Colors.Grey.Darken2);
                            });

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text(customer.InvoiceCount.ToString())
                            .FontSize(9).Bold().FontColor(Colors.Blue.Medium);

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text($"{customer.TotalValue:N2}")
                            .FontSize(9).Bold().FontColor(Colors.Green.Darken1);

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text($"{avgTicketCust:N2}")
                            .FontSize(9).FontColor(Colors.Grey.Darken2);

                        rowIndex++;
                    }
                });
            });
        }

        #endregion
        public Task GenerateProfitMarginReportAsync(CompanyConfigDto companyConfig, IEnumerable<ProfitMarginDto> profitMargins, DateTime startDate,
            DateTime endDate, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8));

                        page.Header().Element(c => ComposeReportHeader(c, companyConfig, "Análise de Margem de Lucro", startDate, endDate));
                        page.Content().Element(c => ComposeProfitMarginContent(c, profitMargins));
                        page.Footer().Element(c => ComposeSimpleFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        public Task GenerateInvoicesByStatusReportAsync(CompanyConfigDto companyConfig, IEnumerable<InvoicesByStatusDto> invoicesByStatus,
            DateTime startDate, DateTime endDate, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8));

                        page.Header().Element(c => ComposeReportHeader(c, companyConfig, "Faturas por Status", startDate, endDate));
                        page.Content().Element(c => ComposeInvoicesByStatusContent(c, invoicesByStatus));
                        page.Footer().Element(c => ComposeSimpleFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        #region Sales Report Components

        private void ComposeReportHeader(IContainer container, CompanyConfigDto company, string title, DateTime startDate, DateTime endDate)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    // Logo (se existir)
                    if (!string.IsNullOrWhiteSpace(company.LogoUrl) && File.Exists(company.LogoUrl))
                    {
                        row.ConstantItem(80).Column(logoCol =>
                        {
                            logoCol.Item().MaxHeight(60).Image(company.LogoUrl);
                        });
                        row.ConstantItem(10); // Espaçamento
                    }
                    
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(company.CompanyName ?? "VendaFlex")
                            .FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text(title)
                            .FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);
                        col.Item().Text($"Período: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}")
                            .FontSize(10).FontColor(Colors.Grey.Darken1);
                    });
                    
                    row.ConstantItem(100).AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });

                column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Medium);
            });
        }

        private void ComposeSalesByPeriodContent(IContainer container, IEnumerable<SalesByPeriodDto> salesData, DateTime startDate, DateTime endDate)
        {
            var data = salesData.ToList();
            var totalValue = data.Sum(s => s.TotalValue);
            var totalQuantity = data.Sum(s => s.InvoiceCount);

            container.Column(column =>
            {
                // Resumo
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Total de Vendas", $"{totalValue:N2} Kz", Colors.Blue.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Total de Faturas", totalQuantity.ToString(), Colors.Green.Medium));
                });

                // Tabela
                column.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Data").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Faturas").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Valor Total (Kz)").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Ticket Médio (Kz)").FontSize(10).SemiBold();
                    });

                    foreach (var sale in data)
                    {
                        var avgTicket = sale.InvoiceCount > 0 ? sale.TotalValue / sale.InvoiceCount : 0;
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(sale.DateFormatted).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(sale.InvoiceCount.ToString()).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{sale.TotalValue:N2}").FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{avgTicket:N2}").FontSize(9);
                    }
                });
            });
        }

        private void ComposeTopProductsContent(IContainer container, IEnumerable<TopProductDto> topProducts)
        {
            var products = topProducts.ToList();
            var totalRevenue = products.Sum(p => p.Revenue);
            var totalQuantity = products.Sum(p => p.QuantitySold);

            container.Column(column =>
            {
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Receita Total", $"{totalRevenue:N2} Kz", Colors.Green.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Quantidade Total", totalQuantity.ToString(), Colors.Blue.Medium));
                });

                column.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("#").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Produto").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Qtd Vendida").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Receita (Kz)").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("%").FontSize(10).SemiBold();
                    });

                    int rank = 1;
                    foreach (var product in products)
                    {
                        var percentage = totalRevenue > 0 ? (product.Revenue / totalRevenue) * 100 : 0;
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignCenter().Text(rank.ToString()).FontSize(9).Bold();
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(product.ProductName).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(product.QuantitySold.ToString()).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{product.Revenue:N2}").FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{percentage:N1}%").FontSize(9);
                        
                        rank++;
                    }
                });
            });
        }

        private void ComposeSalesByCustomerContent(IContainer container, IEnumerable<SalesByCustomerDto> salesByCustomer)
        {
            var customers = salesByCustomer.ToList();
            var totalValue = customers.Sum(c => c.TotalValue);
            var totalInvoices = customers.Sum(c => c.InvoiceCount);

            container.Column(column =>
            {
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Total de Clientes", customers.Count.ToString(), Colors.Purple.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Valor Total", $"{totalValue:N2} Kz", Colors.Green.Medium));
                });

                column.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Cliente").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Compras").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Total (Kz)").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Ticket Médio").FontSize(10).SemiBold();
                    });

                    foreach (var customer in customers)
                    {
                        var avgTicket = customer.InvoiceCount > 0 ? customer.TotalValue / customer.InvoiceCount : 0;
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(customer.CustomerName).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(customer.InvoiceCount.ToString()).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{customer.TotalValue:N2}").FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{avgTicket:N2}").FontSize(9);
                    }
                });
            });
        }

        private void ComposeProfitMarginContent(IContainer container, IEnumerable<ProfitMarginDto> profitMargins)
        {
            var margins = profitMargins.ToList();
            var totalRevenue = margins.Sum(m => m.TotalRevenue);
            var totalProfit = margins.Sum(m => m.GrossProfit);
            var avgMargin = totalRevenue > 0 ? (totalProfit / totalRevenue) * 100 : 0;

            container.Column(column =>
            {
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Receita Total", $"{totalRevenue:N2} Kz", Colors.Blue.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Lucro Total", $"{totalProfit:N2} Kz", Colors.Green.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Margem Média", $"{avgMargin:N2}%", Colors.Orange.Medium));
                });

                column.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Produto").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Custo (Kz)").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Receita (Kz)").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Lucro (Kz)").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Margem %").FontSize(10).SemiBold();
                    });

                    foreach (var margin in margins)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(margin.InvoiceNumber).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{margin.TotalCost:N2}").FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{margin.TotalRevenue:N2}").FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{margin.GrossProfit:N2}").FontSize(9).FontColor(margin.GrossProfit >= 0 ? Colors.Green.Medium : Colors.Red.Medium);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{margin.MarginPercentage:N2}%").FontSize(9).Bold();
                    }
                });
            });
        }

        private void ComposeInvoicesByStatusContent(IContainer container, IEnumerable<InvoicesByStatusDto> invoicesByStatus)
        {
            var statuses = invoicesByStatus.ToList();
            var totalCount = statuses.Sum(s => s.Count);
            var totalValue = statuses.Sum(s => s.TotalValue);

            container.Column(column =>
            {
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Total de Faturas", totalCount.ToString(), Colors.Blue.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Valor Total", $"{totalValue:N2} Kz", Colors.Green.Medium));
                });

                column.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Status").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Quantidade").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Valor Total (Kz)").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Percentual").FontSize(10).SemiBold();
                    });

                    foreach (var status in statuses)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(status.Status).FontSize(9).Bold();
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(status.Count.ToString()).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{status.TotalValue:N2}").FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{status.Percentage:N1}%").FontSize(9);
                    }
                });
            });
        }

        #endregion

        // MELHORIAS IMPLEMENTADAS:
        // 1. Headers mais modernos e visuais com gradientes
        // 2. Cards KPI com ícones e melhor hierarquia visual
        // 3. Tabelas com zebra striping e hover effects simulados
        // 4. Badges de status com cores semânticas
        // 5. Seções com títulos destacados e ícones
        // 6. Gráficos visuais melhorados
        // 7. Alertas e notificações mais evidentes
        // 8. Melhor uso de espaçamento e respiração
        // 9. Tipografia hierárquica mais clara
        // 10. Rodapé profissional com informações relevantes

        #region Cash Flow Report (Fluxo de Caixa)

        public Task GenerateCashFlowReportAsync(CompanyConfigDto companyConfig, IEnumerable<CashFlowDto> cashFlowData,
            DateTime startDate, DateTime endDate, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                        page.Header().Element(c => ComposeModernReportHeader(c, companyConfig, "Fluxo de Caixa",
                            "Análise detalhada de entradas e saídas financeiras", startDate, endDate));
                        page.Content().Element(c => ComposeModernCashFlowContent(c, cashFlowData));
                        page.Footer().Element(c => ComposeProfessionalFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        private void ComposeModernCashFlowContent(IContainer container, IEnumerable<CashFlowDto> cashFlowData)
        {
            var data = cashFlowData.ToList();
            var totalInflow = data.Sum(c => c.Inflow);
            var totalOutflow = data.Sum(c => c.Outflow);
            var netFlow = totalInflow - totalOutflow;

            container.Column(column =>
            {
                // KPI Cards com design moderno
                column.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "💰", "Total de Entradas",
                        $"{totalInflow:N2} Kz", Colors.Green.Darken1, Colors.Green.Lighten4));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "💸", "Total de Saídas",
                        $"{totalOutflow:N2} Kz", Colors.Red.Darken1, Colors.Red.Lighten4));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "📊", "Fluxo Líquido",
                        $"{netFlow:N2} Kz", netFlow >= 0 ? Colors.Blue.Darken1 : Colors.Orange.Darken1,
                        netFlow >= 0 ? Colors.Blue.Lighten4 : Colors.Orange.Lighten4));
                });

                // Status Badge
                if (netFlow < 0)
                {
                    column.Item().PaddingTop(20).AlignCenter().Container()
                        .Background(Colors.Orange.Lighten4).Border(1).BorderColor(Colors.Orange.Medium)
                        .PaddingVertical(12).PaddingHorizontal(20).Text("⚠️ ATENÇÃO: Fluxo de caixa negativo no período")
                        .FontSize(9).Bold().FontColor(Colors.Orange.Darken2);
                }

                // Seção de Tabela
                column.Item().PaddingTop(30).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(4).Height(24).Background(Colors.Blue.Medium);
                        row.AutoItem().PaddingLeft(10).Text("MOVIMENTAÇÃO DETALHADA")
                            .FontSize(13).Bold().FontColor(Colors.Grey.Darken3);
                    });
                    col.Item().PaddingTop(2).Height(2).Background(Colors.Blue.Lighten3);
                });

                column.Item().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                    });

                    // Header moderno
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken1).Padding(6).Text("Data")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Green.Darken1).Padding(6).AlignRight().Text("Entradas (Kz)")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Red.Darken1).Padding(6).AlignRight().Text("Saídas (Kz)")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken1).Padding(6).AlignRight().Text("Saldo (Kz)")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Grey.Darken2).Padding(6).AlignRight().Text("Acumulado (Kz)")
                            .FontSize(10).Bold().FontColor(Colors.White);
                    });

                    // Rows com zebra striping
                    int rowIndex = 0;
                    foreach (var flow in data)
                    {
                        var balance = flow.Inflow - flow.Outflow;
                        var bgColor = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).Text(flow.DateFormatted).FontSize(9).FontColor(Colors.Grey.Darken2);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text($"{flow.Inflow:N2}").FontSize(9).Bold()
                            .FontColor(flow.Inflow > 0 ? Colors.Green.Darken1 : Colors.Grey.Medium);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text($"{flow.Outflow:N2}").FontSize(9).Bold()
                            .FontColor(flow.Outflow > 0 ? Colors.Red.Darken1 : Colors.Grey.Medium);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text($"{balance:N2}").FontSize(9).Bold()
                            .FontColor(balance >= 0 ? Colors.Blue.Medium : Colors.Orange.Medium);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text($"{flow.Balance:N2}").FontSize(9).Bold()
                            .FontColor(flow.Balance >= 0 ? Colors.Green.Darken1 : Colors.Red.Darken1);

                        rowIndex++;
                    }
                });

                // Insights Box
                column.Item().PaddingTop(30).Background(Colors.Blue.Lighten5).Border(1)
                    .BorderColor(Colors.Blue.Lighten2).Padding(20).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.AutoItem().Text("💡").FontSize(24);
                            row.AutoItem().PaddingLeft(10).Text("INSIGHTS E RECOMENDAÇÕES")
                        .FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                        });

                        col.Item().PaddingTop(12).PaddingLeft(10).Column(insights =>
                        {
                            var avgInflow = data.Any() ? data.Average(d => d.Inflow) : 0;
                            var avgOutflow = data.Any() ? data.Average(d => d.Outflow) : 0;

                            insights.Item().PaddingVertical(4).Row(row =>
                            {
                                row.AutoItem().Width(4).Height(4).Background(Colors.Blue.Medium).AlignMiddle();
                                row.AutoItem().PaddingLeft(8).Text($"Média diária de entradas: {avgInflow:N2} Kz")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);
                            });

                            insights.Item().PaddingVertical(4).Row(row =>
                            {
                                row.AutoItem().Width(4).Height(4).Background(Colors.Blue.Medium).AlignMiddle();
                                row.AutoItem().PaddingLeft(8).Text($"Média diária de saídas: {avgOutflow:N2} Kz")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);
                            });

                            if (netFlow < 0)
                            {
                                insights.Item().PaddingVertical(4).Row(row =>
                                {
                                    row.AutoItem().Width(4).Height(4).Background(Colors.Orange.Medium).AlignMiddle();
                                    row.AutoItem().PaddingLeft(8).Text("Recomendação: Revisar despesas e aumentar receitas")
                                .FontSize(10).FontColor(Colors.Orange.Darken2);
                                });
                            }
                        });
                    });
            });
        }

        #endregion

        #region Payment Methods Report (Formas de Pagamento)

        public Task GeneratePaymentMethodsReportAsync(CompanyConfigDto companyConfig, IEnumerable<PaymentMethodDto> paymentMethods,
            DateTime startDate, DateTime endDate, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                        page.Header().Element(c => ComposeModernReportHeader(c, companyConfig, "Formas de Pagamento",
                            "Distribuição e análise dos métodos de pagamento", startDate, endDate));
                        page.Content().Element(c => ComposeModernPaymentMethodsContent(c, paymentMethods));
                        page.Footer().Element(c => ComposeProfessionalFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        private void ComposeModernPaymentMethodsContent(IContainer container, IEnumerable<PaymentMethodDto> paymentMethods)
        {
            var methods = paymentMethods.ToList();
            var totalValue = methods.Sum(m => m.TotalValue);
            var totalTransactions = methods.Sum(m => m.TransactionCount);
            var avgTransaction = totalTransactions > 0 ? totalValue / totalTransactions : 0;

            container.Column(column =>
            {
                // KPIs
                column.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "💳", "Valor Total",
                        $"{totalValue:N2} Kz", Colors.Green.Darken1, Colors.Green.Lighten4));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "🔢", "Transações",
                        totalTransactions.ToString(), Colors.Blue.Darken1, Colors.Blue.Lighten4));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "📊", "Ticket Médio",
                        $"{avgTransaction:N2} Kz", Colors.Purple.Darken1, Colors.Purple.Lighten4));
                });

                // Título da seção
                column.Item().PaddingTop(30).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(4).Height(24).Background(Colors.Blue.Medium);
                        row.AutoItem().PaddingLeft(10).Text("DISTRIBUIÇÃO POR MÉTODO")
                            .FontSize(13).Bold().FontColor(Colors.Grey.Darken3);
                    });
                    col.Item().PaddingTop(2).Height(2).Background(Colors.Blue.Lighten3);
                });

                // Tabela moderna
                column.Item().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2.5f);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken1).Padding(6).Text("Forma de Pagamento")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken1).Padding(6).AlignRight().Text("Transações")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken1).Padding(6).AlignRight().Text("Valor Total (Kz)")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken1).Padding(6).AlignRight().Text("% do Total")
                            .FontSize(10).Bold().FontColor(Colors.White);
                    });

                    int rowIndex = 0;
                    foreach (var method in methods)
                    {
                        var bgColor = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).Row(row =>
                            {
                                row.AutoItem().Text("💳 ").FontSize(9);
                                row.AutoItem().Text(method.MethodName).FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);
                            });
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text(method.TransactionCount.ToString()).FontSize(9).FontColor(Colors.Grey.Darken2);
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).AlignRight().Text($"{method.TotalValue:N2}").FontSize(9).Bold().FontColor(Colors.Green.Darken1);
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                    .Padding(5).AlignRight().Row(badgeRow =>
                    {
                        badgeRow.AutoItem().Background(Colors.Blue.Medium).PaddingVertical(4).PaddingHorizontal(10)
                    .Text($"{method.Percentage:N1}%").FontSize(8).Bold().FontColor(Colors.White);
                    });                        rowIndex++;
                    }
                });

                // Visualização gráfica melhorada
                column.Item().PaddingTop(30).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(4).Height(24).Background(Colors.Purple.Medium);
                        row.AutoItem().PaddingLeft(10).Text("VISUALIZAÇÃO GRÁFICA")
                            .FontSize(13).Bold().FontColor(Colors.Grey.Darken3);
                    });
                    col.Item().PaddingTop(2).Height(2).Background(Colors.Purple.Lighten3);
                });

                column.Item().PaddingTop(15).Column(col =>
                {
                    var colors = new[] {
                Colors.Blue.Medium, Colors.Green.Medium, Colors.Orange.Medium,
                Colors.Purple.Medium, Colors.Teal.Medium, Colors.Pink.Medium
            };

                    int colorIndex = 0;
                    foreach (var method in methods.Take(6))
                    {
                        var color = colors[colorIndex % colors.Length];

                        col.Item().PaddingVertical(8).Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Background(Colors.White).Padding(15).Column(methodCol =>
                            {
                                methodCol.Item().Row(row =>
                                {
                                    row.RelativeItem().Column(textCol =>
                                    {
                                        textCol.Item().Text(method.MethodName).FontSize(9).Bold().FontColor(Colors.Grey.Darken3);
                                        textCol.Item().PaddingTop(4).Text($"{method.TransactionCount} transações")
                                    .FontSize(9).FontColor(Colors.Grey.Medium);
                                    });
                                    row.ConstantItem(120).AlignRight().Column(valueCol =>
                                    {
                                        valueCol.Item().Text($"{method.TotalValue:N2} Kz").FontSize(9).Bold().FontColor(Colors.Green.Darken1);
                                        valueCol.Item().PaddingTop(4).AlignRight().Container()
                                    .Background(color).PaddingVertical(3).PaddingHorizontal(8)
                                    .Text($"{method.Percentage:N1}%").FontSize(9).Bold().FontColor(Colors.White);
                                    });
                                });

                                methodCol.Item().PaddingTop(10).Container().Height(8).Border(1)
                            .BorderColor(Colors.Grey.Lighten2).Row(barRow =>
                                {
                                    barRow.RelativeItem((float)method.Percentage / 100).Background(color);
                                    barRow.RelativeItem(1 - (float)method.Percentage / 100).Background(Colors.Grey.Lighten4);
                                });
                            });

                        colorIndex++;
                    }
                });
            });
        }

        #endregion

        #region Accounts Receivable Report (Contas a Receber)

        public Task GenerateAccountsReceivableReportAsync(CompanyConfigDto companyConfig,
            IEnumerable<AccountsReceivableDto> accountsReceivable, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                        page.Header().Element(c => ComposeModernReportHeader(c, companyConfig, "Contas a Receber",
                            "Gestão e controle de recebíveis", DateTime.Now.AddMonths(-1), DateTime.Now));
                        page.Content().Element(c => ComposeModernAccountsReceivableContent(c, accountsReceivable));
                        page.Footer().Element(c => ComposeProfessionalFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        private void ComposeModernAccountsReceivableContent(IContainer container, IEnumerable<AccountsReceivableDto> accountsReceivable)
        {
            var accounts = accountsReceivable.ToList();
            var totalPending = accounts.Sum(a => a.PendingValue);
            var totalPaid = accounts.Sum(a => a.PaidValue);
            var overdueAccounts = accounts.Where(a => a.DaysOverdue > 0).ToList();
            var totalOverdue = overdueAccounts.Sum(a => a.PendingValue);
            var criticalAccounts = overdueAccounts.Where(a => a.DaysOverdue > 30).Count();

            container.Column(column =>
            {
                // Alert crítico no topo se houver contas vencidas
                if (overdueAccounts.Any())
                {
                    column.Item().PaddingTop(10).Background(Colors.Red.Lighten4).Border(2)
                        .BorderColor(Colors.Red.Medium).Padding(15).Row(row =>
                        {
                            row.AutoItem().Text("🚨").FontSize(32).AlignCenter();
                            row.RelativeItem().PaddingLeft(15).AlignMiddle().Column(col =>
                            {
                                col.Item().Text("ATENÇÃO: FATURAS VENCIDAS DETECTADAS")
                            .FontSize(13).Bold().FontColor(Colors.Red.Darken2);
                                col.Item().PaddingTop(5).Text($"{overdueAccounts.Count} faturas em atraso • Total: {totalOverdue:N2} Kz")
                            .FontSize(10).FontColor(Colors.Red.Darken1);
                                if (criticalAccounts > 0)
                                {
                                    col.Item().PaddingTop(3).Text($"⚠️ {criticalAccounts} faturas com mais de 30 dias de atraso")
                                .FontSize(9).Bold().FontColor(Colors.Red.Darken2);
                                }
                            });
                        });
                }

                // KPIs
                column.Item().PaddingTop(20).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "💰", "A Receber",
                        $"{totalPending:N2} Kz", Colors.Orange.Darken1, Colors.Orange.Lighten4));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "📋", "Faturas",
                        accounts.Count.ToString(), Colors.Blue.Darken1, Colors.Blue.Lighten4));
                    row.ConstantItem(15);
                    row.RelativeItem().Element(c => ComposeModernKpiCard(c, "⚠️", "Em Atraso",
                        $"{totalOverdue:N2} Kz", Colors.Red.Darken1, Colors.Red.Lighten4));
                });

                // Título da seção
                column.Item().PaddingTop(30).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.AutoItem().Width(4).Height(24).Background(Colors.Orange.Medium);
                        row.AutoItem().PaddingLeft(10).Text("FATURAS PENDENTES")
                            .FontSize(13).Bold().FontColor(Colors.Grey.Darken3);
                    });
                    col.Item().PaddingTop(2).Height(2).Background(Colors.Orange.Lighten3);
                });

                // Tabela moderna com status visual
                column.Item().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(1.2f);
                        columns.RelativeColumn(0.8f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Darken2).Padding(6).Text("Fatura")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Grey.Darken2).Padding(6).Text("Cliente")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Grey.Darken2).Padding(6).AlignRight().Text("Vencimento")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Grey.Darken2).Padding(6).AlignRight().Text("Total (Kz)")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Grey.Darken2).Padding(6).AlignRight().Text("Pendente (Kz)")
                            .FontSize(10).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Grey.Darken2).Padding(6).AlignCenter().Text("Status")
                            .FontSize(10).Bold().FontColor(Colors.White);
                    });

                    int rowIndex = 0;
                    foreach (var account in accounts.OrderByDescending(a => a.DaysOverdue))
                    {
                        var isOverdue = account.DaysOverdue > 0;
                        var isCritical = account.DaysOverdue > 30;
                        var bgColor = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                        if (isCritical)
                            bgColor = Colors.Red.Lighten5;
                        else if (isOverdue)
                            bgColor = Colors.Orange.Lighten5;

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).Text(account.InvoiceNumber).FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).Text(account.CustomerName).FontSize(9).FontColor(Colors.Grey.Darken2);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text(account.DueDate.ToString()).FontSize(9).FontColor(Colors.Grey.Darken2);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text($"{account.TotalValue:N2}").FontSize(9).FontColor(Colors.Grey.Darken2);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignRight().Text($"{account.PendingValue:N2}").FontSize(9).Bold()
                            .FontColor(isOverdue ? Colors.Red.Darken1 : Colors.Orange.Darken1);

                        // Status Badge
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(5).AlignCenter().Container().Width(60).Column(statusCol =>
                            {
                                if (isCritical)
                                {
                                    statusCol.Item().Background(Colors.Red.Darken1).PaddingVertical(4)
                                .PaddingHorizontal(6).Text("CRÍTICO").FontSize(7).Bold().FontColor(Colors.White);
                                }
                                else if (isOverdue)
                                {
                                    statusCol.Item().Background(Colors.Orange.Darken1).PaddingVertical(4)
                                .PaddingHorizontal(6).Text($"+{account.DaysOverdue}d").FontSize(7).Bold().FontColor(Colors.White);
                                }
                                else
                                {
                                    statusCol.Item().Background(Colors.Green.Medium).PaddingVertical(4)
                                .PaddingHorizontal(6).Text("OK").FontSize(7).Bold().FontColor(Colors.White);
                                }
                            });

                        rowIndex++;
                    }
                });

                // Recomendações
                column.Item().PaddingTop(30).Background(Colors.Blue.Lighten5).Border(1)
                    .BorderColor(Colors.Blue.Lighten2).Padding(20).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.AutoItem().Text("💡").FontSize(24);
                            row.AutoItem().PaddingLeft(10).Text("AÇÕES RECOMENDADAS")
                        .FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                        });

                        col.Item().PaddingTop(12).PaddingLeft(10).Column(actions =>
                        {
                            actions.Item().PaddingVertical(4).Row(row =>
                            {
                                row.AutoItem().Width(6).Height(6).Background(Colors.Blue.Medium).AlignMiddle();
                                row.AutoItem().PaddingLeft(8).Text("Priorizar cobrança das faturas críticas (>30 dias)")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);
                            });

                            actions.Item().PaddingVertical(4).Row(row =>
                            {
                                row.AutoItem().Width(6).Height(6).Background(Colors.Blue.Medium).AlignMiddle();
                                row.AutoItem().PaddingLeft(8).Text("Entrar em contato com clientes em atraso")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);
                            });

                            actions.Item().PaddingVertical(4).Row(row =>
                            {
                                row.AutoItem().Width(6).Height(6).Background(Colors.Blue.Medium).AlignMiddle();
                                row.AutoItem().PaddingLeft(8).Text("Considerar descontos para pagamento antecipado")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);
                            });

                            actions.Item().PaddingVertical(4).Row(row =>
                            {
                                row.AutoItem().Width(6).Height(6).Background(Colors.Blue.Medium).AlignMiddle();
                                row.AutoItem().PaddingLeft(8).Text("Revisar política de crédito para clientes recorrentes")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);
                            });
                        });
                    });
            });
        }

        #endregion

        #region Helper Methods (Métodos Auxiliares Modernos)

        private void ComposeModernReportHeader(IContainer container, CompanyConfigDto company,
            string title, string subtitle, DateTime startDate, DateTime endDate)
        {
            container.Column(column =>
            {
                // Cabeçalho moderno com gradiente simulado - OTIMIZADO
                column.Item().Background(Colors.Blue.Lighten5).Padding(5).Row(row =>
                {
                    // Logo - reduzido
                    if (!string.IsNullOrWhiteSpace(company.LogoUrl) && File.Exists(company.LogoUrl))
                    {
                        row.ConstantItem(55).Column(logoCol =>
                        {
                            logoCol.Item().Border(1).BorderColor(Colors.White).MaxHeight(45).Image(company.LogoUrl);
                        });
                        row.ConstantItem(8);
                    }

                    // Informações principais - fontes reduzidas
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(company.CompanyName ?? "VendaFlex")
                            .FontSize(13).Bold().FontColor(Colors.Blue.Darken3);

                        col.Item().PaddingTop(3).Row(titleRow =>
                        {
                            titleRow.AutoItem().Width(3).Height(12).Background(Colors.Blue.Medium).AlignMiddle();
                            titleRow.AutoItem().PaddingLeft(6).Text(title)
                                .FontSize(9).Bold().FontColor(Colors.Grey.Darken3);
                        });

                        col.Item().PaddingTop(2).PaddingLeft(9).Text(subtitle)
                            .FontSize(8).Italic().FontColor(Colors.Grey.Medium);

                        col.Item().PaddingTop(4).PaddingLeft(9).Row(dateRow =>
                        {
                            dateRow.AutoItem().Text("📅 ").FontSize(8);
                            dateRow.AutoItem().Text($"Período: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}")
                                .FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    // Data de geração - compacta
                    row.ConstantItem(65).AlignRight().Column(col =>
                    {
                        col.Item().Background(Colors.Blue.Darken1).Padding(4)
                            .Text("RELATÓRIO").FontSize(7).Bold().FontColor(Colors.White);
                        col.Item().PaddingTop(2).Text(DateTime.Now.ToString("dd/MM/yyyy"))
                            .FontSize(8).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text(DateTime.Now.ToString("HH:mm"))
                            .FontSize(7).FontColor(Colors.Grey.Medium);
                    });
                });

                // Linha decorativa - reduzida
                column.Item().PaddingTop(3).Height(2).Row(row =>
                {
                    row.RelativeItem(3).Background(Colors.Blue.Darken1);
                    row.RelativeItem(2).Background(Colors.Blue.Medium);
                    row.RelativeItem(1).Background(Colors.Blue.Lighten2);
                });
            });
        }

        private void ComposeModernKpiCard(IContainer container, string icon, string label,
            string value, string valueColor, string backgroundColor)
        {
            container.Border(1).BorderColor(valueColor).Background(backgroundColor)
                .Padding(4).Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.AutoItem().Text(icon).FontSize(9);
                        row.RelativeItem().AlignRight().Container().Background(valueColor)
                    .Width(18).Height(18).AlignMiddle().AlignCenter()
                    .Text("•").FontSize(9).FontColor(Colors.White);
                    });

                    column.Item().PaddingTop(5).Text(label)
                .FontSize(7).FontColor(Colors.Grey.Darken1);

                    column.Item().PaddingTop(3).Text(value)
                .FontSize(10).Bold().FontColor(valueColor);
                });
        }

        private void ComposeProfessionalFooter(IContainer container, CompanyConfigDto company)
        {
            container.Column(column =>
            {
                // Linha decorativa - reduzida
                column.Item().Height(1).Background(Colors.Blue.Lighten3);

                column.Item().PaddingTop(8).Row(row =>
                {
                    // Informações da empresa - compactas
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(company.CompanyName ?? "VendaFlex")
                            .FontSize(7).Bold().FontColor(Colors.Grey.Darken2);

                        if (!string.IsNullOrWhiteSpace(company.Address))
                        {
                            col.Item().PaddingTop(1).Text(company.Address)
                                .FontSize(6).FontColor(Colors.Grey.Medium);
                        }

                        if (!string.IsNullOrWhiteSpace(company.PhoneNumber))
                        {
                            col.Item().PaddingTop(1).Row(phoneRow =>
                            {
                                phoneRow.AutoItem().Text("📞 ").FontSize(6);
                                phoneRow.AutoItem().Text(company.PhoneNumber)
                                    .FontSize(6).FontColor(Colors.Grey.Medium);
                            });
                        }
                    });

                    // Data e hora de geração - compacta
                    row.ConstantItem(140).AlignRight().Column(col =>
                    {
                        col.Item().Text("Documento gerado em:")
                            .FontSize(6).FontColor(Colors.Grey.Medium);
                        col.Item().PaddingTop(1).Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"))
                            .FontSize(7).Bold().FontColor(Colors.Grey.Darken2);
                        col.Item().PaddingTop(3).Text("Sistema VendaFlex")
                            .FontSize(6).Italic().FontColor(Colors.Grey.Medium);
                    });
                });

                // Numeração de página - compacta
                column.Item().PaddingTop(5).AlignCenter().Text(text =>
                {
                    text.Span("Página ").FontSize(6).FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber().FontSize(6).Bold().FontColor(Colors.Blue.Medium);
                    text.Span(" de ").FontSize(6).FontColor(Colors.Grey.Medium);
                    text.TotalPages().FontSize(6).Bold().FontColor(Colors.Blue.Medium);
                });
            });
        }

        #endregion


        // Metodo
        public Task GenerateFinancialStatementReportAsync(CompanyConfigDto companyConfig, decimal totalRevenue, decimal totalCost,
            decimal totalProfit, decimal profitMargin, IEnumerable<PaymentMethodDto> paymentMethods, IEnumerable<AccountsReceivableDto> accountsReceivable,
            DateTime startDate, DateTime endDate, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8));

                        page.Header().Element(c => ComposeReportHeader(c, companyConfig, "Demonstrativo Financeiro", startDate, endDate));
                        page.Content().Element(c => ComposeFinancialStatementContent(c, totalRevenue, totalCost, totalProfit, profitMargin, paymentMethods, accountsReceivable));
                        page.Footer().Element(c => ComposeSimpleFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        #region Financial Report Components

        private void ComposeSimpleReportHeader(IContainer container, CompanyConfigDto company, string title)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    // Logo (se existir)
                    if (!string.IsNullOrWhiteSpace(company.LogoUrl) && File.Exists(company.LogoUrl))
                    {
                        row.ConstantItem(80).Column(logoCol =>
                        {
                            logoCol.Item().MaxHeight(60).Image(company.LogoUrl);
                        });
                        row.ConstantItem(10); // Espaçamento company.CompanyName
                    }
                    
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(company.CompanyName ?? "VendaFlex")
                            .FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text(title)
                            .FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);
                    });
                    
                    row.ConstantItem(100).AlignRight().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                        .FontSize(9).FontColor(Colors.Grey.Medium);
                });

                column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Medium);
            });
        }

        private void ComposeCashFlowContent(IContainer container, IEnumerable<CashFlowDto> cashFlowData)
        {
            var data = cashFlowData.ToList();
            var totalInflow = data.Sum(c => c.Inflow);
            var totalOutflow = data.Sum(c => c.Outflow);
            var netFlow = totalInflow - totalOutflow;

            container.Column(column =>
            {
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Total Entradas", $"{totalInflow:N2} Kz", Colors.Green.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Total Saídas", $"{totalOutflow:N2} Kz", Colors.Red.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Fluxo Líquido", $"{netFlow:N2} Kz", netFlow >= 0 ? Colors.Blue.Medium : Colors.Orange.Medium));
                });

                column.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Data").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Green.Lighten3).Padding(4).AlignRight().Text("Entradas (Kz)").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Red.Lighten3).Padding(4).AlignRight().Text("Saídas (Kz)").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Saldo (Kz)").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Acumulado (Kz)").FontSize(10).SemiBold();
                    });

                    foreach (var flow in data)
                    {
                        var balance = flow.Inflow - flow.Outflow;
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(flow.DateFormatted).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{flow.Inflow:N2}").FontSize(9).FontColor(Colors.Green.Darken1);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{flow.Outflow:N2}").FontSize(9).FontColor(Colors.Red.Darken1);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{balance:N2}").FontSize(9).Bold();
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{flow.Balance:N2}").FontSize(9).Bold().FontColor(flow.Balance >= 0 ? Colors.Blue.Medium : Colors.Red.Medium);
                    }
                });
            });
        }

        private void ComposePaymentMethodsContent(IContainer container, IEnumerable<PaymentMethodDto> paymentMethods)
        {
            var methods = paymentMethods.ToList();
            var totalValue = methods.Sum(m => m.TotalValue);
            var totalTransactions = methods.Sum(m => m.TransactionCount);

            container.Column(column =>
            {
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Valor Total", $"{totalValue:N2} Kz", Colors.Green.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Total de Transações", totalTransactions.ToString(), Colors.Blue.Medium));
                });

                column.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Forma de Pagamento").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Transações").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Valor Total (Kz)").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("% do Total").FontSize(10).SemiBold();
                    });

                    foreach (var method in methods)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(method.MethodName).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(method.TransactionCount.ToString()).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{method.TotalValue:N2}").FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{method.Percentage:N1}%").FontSize(9).Bold();
                    }
                });

                // Gráfico visual de percentuais
                column.Item().PaddingTop(20).Text("Distribuição Visual").FontSize(9).SemiBold();
                column.Item().PaddingTop(10).Column(col =>
                {
                    foreach (var method in methods.Take(5))
                    {
                        col.Item().PaddingVertical(5).Row(row =>
                        {
                            row.ConstantItem(150).Text(method.MethodName).FontSize(9);
                            row.RelativeItem().Container().Height(20).Border(1).BorderColor(Colors.Grey.Lighten2).Row(innerRow =>
                            {
                                innerRow.RelativeItem((float)method.Percentage / 100).Background(Colors.Blue.Medium);
                                innerRow.RelativeItem(1 - (float)method.Percentage / 100).Background(Colors.Grey.Lighten3);
                            });
                            row.ConstantItem(60).AlignRight().Text($"{method.Percentage:N1}%").FontSize(9).Bold();
                        });
                    }
                });
            });
        }

        private void ComposeAccountsReceivableContent(IContainer container, IEnumerable<AccountsReceivableDto> accountsReceivable)
        {
            var accounts = accountsReceivable.ToList();
            var totalPending = accounts.Sum(a => a.PendingValue);
            var overdueAccounts = accounts.Where(a => a.DaysOverdue > 0).ToList();
            var totalOverdue = overdueAccounts.Sum(a => a.PendingValue);

            container.Column(column =>
            {
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Total a Receber", $"{totalPending:N2} Kz", Colors.Orange.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Faturas Pendentes", accounts.Count.ToString(), Colors.Blue.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Em Atraso", $"{totalOverdue:N2} Kz", Colors.Red.Medium));
                });

                if (overdueAccounts.Any())
                {
                    column.Item().PaddingTop(20).Background(Colors.Red.Lighten4).Padding(5)
                        .Text($"⚠️ ATENÇÃO: {overdueAccounts.Count} faturas em atraso!")
                        .FontSize(9).Bold().FontColor(Colors.Red.Darken2);
                }

                column.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Fatura").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Cliente").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Vencimento").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Total (Kz)").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Pendente (Kz)").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Dias").FontSize(10).SemiBold();
                    });

                    foreach (var account in accounts.OrderByDescending(a => a.DaysOverdue))
                    {
                        var isOverdue = account.DaysOverdue > 0;
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(account.InvoiceNumber).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(account.CustomerName).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(account.DueDate.ToString()).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{account.TotalValue:N2}").FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{account.PaidValue:N2}").FontSize(9).Bold()
                            .FontColor(isOverdue ? Colors.Red.Medium : Colors.Orange.Medium);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(account.DaysOverdue > 0 ? $"+{account.DaysOverdue}" : "OK")
                            .FontSize(9).Bold().FontColor(isOverdue ? Colors.Red.Medium : Colors.Green.Medium);
                    }
                });
            });
        }

        private void ComposeFinancialStatementContent(IContainer container, decimal totalRevenue, decimal totalCost, decimal totalProfit, 
            decimal profitMargin, IEnumerable<PaymentMethodDto> paymentMethods, IEnumerable<AccountsReceivableDto> accountsReceivable)
        {
            var totalReceivable = accountsReceivable.Sum(a => a.PendingValue);

            container.Column(column =>
            {
                // DRE Simplificado
                column.Item().Text("Demonstrativo de Resultados").FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);

                column.Item().PaddingTop(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                    });

                    // Receita
                    table.Cell().Padding(4).Text("(+) Receita Total").FontSize(9).SemiBold();
                    table.Cell().Padding(4).AlignRight().Text($"{totalRevenue:N2} Kz").FontSize(9).Bold().FontColor(Colors.Green.Medium);

                    // Custo
                    table.Cell().Padding(4).Text("(-) Custo Total").FontSize(9).SemiBold();
                    table.Cell().Padding(4).AlignRight().Text($"{totalCost:N2} Kz").FontSize(9).Bold().FontColor(Colors.Red.Medium);

                    // Linha divisória
                    table.Cell().ColumnSpan(2).BorderTop(2).BorderColor(Colors.Grey.Darken1).PaddingTop(5);

                    // Lucro
                    table.Cell().Padding(4).Text("(=) Lucro Bruto").FontSize(9).Bold();
                    table.Cell().Padding(4).AlignRight().Text($"{totalProfit:N2} Kz").FontSize(9).Bold().FontColor(Colors.Blue.Medium);

                    // Margem
                    table.Cell().Padding(4).Text("Margem de Lucro").FontSize(9).SemiBold();
                    table.Cell().Padding(4).AlignRight().Text($"{profitMargin:N2}%").FontSize(9).Bold().FontColor(Colors.Orange.Medium);
                });

                // Formas de Pagamento Resumo
                column.Item().PaddingTop(30).Text("Formas de Pagamento (Resumo)").FontSize(9).SemiBold();
                column.Item().PaddingTop(10).Column(col =>
                {
                    foreach (var method in paymentMethods.Take(5))
                    {
                        col.Item().PaddingVertical(3).Row(row =>
                        {
                            row.RelativeItem().Text($"• {method.MethodName}").FontSize(10);
                            row.ConstantItem(150).AlignRight().Text($"{method.TotalValue:N2} Kz ({method.Percentage:N1}%)")
                                .FontSize(10).FontColor(Colors.Grey.Darken1);
                        });
                    }
                });

                // Contas a Receber Resumo
                column.Item().PaddingTop(20).Text($"Contas a Receber: {totalReceivable:N2} Kz")
                    .FontSize(9).SemiBold().FontColor(Colors.Orange.Darken1);
                column.Item().PaddingTop(5).Text($"{accountsReceivable.Count()} faturas pendentes")
                    .FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        }

        #endregion

        public Task GenerateStockMovementsReportAsync(CompanyConfigDto companyConfig, IEnumerable<StockMovementReportDto> stockMovements, DateTime startDate,
            DateTime endDate, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape()); // Paisagem para mais colunas
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8));

                        page.Header().Element(c => ComposeReportHeader(c, companyConfig, "Movimentação de Estoque", startDate, endDate));
                        page.Content().Element(c => ComposeStockMovementsContent(c, stockMovements));
                        page.Footer().Element(c => ComposeSimpleFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        public Task GenerateLowStockReportAsync(CompanyConfigDto companyConfig, IEnumerable<LowStockDto> lowStockProducts, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8));

                        page.Header().Element(c => ComposeSimpleReportHeader(c, companyConfig, "Produtos com Estoque Baixo"));
                        page.Content().Element(c => ComposeLowStockContent(c, lowStockProducts));
                        page.Footer().Element(c => ComposeSimpleFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        public Task GenerateExpiringProductsReportAsync(CompanyConfigDto companyConfig, IEnumerable<ExpirationReportDto> expiringProducts,
            int daysThreshold, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8));

                        page.Header().Element(c => ComposeSimpleReportHeader(c, companyConfig, $"Produtos Vencendo (Próximos {daysThreshold} dias)"));
                        page.Content().Element(c => ComposeExpiringProductsContent(c, expiringProducts, daysThreshold));
                        page.Footer().Element(c => ComposeSimpleFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        #region Stock Report Components

        private void ComposeStockMovementsContent(IContainer container, IEnumerable<StockMovementReportDto> stockMovements)
        {
            var movements = stockMovements.ToList();
            var entries = movements.Where(m => m.MovementType == "Entrada").Sum(m => m.Quantity);
            var exits = movements.Where(m => m.MovementType == "Saída").Sum(m => m.Quantity);

            container.Column(column =>
            {
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Total de Movimentos", movements.Count.ToString(), Colors.Blue.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Entradas", entries.ToString(), Colors.Green.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Saídas", exits.ToString(), Colors.Red.Medium));
                });

                column.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Data").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Produto").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignCenter().Text("Tipo").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Qtd").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Saldo").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Motivo").FontSize(10).SemiBold();
                    });

                    foreach (var movement in movements)
                    {
                        var isEntry = movement.MovementType == "Entrada";
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(movement.Date.ToShortDateString()).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(movement.ProductName).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignCenter().Text(movement.MovementType).FontSize(9).Bold()
                            .FontColor(isEntry ? Colors.Green.Medium : Colors.Red.Medium);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(movement.Quantity.ToString()).FontSize(9).Bold();
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text("-").FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(movement.Reason ?? "-").FontSize(8).FontColor(Colors.Grey.Darken1);
                    }
                });
            });
        }

        private void ComposeLowStockContent(IContainer container, IEnumerable<LowStockDto> lowStockProducts)
        {
            var products = lowStockProducts.ToList();
            var critical = products.Where(p => p.CurrentQuantity <= p.MinimumQuantity / 2).Count();

            container.Column(column =>
            {
                column.Item().PaddingTop(15).Background(Colors.Orange.Lighten4).Padding(15).Row(row =>
                {
                    row.ConstantItem(50).AlignMiddle().Text("⚠️").FontSize(32);
                    row.RelativeItem().AlignMiddle().Column(col =>
                    {
                        col.Item().Text($"{products.Count} produtos necessitam reposição")
                            .FontSize(9).Bold().FontColor(Colors.Orange.Darken2);
                        col.Item().Text($"{critical} em situação crítica (abaixo de 50% do mínimo)")
                            .FontSize(9).FontColor(Colors.Red.Darken1);
                    });
                });

                column.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Orange.Lighten3).Padding(4).Text("Produto").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Orange.Lighten3).Padding(4).AlignRight().Text("Atual").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Orange.Lighten3).Padding(4).AlignRight().Text("Mínimo").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Orange.Lighten3).Padding(4).AlignRight().Text("Reposição").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Orange.Lighten3).Padding(4).AlignRight().Text("Diferença").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Orange.Lighten3).Padding(4).AlignCenter().Text("Status").FontSize(10).SemiBold();
                    });

                    foreach (var product in products.OrderBy(p => p.CurrentQuantity))
                    {
                        var isCritical = product.CurrentQuantity <= product.MinimumQuantity / 2;
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(product.ProductName).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(product.CurrentQuantity.ToString()).FontSize(9).Bold()
                            .FontColor(isCritical ? Colors.Red.Medium : Colors.Orange.Medium);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(product.MinimumQuantity.ToString()).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(product.ReorderPoint.ToString()).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(product.Difference.ToString()).FontSize(9).FontColor(Colors.Red.Darken1);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignCenter().Text(isCritical ? "CRÍTICO" : "BAIXO").FontSize(8).Bold()
                            .FontColor(isCritical ? Colors.Red.Darken2 : Colors.Orange.Darken1);
                    }
                });

                // Recomendações
                column.Item().PaddingTop(20).Background(Colors.Blue.Lighten4).Padding(15).Column(col =>
                {
                    col.Item().Text("💡 Recomendações").FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(8).Text("• Priorize a reposição dos produtos em situação crítica").FontSize(10);
                    col.Item().Text("• Verifique fornecedores e prazos de entrega").FontSize(10);
                    col.Item().Text("• Considere ajustar os níveis mínimos de estoque com base no histórico de vendas").FontSize(10);
                });
            });
        }

        private void ComposeExpiringProductsContent(IContainer container, IEnumerable<ExpirationReportDto> expiringProducts, int daysThreshold)
        {
            var products = expiringProducts.ToList();
            var expired = products.Where(p => p.DaysToExpire < 0).Count();
            var critical = products.Where(p => p.DaysToExpire >= 0 && p.DaysToExpire <= 7).Count();
            var warning = products.Where(p => p.DaysToExpire > 7 && p.DaysToExpire <= daysThreshold).Count();

            container.Column(column =>
            {
                column.Item().PaddingTop(15).Background(Colors.Red.Lighten4).Padding(15).Row(row =>
                {
                    row.ConstantItem(50).AlignMiddle().Text("⏰").FontSize(32);
                    row.RelativeItem().AlignMiddle().Column(col =>
                    {
                        col.Item().Text($"{products.Count} produtos requerem atenção quanto à validade")
                            .FontSize(9).Bold().FontColor(Colors.Red.Darken2);
                        col.Item().Row(r =>
                        {
                            if (expired > 0)
                                r.AutoItem().Padding(2).Text($"• {expired} VENCIDOS").FontSize(10).Bold().FontColor(Colors.Red.Darken2);
                            if (critical > 0)
                                r.AutoItem().Padding(2).Text($"• {critical} Críticos (≤7 dias)").FontSize(10).FontColor(Colors.Orange.Darken1);
                            if (warning > 0)
                                r.AutoItem().Padding(2).Text($"• {warning} Atenção ({daysThreshold} dias)").FontSize(10).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });

                column.Item().PaddingTop(20).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Red.Lighten3).Padding(4).Text("Produto").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Red.Lighten3).Padding(4).Text("Lote").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Red.Lighten3).Padding(4).AlignRight().Text("Validade").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Red.Lighten3).Padding(4).AlignRight().Text("Dias").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Red.Lighten3).Padding(4).AlignCenter().Text("Status").FontSize(10).SemiBold();
                    });

                    foreach (var product in products.OrderBy(p => p.DaysToExpire))
                    {
                        var statusText = product.DaysToExpire < 0 ? "VENCIDO" : 
                                        product.DaysToExpire <= 7 ? "CRÍTICO" : "ATENÇÃO";
                        var statusColor = product.DaysToExpire < 0 ? Colors.Red.Darken2 :
                                         product.DaysToExpire <= 7 ? Colors.Orange.Darken1 : Colors.Yellow.Darken2;
                        
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(product.ProductName).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(product.Batch ?? "-").FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(product.ExpirationDateFormatted).FontSize(9);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(product.DaysToExpire.ToString()).FontSize(9).Bold()
                            .FontColor(statusColor);
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignCenter().Text(statusText).FontSize(8).Bold()
                            .FontColor(statusColor);
                    }
                });

                // Ações Recomendadas
                column.Item().PaddingTop(20).Background(Colors.Orange.Lighten4).Padding(15).Column(col =>
                {
                    col.Item().Text("🎯 Ações Recomendadas").FontSize(9).Bold().FontColor(Colors.Orange.Darken2);
                    col.Item().PaddingTop(8).Text("• Produtos vencidos devem ser removidos imediatamente do estoque").FontSize(10);
                    col.Item().Text("• Produtos críticos (≤7 dias) devem ser priorizados para venda ou promoção").FontSize(10);
                    col.Item().Text("• Considere descontos progressivos para produtos próximos ao vencimento").FontSize(10);
                    col.Item().Text("• Revise políticas de compra para evitar excesso de estoque com validade curta").FontSize(10);
                });
            });
        }

        #endregion

        public Task GenerateExecutiveDashboardReportAsync(CompanyConfigDto companyConfig, decimal totalSales, decimal totalRevenue,
            decimal profitMargin, int totalInvoices, decimal pendingAmount, decimal stockValue, IEnumerable<TopProductDto> topProducts,
            IEnumerable<PaymentMethodDto> paymentMethods, DateTime startDate, DateTime endDate, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8));

                        page.Header().Element(c => ComposeReportHeader(c, companyConfig, "Dashboard Executivo", startDate, endDate));
                        page.Content().Element(c => ComposeExecutiveDashboardContent(c, totalSales, totalRevenue, profitMargin, 
                            totalInvoices, pendingAmount, stockValue, topProducts, paymentMethods));
                        page.Footer().Element(c => ComposeSimpleFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        public Task GenerateUserPerformanceReportAsync(CompanyConfigDto companyConfig, IEnumerable<SalesByUserDto> salesByUser, DateTime startDate,
            DateTime endDate, string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8));

                        page.Header().Element(c => ComposeReportHeader(c, companyConfig, "Desempenho por Vendedor", startDate, endDate));
                        page.Content().Element(c => ComposeUserPerformanceContent(c, salesByUser));
                        page.Footer().Element(c => ComposeSimpleFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        #region Management Report Components

        private void ComposeExecutiveDashboardContent(IContainer container, decimal totalSales, decimal totalRevenue, 
            decimal profitMargin, int totalInvoices, decimal pendingAmount, decimal stockValue, 
            IEnumerable<TopProductDto> topProducts, IEnumerable<PaymentMethodDto> paymentMethods)
        {
            container.Column(column =>
            {
                // KPIs Principais
                column.Item().Text("Indicadores Principais").FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);

                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Vendas Totais", $"{totalSales:N2} Kz", Colors.Blue.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Receita Total", $"{totalRevenue:N2} Kz", Colors.Green.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Margem de Lucro", $"{profitMargin:N2}%", Colors.Orange.Medium));
                });

                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Total de Faturas", totalInvoices.ToString(), Colors.Purple.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "A Receber", $"{pendingAmount:N2} Kz", Colors.Orange.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Valor em Estoque", $"{stockValue:N2} Kz", Colors.Teal.Medium));
                });

                // Top 5 Produtos
                var products = topProducts.Take(5).ToList();
                if (products.Any())
                {
                    column.Item().PaddingTop(30).Text("🏆 Top 5 Produtos").FontSize(13).SemiBold().FontColor(Colors.Grey.Darken2);
                    
                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(30);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("#").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Produto").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Qtd").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Receita (Kz)").FontSize(10).SemiBold();
                        });

                        int rank = 1;
                        foreach (var product in products)
                        {
                            var bgColor = rank == 1 ? Colors.Yellow.Lighten3 : 
                                         rank == 2 ? Colors.Grey.Lighten3 : 
                                         rank == 3 ? Colors.Orange.Lighten3 : Colors.White;

                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignCenter().Text(rank.ToString()).FontSize(10).Bold();
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(product.ProductName).FontSize(10);
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text(product.QuantitySold.ToString()).FontSize(10);
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text($"{product.Revenue:N2}").FontSize(10).Bold();
                            
                            rank++;
                        }
                    });
                }

                // Formas de Pagamento
                var methods = paymentMethods.ToList();
                if (methods.Any())
                {
                    column.Item().PaddingTop(30).Text("💳 Formas de Pagamento").FontSize(13).SemiBold().FontColor(Colors.Grey.Darken2);
                    
                    column.Item().PaddingTop(10).Column(col =>
                    {
                        foreach (var method in methods.Take(5))
                        {
                            col.Item().PaddingVertical(8).Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Padding(6).Row(row =>
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text(method.MethodName).FontSize(9).SemiBold();
                                        c.Item().Text($"{method.TransactionCount} transações").FontSize(9).FontColor(Colors.Grey.Darken1);
                                    });
                                    row.ConstantItem(150).AlignRight().Column(c =>
                                    {
                                        c.Item().Text($"{method.TotalValue:N2} Kz").FontSize(9).Bold().FontColor(Colors.Green.Medium);
                                        c.Item().Text($"{method.Percentage:N1}%").FontSize(9).FontColor(Colors.Blue.Medium);
                                    });
                                });
                        }
                    });
                }

                // Insights
                column.Item().PaddingTop(30).Background(Colors.Blue.Lighten4).Padding(15).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.ConstantItem(40).Text("💡").FontSize(28);
                        row.RelativeItem().AlignMiddle().Column(c =>
                        {
                            c.Item().Text("Insights Rápidos").FontSize(13).Bold().FontColor(Colors.Blue.Darken2);
                            c.Item().PaddingTop(5).Text($"• Margem de lucro {(profitMargin > 20 ? "saudável" : "necessita atenção")} ({profitMargin:N2}%)").FontSize(10);
                            c.Item().Text($"• Valor pendente representa {(totalRevenue > 0 ? (pendingAmount/totalRevenue*100) : 0):N1}% da receita").FontSize(10);
                            c.Item().Text($"• Estoque equivale a {(totalRevenue > 0 ? (stockValue/totalRevenue*100) : 0):N1}% da receita do período").FontSize(10);
                        });
                    });
                });
            });
        }

        private void ComposeUserPerformanceContent(IContainer container, IEnumerable<SalesByUserDto> salesByUser)
        {
            var users = salesByUser.ToList();
            var totalSales = users.Sum(u => u.TotalValue);
            var totalInvoices = users.Sum(u => u.InvoiceCount);

            container.Column(column =>
            {
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Total de Vendedores", users.Count.ToString(), Colors.Purple.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Vendas Totais", $"{totalSales:N2} Kz", Colors.Green.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Total de Vendas", totalInvoices.ToString(), Colors.Blue.Medium));
                });

                // Ranking de Vendedores
                column.Item().PaddingTop(20).Text("🏆 Ranking de Desempenho").FontSize(13).SemiBold().FontColor(Colors.Grey.Darken2);

                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignCenter().Text("Pos.").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Vendedor").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Vendas").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Total (Kz)").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Ticket Médio").FontSize(10).SemiBold();
                        header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("% Total").FontSize(10).SemiBold();
                    });

                    int position = 1;
                    foreach (var user in users.OrderByDescending(u => u.TotalValue))
                    {
                        var avgTicket = user.InvoiceCount > 0 ? user.TotalValue / user.InvoiceCount : 0;
                        var percentage = totalSales > 0 ? (user.TotalValue / totalSales) * 100 : 0;
                        
                        var bgColor = position == 1 ? Colors.Yellow.Lighten3 : 
                                     position == 2 ? Colors.Grey.Lighten3 : 
                                     position == 3 ? Colors.Orange.Lighten3 : Colors.White;
                        
                        var trophy = position == 1 ? "🥇" : position == 2 ? "🥈" : position == 3 ? "🥉" : "";

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignCenter().Text($"{trophy} {position}").FontSize(10).Bold();
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .Text(user.UserName).FontSize(9).SemiBold();
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text(user.InvoiceCount.ToString()).FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{user.TotalValue:N2}").FontSize(9).Bold().FontColor(Colors.Green.Medium);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{avgTicket:N2}").FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                            .AlignRight().Text($"{percentage:N1}%").FontSize(9).Bold().FontColor(Colors.Blue.Medium);
                        
                        position++;
                    }
                });

                // Análise de Desempenho
                if (users.Any())
                {
                    var topPerformer = users.OrderByDescending(u => u.TotalValue).First();
                    var avgSalesPerUser = users.Average(u => u.TotalValue);
                    var avgInvoicesPerUser = users.Average(u => u.InvoiceCount);

                    column.Item().PaddingTop(30).Background(Colors.Green.Lighten4).Padding(15).Column(col =>
                    {
                        col.Item().Text("📊 Análise de Desempenho").FontSize(9).Bold().FontColor(Colors.Green.Darken2);
                        col.Item().PaddingTop(8).Text($"• Melhor Vendedor: {topPerformer.UserName} com {topPerformer.TotalValue:N2} Kz")
                            .FontSize(10).SemiBold();
                        col.Item().Text($"• Média de vendas por vendedor: {avgSalesPerUser:N2} Kz")
                            .FontSize(10);
                        col.Item().Text($"• Média de faturas por vendedor: {avgInvoicesPerUser:N1}")
                            .FontSize(10);
                        
                        var topPerformerShare = totalSales > 0 ? (topPerformer.TotalValue / totalSales) * 100 : 0;
                        if (topPerformerShare > 50)
                        {
                            col.Item().PaddingTop(5).Text($"⚠️ Atenção: O melhor vendedor concentra {topPerformerShare:N1}% das vendas. Considere estratégias para equilibrar o desempenho da equipe.")
                                .FontSize(9).FontColor(Colors.Orange.Darken1);
                        }
                    });
                }

                // Gráfico de Barras Visual
                column.Item().PaddingTop(20).Text("Comparativo Visual").FontSize(9).SemiBold();
                column.Item().PaddingTop(10).Column(col =>
                {
                    var maxValue = users.Any() ? users.Max(u => u.TotalValue) : 1;
                    
                    foreach (var user in users.OrderByDescending(u => u.TotalValue).Take(10))
                    {
                        col.Item().PaddingVertical(5).Row(row =>
                        {
                            row.ConstantItem(150).Text(user.UserName).FontSize(9);
                            row.RelativeItem().Container().Height(25).Border(1).BorderColor(Colors.Grey.Lighten2)
                                .Row(innerRow =>
                                {
                                    var percentage = maxValue > 0 ? (float)(user.TotalValue / maxValue) : 0;
                                    innerRow.RelativeItem(percentage).Background(Colors.Blue.Medium).Padding(2)
                                        .AlignMiddle().Text($"{user.TotalValue:N0} Kz").FontSize(8).FontColor(Colors.White);
                                    innerRow.RelativeItem(1 - percentage).Background(Colors.Grey.Lighten3);
                                });
                        });
                    }
                });
            });
        }

        #endregion

        public Task GenerateComprehensiveReportAsync(CompanyConfigDto companyConfig, ComprehensiveReportDto reportData,
            string filePath)
        {
            return Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(8));

                        page.Header().Element(c => ComposeComprehensiveHeader(c, companyConfig, reportData));
                        page.Content().Element(c => ComposeComprehensiveContent(c, reportData));
                        page.Footer().Element(c => ComposeSimpleFooter(c, companyConfig));
                    });
                })
                .GeneratePdf(filePath);
            });
        }

        #region Comprehensive Report Components

        private void ComposeComprehensiveHeader(IContainer container, CompanyConfigDto company, ComprehensiveReportDto data)
        {
            container.Column(column =>
            {
                // Logo e título
                column.Item().Row(row =>
                {
                    // Logo (se existir)
                    if (!string.IsNullOrWhiteSpace(company.LogoUrl) && File.Exists(company.LogoUrl))
                    {
                        row.ConstantItem(100).Column(logoCol =>
                        {
                            logoCol.Item().MaxHeight(80).Image(company.LogoUrl);
                        });
                        row.ConstantItem(15); // Espaçamento
                    }
                    
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(company.CompanyName ?? "VendaFlex")
                            .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Relatório {data.ReportType}")
                            .FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);
                        col.Item().Text($"Período: {data.StartDateFormatted} - {data.EndDateFormatted}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                    
                    row.ConstantItem(100).AlignRight().Column(col =>
                    {
                        col.Item().Text("RELATÓRIO GERAL").FontSize(10).Bold().FontColor(Colors.Blue.Medium);
                        col.Item().Text(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(9).FontColor(Colors.Grey.Medium);
                    });
                });

                column.Item().PaddingTop(15).LineHorizontal(2).LineColor(Colors.Blue.Medium);
            });
        }

        private void ComposeComprehensiveContent(IContainer container, ComprehensiveReportDto data)
        {
            container.Column(column =>
            {
                // RESUMO EXECUTIVO
                column.Item().PaddingTop(20).Element(c => ComposeExecutiveSummary(c, data));

                // ANÁLISE DE VENDAS
                column.Item().PageBreak();
                column.Item().Element(c => ComposeSalesAnalysis(c, data));

                // ANÁLISE FINANCEIRA
                column.Item().PageBreak();
                column.Item().Element(c => ComposeFinancialAnalysis(c, data));

                // ANÁLISE DE CLIENTES
                column.Item().PageBreak();
                column.Item().Element(c => ComposeCustomerAnalysis(c, data));

                // ANÁLISE DE ESTOQUE
                column.Item().PageBreak();
                column.Item().Element(c => ComposeStockAnalysis(c, data));

                // ANÁLISE OPERACIONAL
                column.Item().PageBreak();
                column.Item().Element(c => ComposeOperationalAnalysis(c, data));

                // CONCLUSÕES
                column.Item().PageBreak();
                column.Item().Element(c => ComposeConclusions(c, data));
            });
        }

        private void ComposeExecutiveSummary(IContainer container, ComprehensiveReportDto data)
        {
            container.Column(column =>
            {
                column.Item().Text("📊 RESUMO EXECUTIVO")
                    .FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);

                column.Item().PaddingTop(15).Row(row =>
                {
                    // KPI Cards
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Total de Vendas", $"{data.TotalSales:N2} Kz", Colors.Blue.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Receita Total", $"{data.TotalRevenue:N2} Kz", Colors.Green.Medium));
                });

                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Lucro Bruto", $"{data.GrossProfit:N2} Kz", Colors.Teal.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Margem de Lucro", $"{data.ProfitMargin:N2}%", Colors.Orange.Medium));
                });

                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Total de Faturas", data.TotalInvoices.ToString(), Colors.Purple.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Crescimento", $"{data.GrowthPercentage:+0.00;-0.00}%", 
                        data.GrowthPercentage >= 0 ? Colors.Green.Medium : Colors.Red.Medium));
                });
            });
        }

        private void ComposeKpiCard(IContainer container, string label, string value, string color)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2)
                .Padding(15).Column(column =>
                {
                    column.Item().Text(label).FontSize(9).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingTop(5).Text(value).FontSize(9).Bold().FontColor(color);
                });
        }

        private void ComposeSalesAnalysis(IContainer container, ComprehensiveReportDto data)
        {
            container.Column(column =>
            {
                column.Item().Text("📈 ANÁLISE DE VENDAS")
                    .FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);

                // Ticket Médio
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Text("Ticket Médio:").FontSize(9).SemiBold();
                    row.ConstantItem(150).AlignRight().Text($"{data.AverageTicket:N2} Kz").FontSize(9).FontColor(Colors.Blue.Medium);
                });

                // Top 10 Produtos
                if (data.TopProducts.Any())
                {
                    column.Item().PaddingTop(20).Text("Top 10 Produtos Mais Vendidos")
                        .FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);

                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Produto").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Quantidade").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Receita (Kz)").FontSize(10).SemiBold();
                        });

                        // Rows
                        foreach (var product in data.TopProducts.Take(10))
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(product.ProductName).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text(product.QuantitySold.ToString()).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text($"{product.Revenue:N2}").FontSize(9);
                        }
                    });
                }
            });
        }

        private void ComposeFinancialAnalysis(IContainer container, ComprehensiveReportDto data)
        {
            container.Column(column =>
            {
                column.Item().Text("💰 ANÁLISE FINANCEIRA")
                    .FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);

                // Métricas Financeiras
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Pendente", $"{data.TotalPendingAmount:N2} Kz", Colors.Orange.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Taxa de Inadimplência", $"{data.DefaultRate:N2}%", Colors.Red.Medium));
                });

                // Formas de Pagamento
                if (data.PaymentMethods.Any())
                {
                    column.Item().PaddingTop(20).Text("Formas de Pagamento")
                        .FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);

                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Método").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Transações").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Valor (Kz)").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("%").FontSize(10).SemiBold();
                        });

                        foreach (var method in data.PaymentMethods)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(method.MethodName).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text(method.TransactionCount.ToString()).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text($"{method.TotalValue:N2}").FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text($"{method.Percentage:N1}%").FontSize(9);
                        }
                    });
                }

                // Contas a Receber
                if (data.AccountsReceivable.Any())
                {
                    column.Item().PaddingTop(20).Text($"Contas a Receber ({data.AccountsReceivable.Count()} faturas)")
                        .FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);

                    var totalReceivable = data.AccountsReceivable.Sum(a => a.PendingValue);
                    column.Item().PaddingTop(5).Text($"Total a Receber: {totalReceivable:N2} Kz")
                        .FontSize(10).FontColor(Colors.Orange.Darken1);
                }
            });
        }

        private void ComposeCustomerAnalysis(IContainer container, ComprehensiveReportDto data)
        {
            container.Column(column =>
            {
                column.Item().Text("👥 ANÁLISE DE CLIENTES")
                    .FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);

                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Novos Clientes", data.NewCustomers.ToString(), Colors.Green.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Taxa de Retenção", $"{data.RetentionRate:N2}%", Colors.Blue.Medium));
                    row.ConstantItem(10);
                    row.RelativeItem().Element(c => ComposeKpiCard(c, "Ticket Médio/Cliente", $"{data.AverageTicketPerCustomer:N2} Kz", Colors.Purple.Medium));
                });

                // Top Clientes
                if (data.TopCustomers.Any())
                {
                    column.Item().PaddingTop(20).Text("Top 10 Clientes")
                        .FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);

                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Cliente").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Compras").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Total (Kz)").FontSize(10).SemiBold();
                        });

                        foreach (var customer in data.TopCustomers.Take(10))
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(customer.CustomerName).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text(customer.InvoiceCount.ToString()).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text($"{customer.TotalValue:N2}").FontSize(9);
                        }
                    });
                }
            });
        }

        private void ComposeStockAnalysis(IContainer container, ComprehensiveReportDto data)
        {
            container.Column(column =>
            {
                column.Item().Text("📦 ANÁLISE DE ESTOQUE")
                    .FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);

                column.Item().PaddingTop(15).Element(c => ComposeKpiCard(c, "Valor Total em Estoque", $"{data.TotalStockValue:N2} Kz", Colors.Teal.Medium));

                // Produtos com Estoque Baixo
                if (data.LowStockProducts.Any())
                {
                    column.Item().PaddingTop(20).Text($"⚠️ Produtos com Estoque Baixo ({data.LowStockProducts.Count()})")
                        .FontSize(9).SemiBold().FontColor(Colors.Orange.Darken1);

                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Orange.Lighten3).Padding(4).Text("Produto").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Orange.Lighten3).Padding(4).AlignRight().Text("Atual").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Orange.Lighten3).Padding(4).AlignRight().Text("Mínimo").FontSize(10).SemiBold();
                        });

                        foreach (var product in data.LowStockProducts.Take(15))
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(product.ProductName).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text(product.CurrentQuantity.ToString()).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text(product.MinimumQuantity.ToString()).FontSize(9);
                        }
                    });
                }

                // Produtos Vencendo
                if (data.ExpiringProducts.Any())
                {
                    column.Item().PaddingTop(20).Text($"⏰ Produtos Próximos ao Vencimento ({data.ExpiringProducts.Count()})")
                        .FontSize(9).SemiBold().FontColor(Colors.Red.Darken1);

                    column.Item().PaddingTop(5).Text("Atenção para produtos que vencem nos próximos 30 dias")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                }
            });
        }

        private void ComposeOperationalAnalysis(IContainer container, ComprehensiveReportDto data)
        {
            container.Column(column =>
            {
                column.Item().Text("⚙️ ANÁLISE OPERACIONAL")
                    .FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);

                // Status de Faturas
                if (data.InvoicesByStatus.Any())
                {
                    column.Item().PaddingTop(15).Text("Distribuição de Faturas por Status")
                        .FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);

                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Status").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Quantidade").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Valor (Kz)").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("%").FontSize(10).SemiBold();
                        });

                        foreach (var status in data.InvoicesByStatus)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(status.Status).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text(status.Count.ToString()).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text($"{status.TotalValue:N2}").FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text($"{status.Percentage:N1}%").FontSize(9);
                        }
                    });
                }

                // Taxa de Cancelamento
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem().Text("Taxa de Cancelamento:").FontSize(9).SemiBold();
                    row.ConstantItem(100).AlignRight().Text($"{data.CancellationRate:N2}%")
                        .FontSize(9).FontColor(data.CancellationRate > 5 ? Colors.Red.Medium : Colors.Green.Medium);
                });

                // Desempenho por Vendedor
                if (data.UserPerformance.Any())
                {
                    column.Item().PaddingTop(20).Text("Desempenho por Vendedor")
                        .FontSize(9).SemiBold().FontColor(Colors.Grey.Darken2);

                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).Text("Vendedor").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Vendas").FontSize(10).SemiBold();
                            header.Cell().Background(Colors.Blue.Lighten3).Padding(4).AlignRight().Text("Total (Kz)").FontSize(10).SemiBold();
                        });

                        foreach (var user in data.UserPerformance)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .Text(user.UserName).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text(user.InvoiceCount.ToString()).FontSize(9);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4)
                                .AlignRight().Text($"{user.TotalValue:N2}").FontSize(9);
                        }
                    });
                }
            });
        }

        private void ComposeConclusions(IContainer container, ComprehensiveReportDto data)
        {
            container.Column(column =>
            {
                column.Item().Text("📋 CONCLUSÕES E RECOMENDAÇÕES")
                    .FontSize(9).Bold().FontColor(Colors.Blue.Darken2);
                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Lighten2);

                // Pontos Fortes
                column.Item().PaddingTop(15).Text("✅ Pontos Fortes")
                    .FontSize(9).SemiBold().FontColor(Colors.Green.Darken1);

                column.Item().PaddingTop(8).PaddingLeft(15).Column(col =>
                {
                    if (data.GrowthPercentage > 0)
                        col.Item().Text($"• Crescimento positivo de {data.GrowthPercentage:N2}% no período").FontSize(10);
                    
                    if (data.ProfitMargin > 20)
                        col.Item().Text($"• Margem de lucro saudável de {data.ProfitMargin:N2}%").FontSize(10);
                    
                    if (data.DefaultRate < 5)
                        col.Item().Text($"• Baixa taxa de inadimplência ({data.DefaultRate:N2}%)").FontSize(10);
                });

                // Áreas de Melhoria
                column.Item().PaddingTop(15).Text("⚠️ Áreas de Melhoria")
                    .FontSize(9).SemiBold().FontColor(Colors.Orange.Darken1);

                column.Item().PaddingTop(8).PaddingLeft(15).Column(col =>
                {
                    if (data.LowStockProducts.Any())
                        col.Item().Text($"• {data.LowStockProducts.Count()} produtos com estoque baixo requerem reposição").FontSize(10);
                    
                    if (data.ExpiringProducts.Any())
                        col.Item().Text($"• {data.ExpiringProducts.Count()} produtos próximos ao vencimento necessitam atenção").FontSize(10);
                    
                    if (data.TotalPendingAmount > 0)
                        col.Item().Text($"• {data.TotalPendingAmount:N2} Kz em contas a receber pendentes").FontSize(10);
                });

                // Ações Recomendadas
                column.Item().PaddingTop(15).Text("🎯 Ações Recomendadas")
                    .FontSize(9).SemiBold().FontColor(Colors.Blue.Darken1);

                column.Item().PaddingTop(8).PaddingLeft(15).Column(col =>
                {
                    col.Item().Text("• Implementar estratégias de fidelização de clientes").FontSize(10);
                    col.Item().Text("• Otimizar gestão de estoque para evitar rupturas").FontSize(10);
                    col.Item().Text("• Intensificar cobranças de contas pendentes").FontSize(10);
                    col.Item().Text("• Promover produtos com baixa rotatividade").FontSize(10);
                });
            });
        }

        private void ComposeSimpleFooter(IContainer container, CompanyConfigDto company)
        {
            container.AlignCenter().Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                column.Item().PaddingTop(10).Text(text =>
                {
                    text.Span("Relatório gerado em: ").FontSize(8).FontColor(Colors.Grey.Medium);
                    text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(8).FontColor(Colors.Grey.Medium);
                });
                
                if (!string.IsNullOrWhiteSpace(company.CompanyName))
                {
                    column.Item().Text($"{company.CompanyName} - Sistema VendaFlex")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                }
            });
        }

        #endregion
    }
}
