using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;

namespace VendaFlex.Infrastructure.Services
{
    /// <summary>
    /// Serviço profissional de impressão de recibos/faturas usando QuestPDF
    /// Suporta formato A4 e Rolo térmico (80mm)
    /// </summary>
    public class ReceiptPrintService : IReceiptPrintService
    {
        private readonly IProductService _productService;
        private readonly IPersonService _personService;

        public ReceiptPrintService(IProductService productService, IPersonService personService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _personService = personService ?? throw new ArgumentNullException(nameof(personService));

            // Configurar licença do QuestPDF (Community - gratuita)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task PrintAsync(CompanyConfigDto cfg, InvoiceDto invoice, IEnumerable<InvoiceProductDto> items, string format)
        {
            try
            {
                // Carregar dados dos produtos
                var productMap = await LoadProductMapAsync(items);

                // Carregar dados do cliente (se configurado)
                PersonDto? customer = null;
                if (cfg.IncludeCustomerData && invoice.PersonId > 0)
                {
                    var customerResult = await _personService.GetByIdAsync(invoice.PersonId);
                    if (customerResult.Success && customerResult.Data != null)
                    {
                        customer = customerResult.Data;
                    }
                }

                // Criar arquivo temporário para o PDF
                var tempPdfPath = Path.Combine(Path.GetTempPath(), $"VendaFlex_Invoice_{invoice.InvoiceNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

                // Gerar PDF baseado no formato
                var formatType = ParseFormat(format);
                await GeneratePdfAsync(cfg, invoice, items, productMap, customer, formatType, tempPdfPath);

                // Abrir o PDF gerado para impressão
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        // Abrir o PDF com o visualizador padrão do sistema
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = tempPdfPath,
                            UseShellExecute = true,
                            Verb = "print"
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Erro ao abrir o documento para impressão: {ex.Message}\n\nO arquivo foi salvo em: {tempPdfPath}",
                            "Erro de Impressão",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao preparar documento para impressão: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                throw;
            }
        }

        private async Task<Dictionary<int, ProductDto>> LoadProductMapAsync(IEnumerable<InvoiceProductDto> items)
        {
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
            return productMap;
        }

        private static CompanyConfig.InvoiceFormatType ParseFormat(string format)
        {
            if (Enum.TryParse<CompanyConfig.InvoiceFormatType>(format, out var ft))
                return ft;
            return CompanyConfig.InvoiceFormatType.A4;
        }

        private async Task GeneratePdfAsync(
            CompanyConfigDto cfg,
            InvoiceDto invoice,
            IEnumerable<InvoiceProductDto> items,
            Dictionary<int, ProductDto> productMap,
            PersonDto? customer,
            CompanyConfig.InvoiceFormatType format,
            string filePath)
        {
            await Task.Run(() =>
            {
                if (format == CompanyConfig.InvoiceFormatType.Rolo)
                {
                    GenerateRollReceipt(cfg, invoice, items, productMap, customer, filePath);
                }
                else
                {
                    GenerateA4Invoice(cfg, invoice, items, productMap, customer, filePath);
                }
            });
        }

        #region A4 Format - Professional Invoice

        private void GenerateA4Invoice(
            CompanyConfigDto cfg,
            InvoiceDto invoice,
            IEnumerable<InvoiceProductDto> items,
            Dictionary<int, ProductDto> productMap,
            PersonDto? customer,
            string filePath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Segoe UI"));

                    page.Header().Element(c => ComposeA4Header(c, cfg, invoice));
                    page.Content().Element(c => ComposeA4Content(c, cfg, invoice, items, productMap, customer));
                    page.Footer().Element(c => ComposeA4Footer(c, cfg, invoice));
                });
            }).GeneratePdf(filePath);
        }

        private void ComposeA4Header(IContainer container, CompanyConfigDto cfg, InvoiceDto invoice)
        {
            container.Column(column =>
            {
                // Barra superior colorida
                column.Item().Height(8).Background(Colors.Blue.Darken2);

                column.Item().PaddingVertical(15).Row(row =>
                {
                    // Coluna esquerda - Logo e informações da empresa
                    row.RelativeItem().Column(leftColumn =>
                    {
                        // Logo
                        if (!string.IsNullOrWhiteSpace(cfg.LogoUrl) && File.Exists(cfg.LogoUrl))
                        {
                            leftColumn.Item().MaxHeight(70).MaxWidth(200).Image(cfg.LogoUrl);
                            leftColumn.Item().PaddingBottom(10);
                        }

                        // Nome da empresa
                        leftColumn.Item().Text(cfg.CompanyName)
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.Blue.Darken3);

                        // Informações da empresa
                        leftColumn.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span($"NIF: {cfg.TaxId}").FontSize(9).SemiBold();
                            text.EmptyLine();

                            if (!string.IsNullOrWhiteSpace(cfg.Address))
                            {
                                text.Span(cfg.Address).FontSize(8);
                                text.EmptyLine();
                            }

                            var cityLine = string.Empty;
                            if (!string.IsNullOrWhiteSpace(cfg.City))
                            {
                                cityLine = cfg.City;
                                if (!string.IsNullOrWhiteSpace(cfg.PostalCode))
                                    cityLine += $" - {cfg.PostalCode}";
                            }
                            if (!string.IsNullOrEmpty(cityLine))
                            {
                                text.Span(cityLine).FontSize(8);
                                text.EmptyLine();
                            }

                            if (!string.IsNullOrWhiteSpace(cfg.PhoneNumber))
                            {
                                text.Span("Tel: ").FontSize(8);
                                text.Span(cfg.PhoneNumber).FontSize(8).SemiBold();
                                text.Span("  ");
                            }

                            if (!string.IsNullOrWhiteSpace(cfg.Email))
                            {
                                text.Span("Email: ").FontSize(8);
                                text.Span(cfg.Email).FontSize(8).SemiBold();
                            }
                        });
                    });

                    // Coluna direita - Informações da fatura
                    row.ConstantItem(220).Column(rightColumn =>
                    {
                        rightColumn.Item()
                            .Background(Colors.Blue.Darken3)
                            .Padding(15)
                            .Column(invColumn =>
                            {
                                invColumn.Item().Text("FATURA")
                                    .FontSize(24)
                                    .Bold()
                                    .FontColor(Colors.White);

                                invColumn.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.White);

                                invColumn.Item().PaddingTop(10).Text(text =>
                                {
                                    text.Span("Nº: ").FontSize(10).FontColor(Colors.Grey.Lighten2);
                                    text.Span(invoice.InvoiceNumber).FontSize(12).Bold().FontColor(Colors.White);
                                    text.EmptyLine();

                                    text.Span("Data: ").FontSize(9).FontColor(Colors.Grey.Lighten2);
                                    text.Span(invoice.Date.ToString("dd/MM/yyyy")).FontSize(10).FontColor(Colors.White);
                                    text.EmptyLine();

                                    text.Span("Hora: ").FontSize(9).FontColor(Colors.Grey.Lighten2);
                                    text.Span(invoice.Date.ToString("HH:mm")).FontSize(10).FontColor(Colors.White);

                                    if (invoice.DueDate.HasValue)
                                    {
                                        text.EmptyLine();
                                        text.Span("Vencimento: ").FontSize(9).FontColor(Colors.Grey.Lighten2);
                                        text.Span(invoice.DueDate.Value.ToString("dd/MM/yyyy")).FontSize(10).FontColor(Colors.White);
                                    }
                                });

                                invColumn.Item().PaddingTop(10)
                                    .Background(GetStatusBackgroundColor(invoice.Status))
                                    .Padding(8)
                                    .Text(GetStatusText(invoice.Status))
                                    .FontSize(10)
                                    .Bold()
                                    .FontColor(GetStatusTextColor(invoice.Status))
                                    .AlignCenter();
                            });
                    });
                });

                // Linha separadora
                column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
            });
        }

        private void ComposeA4Content(
            IContainer container,
            CompanyConfigDto cfg,
            InvoiceDto invoice,
            IEnumerable<InvoiceProductDto> items,
            Dictionary<int, ProductDto> productMap,
            PersonDto? customer)
        {
            container.PaddingTop(20).Column(column =>
            {
                // Informações do cliente
                if (customer != null && cfg.IncludeCustomerData)
                {
                    column.Item().Element(c => ComposeA4CustomerInfo(c, customer));
                    column.Item().PaddingBottom(20);
                }

                // Tabela de itens
                column.Item().Element(c => ComposeA4ItemsTable(c, items, productMap, cfg.CurrencySymbol));

                // Totais e resumo
                column.Item().PaddingTop(20).Row(row =>
                {
                    // Observações (se houver)
                    if (!string.IsNullOrWhiteSpace(invoice.Notes))
                    {
                        row.RelativeItem().PaddingRight(20).Element(c => ComposeA4Notes(c, invoice.Notes));
                    }
                    else
                    {
                        row.RelativeItem();
                    }

                    // Totais
                    row.ConstantItem(250).Element(c => ComposeA4Totals(c, invoice, cfg.CurrencySymbol));
                });
            });
        }

        private void ComposeA4CustomerInfo(IContainer container, PersonDto customer)
        {
            container.Background(Colors.Grey.Lighten4).Padding(15).Row(row =>
            {
                row.AutoItem().PaddingRight(10)
                    .Width(40).Height(40)
                    .Background(Colors.Blue.Darken2)
                    .AlignCenter().AlignMiddle()
                    .Text(GetInitials(customer.Name))
                    .FontSize(16)
                    .Bold()
                    .FontColor(Colors.White);

                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("CLIENTE").FontSize(10).Bold().FontColor(Colors.Blue.Darken2);
                    
                    column.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span(customer.Name).FontSize(11).Bold();
                        text.EmptyLine();

                        if (!string.IsNullOrWhiteSpace(customer.TaxId))
                        {
                            text.Span("NIF: ").FontSize(9);
                            text.Span(customer.TaxId).FontSize(9).SemiBold();
                            text.Span("  ");
                        }

                        if (!string.IsNullOrWhiteSpace(customer.PhoneNumber))
                        {
                            text.Span("Tel: ").FontSize(9);
                            text.Span(customer.PhoneNumber).FontSize(9).SemiBold();
                        }

                        if (!string.IsNullOrWhiteSpace(customer.Email))
                        {
                            text.EmptyLine();
                            text.Span("Email: ").FontSize(9);
                            text.Span(customer.Email).FontSize(9).SemiBold();
                        }

                        if (!string.IsNullOrWhiteSpace(customer.Address))
                        {
                            text.EmptyLine();
                            var fullAddress = customer.Address;
                            if (!string.IsNullOrWhiteSpace(customer.City))
                                fullAddress += $", {customer.City}";
                            if (!string.IsNullOrWhiteSpace(customer.State))
                                fullAddress += $" - {customer.State}";
                            if (!string.IsNullOrWhiteSpace(customer.PostalCode))
                                fullAddress += $", {customer.PostalCode}";
                            
                            text.Span(fullAddress).FontSize(9).Italic();
                        }
                    });
                });
            });
        }

        private void ComposeA4ItemsTable(
            IContainer container,
            IEnumerable<InvoiceProductDto> items,
            Dictionary<int, ProductDto> productMap,
            string currencySymbol)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);   // #
                    columns.RelativeColumn(4);     // Descrição
                    columns.ConstantColumn(60);    // Qtd
                    columns.ConstantColumn(80);    // Preço Unit.
                    columns.ConstantColumn(60);    // Desc%
                    columns.ConstantColumn(90);    // Total
                });

                // Cabeçalho
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("#").FontSize(10).Bold();
                    header.Cell().Element(HeaderCellStyle).Text("DESCRIÇÃO").FontSize(10).Bold();
                    header.Cell().Element(HeaderCellStyle).AlignCenter().Text("QTD").FontSize(10).Bold();
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("PREÇO UNIT.").FontSize(10).Bold();
                    header.Cell().Element(HeaderCellStyle).AlignCenter().Text("DESC%").FontSize(10).Bold();
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("TOTAL").FontSize(10).Bold();

                    static IContainer HeaderCellStyle(IContainer c) =>
                        c.Background(Colors.Blue.Darken2)
                         .Padding(8)
                         .BorderBottom(1)
                         .BorderColor(Colors.Grey.Darken1);
                });

                // Corpo da tabela
                int index = 1;
                foreach (var item in items)
                {
                    var productName = productMap.TryGetValue(item.ProductId, out var p) 
                        ? p.Name 
                        : $"Produto {item.ProductId}";
                    
                    var productCode = productMap.TryGetValue(item.ProductId, out var prod)
                        ? (prod.Code ?? prod.SKU ?? "")
                        : "";

                    var lineTotal = item.UnitPrice * item.Quantity;
                    var discountAmount = lineTotal * (item.DiscountPercentage / 100m);
                    var finalTotal = lineTotal - discountAmount;

                    var isEvenRow = index % 2 == 0;

                    table.Cell().Element(c => RowCellStyle(c, isEvenRow))
                        .Text($"{index}").FontSize(9);

                    table.Cell().Element(c => RowCellStyle(c, isEvenRow))
                        .Column(col =>
                        {
                            col.Item().Text(productName).FontSize(10).SemiBold();
                            if (!string.IsNullOrWhiteSpace(productCode))
                                col.Item().Text($"Cód: {productCode}").FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
                        });

                    table.Cell().Element(c => RowCellStyle(c, isEvenRow))
                        .AlignCenter().Text($"{item.Quantity:N2}").FontSize(9);

                    table.Cell().Element(c => RowCellStyle(c, isEvenRow))
                        .AlignRight().Text($"{currencySymbol} {item.UnitPrice:N2}".Replace('.', ',')).FontSize(9);

                    table.Cell().Element(c => RowCellStyle(c, isEvenRow))
                        .AlignCenter().Text(item.DiscountPercentage > 0 ? $"{item.DiscountPercentage:N1}%" : "-")
                        .FontSize(9)
                        .FontColor(item.DiscountPercentage > 0 ? Colors.Red.Medium : Colors.Grey.Darken1);

                    table.Cell().Element(c => RowCellStyle(c, isEvenRow))
                        .AlignRight().Text($"{currencySymbol} {finalTotal:N2}".Replace('.', ',')).FontSize(10).Bold();

                    index++;
                }

                static IContainer RowCellStyle(IContainer c, bool isEven) =>
                    c.Background(isEven ? Colors.Grey.Lighten5 : Colors.White)
                     .Padding(8)
                     .BorderBottom(1)
                     .BorderColor(Colors.Grey.Lighten2);
            });
        }

        private void ComposeA4Totals(IContainer container, InvoiceDto invoice, string currencySymbol)
        {
            container.Column(column =>
            {
                column.Spacing(5);

                // Subtotal
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Subtotal:").FontSize(11);
                    row.AutoItem().Text($"{currencySymbol} {invoice.SubTotal:N2}".Replace('.', ',')).FontSize(11);
                });

                // Desconto
                if (invoice.DiscountAmount > 0)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Desconto:").FontSize(11).FontColor(Colors.Red.Medium);
                        row.AutoItem().Text($"- {currencySymbol} {invoice.DiscountAmount:N2}".Replace('.', ','))
                            .FontSize(11).FontColor(Colors.Red.Medium);
                    });
                }

                // Frete
                if (invoice.ShippingCost > 0)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Frete:").FontSize(11);
                        row.AutoItem().Text($"{currencySymbol} {invoice.ShippingCost:N2}".Replace('.', ',')).FontSize(11);
                    });
                }

                // Imposto
                if (invoice.TaxAmount > 0)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Imposto:").FontSize(11);
                        row.AutoItem().Text($"{currencySymbol} {invoice.TaxAmount:N2}".Replace('.', ',')).FontSize(11);
                    });
                }

                // Linha separadora
                column.Item().PaddingVertical(8).LineHorizontal(2).LineColor(Colors.Blue.Darken2);

                // Total
                column.Item()
                    .Background(Colors.Blue.Darken3)
                    .Padding(12)
                    .Row(row =>
                    {
                        row.RelativeItem().Text("TOTAL:").FontSize(14).Bold().FontColor(Colors.White);
                        row.AutoItem().Text($"{currencySymbol} {invoice.Total:N2}".Replace('.', ','))
                            .FontSize(16).Bold().FontColor(Colors.White);
                    });

                // Valor pago e troco
                if (invoice.PaidAmount > 0)
                {
                    column.Item().PaddingTop(10);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Valor Pago:").FontSize(11).FontColor(Colors.Green.Darken1);
                        row.AutoItem().Text($"{currencySymbol} {invoice.PaidAmount:N2}".Replace('.', ','))
                            .FontSize(11).Bold().FontColor(Colors.Green.Darken1);
                    });

                    var balance = invoice.Total - invoice.PaidAmount;
                    if (balance > 0)
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Saldo Restante:").FontSize(11).FontColor(Colors.Orange.Darken1);
                            row.AutoItem().Text(text =>
                            {
                                text.Span($"{currencySymbol} {balance:N2}").Bold();
                            });
                        });
                    }
                    else if (balance < 0)
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Troco:").FontSize(11).FontColor(Colors.Blue.Medium);
                            row.AutoItem().Text($"{currencySymbol} {Math.Abs(balance):N2}".Replace('.', ','))
                                .FontSize(11).Bold().FontColor(Colors.Blue.Medium);
                        });
                    }
                }
            });
        }

        private void ComposeA4Notes(IContainer container, string notes)
        {
            container.Border(1).BorderColor(Colors.Orange.Lighten2)
                .Background(Colors.Yellow.Lighten5)
                .Padding(12)
                .Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.AutoItem().PaddingRight(8)
                            .Text("💡").FontSize(14);
                        row.RelativeItem()
                            .Text("OBSERVAÇÕES").FontSize(11).Bold().FontColor(Colors.Orange.Darken2);
                    });
                    
                    column.Item().PaddingTop(8).Text(notes).FontSize(9).LineHeight(1.3f);
                });
        }

        private void ComposeA4Footer(IContainer container, CompanyConfigDto cfg, InvoiceDto invoice)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);

                column.Item().PaddingTop(15).AlignCenter().Row(row =>
                {
                    // Rodapé personalizado
                    if (!string.IsNullOrWhiteSpace(cfg.InvoiceFooterText))
                    {
                        row.RelativeItem().Text(cfg.InvoiceFooterText)
                            .FontSize(9)
                            .Italic()
                            .FontColor(Colors.Grey.Darken1);
                    }
                });

                column.Item().PaddingTop(10).AlignCenter().Text(text =>
                {
                    text.Span("Documento gerado em: ").FontSize(7).FontColor(Colors.Grey.Medium);
                    text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")).FontSize(7).FontColor(Colors.Grey.Darken1);
                    text.Span("  |  ").FontSize(7).FontColor(Colors.Grey.Medium);
                    text.Span("VendaFlex - Sistema de Gestão").FontSize(7).FontColor(Colors.Grey.Medium);
                });
            });
        }

        #endregion

        #region Roll Format - Thermal Printer (80mm)

        private void GenerateRollReceipt(
            CompanyConfigDto cfg,
            InvoiceDto invoice,
            IEnumerable<InvoiceProductDto> items,
            Dictionary<int, ProductDto> productMap,
            PersonDto? customer,
            string filePath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    // 80mm width
                    page.Size(new PageSize(302, 1000));
                    page.MarginVertical(10);
                    page.MarginHorizontal(8);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Courier New"));

                    page.Content().Column(column =>
                    {
                        ComposeRollHeader(column, cfg, invoice);
                        ComposeRollCustomer(column, customer);
                        ComposeRollItems(column, items, productMap, cfg.CurrencySymbol);
                        ComposeRollTotals(column, invoice, cfg.CurrencySymbol);
                        ComposeRollFooter(column, cfg);
                    });
                });
            }).GeneratePdf(filePath);
        }

        private void ComposeRollHeader(ColumnDescriptor column, CompanyConfigDto cfg, InvoiceDto invoice)
        {
            // Logo (se houver e couber)
            if (!string.IsNullOrWhiteSpace(cfg.LogoUrl) && File.Exists(cfg.LogoUrl))
            {
                column.Item().AlignCenter().MaxHeight(60).Image(cfg.LogoUrl);
                column.Item().PaddingBottom(5);
            }

            // Nome da empresa
            column.Item().AlignCenter().Text(cfg.CompanyName)
                .FontSize(12)
                .Bold();

            // NIF
            column.Item().AlignCenter().Text($"NIF: {cfg.TaxId}")
                .FontSize(8);

            // Telefone
            if (!string.IsNullOrWhiteSpace(cfg.PhoneNumber))
            {
                column.Item().AlignCenter().Text($"Tel: {cfg.PhoneNumber}")
                    .FontSize(8);
            }

            // Separador
            column.Item().PaddingVertical(8).Text(new string('=', 40))
                .FontSize(8);

            // Informações da fatura
            column.Item().Text(text =>
            {
                text.Span("FATURA: ").Bold();
                text.Span(invoice.InvoiceNumber);
            });

            column.Item().Text($"Data: {invoice.Date:dd/MM/yyyy HH:mm}");
            
            if (invoice.DueDate.HasValue)
            {
                column.Item().Text($"Vencimento: {invoice.DueDate.Value:dd/MM/yyyy}");
            }

            column.Item().Text(text =>
            {
                text.Span("Status: ").Bold();
                text.Span(GetStatusText(invoice.Status));
            });

            column.Item().PaddingVertical(5).Text(new string('-', 40))
                .FontSize(8);
        }

        private void ComposeRollCustomer(ColumnDescriptor column, PersonDto? customer)
        {
            if (customer == null) return;

            column.Item().Text("CLIENTE").Bold().FontSize(10);
            column.Item().Text(customer.Name).FontSize(9);
            
            if (!string.IsNullOrWhiteSpace(customer.TaxId))
                column.Item().Text($"NIF: {customer.TaxId}").FontSize(8);
            
            if (!string.IsNullOrWhiteSpace(customer.PhoneNumber))
                column.Item().Text($"Tel: {customer.PhoneNumber}").FontSize(8);

            column.Item().PaddingVertical(5).Text(new string('-', 40))
                .FontSize(8);
        }

        private void ComposeRollItems(
            ColumnDescriptor column,
            IEnumerable<InvoiceProductDto> items,
            Dictionary<int, ProductDto> productMap,
            string currencySymbol)
        {
            // Cabeçalho da tabela
            column.Item().Text("QTD  PRODUTO              TOTAL")
                .FontSize(9)
                .Bold();

            column.Item().Text(new string('-', 40)).FontSize(8);

            foreach (var item in items)
            {
                var name = productMap.TryGetValue(item.ProductId, out var p) 
                    ? p.Name 
                    : $"Produto {item.ProductId}";

                var lineTotal = item.UnitPrice * item.Quantity;
                var discountAmount = lineTotal * (item.DiscountPercentage / 100m);
                var finalTotal = lineTotal - discountAmount;

                // Nome do produto (quebrado se necessário)
                var truncatedName = name.Length > 20 ? name.Substring(0, 20) : name.PadRight(20);
                
                column.Item().Text($"{item.Quantity,3:N0}  {truncatedName}  {finalTotal,8:N2}")
                    .FontSize(8);

                // Linha de detalhes
                column.Item().Text($"     {currencySymbol} {item.UnitPrice:N2} x {item.Quantity:N2}")
                    .FontSize(7)
                    .Italic();

                if (item.DiscountPercentage > 0)
                {
                    column.Item().Text($"     Desc: {item.DiscountPercentage:N1}% (-{currencySymbol} {discountAmount:N2})")
                        .FontSize(7);
                }
            }

            column.Item().PaddingVertical(5).Text(new string('=', 40))
                .FontSize(8);
        }

        private void ComposeRollTotals(ColumnDescriptor column, InvoiceDto invoice, string currencySymbol)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("Subtotal:");
                row.AutoItem().Text($"{currencySymbol} {invoice.SubTotal:N2}");
            });

            if (invoice.DiscountAmount > 0)
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Desconto:");
                    row.AutoItem().Text($"-{currencySymbol} {invoice.DiscountAmount:N2}");
                });
            }

            if (invoice.TaxAmount > 0)
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Imposto:");
                    row.AutoItem().Text($"{currencySymbol} {invoice.TaxAmount:N2}");
                });
            }

            column.Item().PaddingVertical(5).Text(new string('=', 40))
                .FontSize(8);

            column.Item().Row(row =>
            {
                row.RelativeItem().Text("TOTAL:").Bold().FontSize(11);
                row.AutoItem().Text($"{currencySymbol} {invoice.Total:N2}").Bold().FontSize(12);
            });

            if (invoice.PaidAmount > 0)
            {
                column.Item().PaddingTop(5);
                
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text("Pago:");
                    row.AutoItem().Text($"{currencySymbol} {invoice.PaidAmount:N2}").Bold();
                });

                var balance = invoice.Total - invoice.PaidAmount;
                if (balance < 0)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Troco:");
                        row.AutoItem().Text($"{currencySymbol} {Math.Abs(balance):N2}").Bold();
                    });
                }
                else if (balance > 0)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Saldo:");
                        row.AutoItem().Text(text =>
                        {
                            text.Span($"{currencySymbol} {balance:N2}").Bold();
                        });
                    });
                }
            }

            column.Item().PaddingVertical(5).Text(new string('=', 40))
                .FontSize(8);
        }

        private void ComposeRollFooter(ColumnDescriptor column, CompanyConfigDto cfg)
        {
            if (!string.IsNullOrWhiteSpace(cfg.InvoiceFooterText))
            {
                column.Item().PaddingTop(10).AlignCenter()
                    .Text(cfg.InvoiceFooterText)
                    .FontSize(7)
                    .Italic();
            }

            column.Item().PaddingTop(10).AlignCenter()
                .Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                .FontSize(7);

            column.Item().AlignCenter()
                .Text("VendaFlex - Sistema de Gestão")
                .FontSize(6);

            // Espaço para corte
            column.Item().PaddingTop(20);
        }

        #endregion

        #region Helper Methods

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            return $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
        }

        private string GetStatusText(InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Draft => "Rascunho",
                InvoiceStatus.Pending => "Pendente",
                InvoiceStatus.Confirmed => "Confirmada",
                InvoiceStatus.Paid => "Paga",
                InvoiceStatus.Cancelled => "Cancelada",
                _ => "Desconhecido"
            };
        }

        private string GetStatusBackgroundColor(InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Draft => Colors.Grey.Lighten3,
                InvoiceStatus.Pending => Colors.Orange.Lighten3,
                InvoiceStatus.Confirmed => Colors.Blue.Lighten3,
                InvoiceStatus.Paid => Colors.Green.Lighten3,
                InvoiceStatus.Cancelled => Colors.Red.Lighten3,
                _ => Colors.Grey.Lighten3
            };
        }

        private string GetStatusTextColor(InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.Draft => Colors.Grey.Darken3,
                InvoiceStatus.Pending => Colors.Orange.Darken3,
                InvoiceStatus.Confirmed => Colors.Blue.Darken3,
                InvoiceStatus.Paid => Colors.Green.Darken3,
                InvoiceStatus.Cancelled => Colors.Red.Darken3,
                _ => Colors.Grey.Darken3
            };
        }

        #endregion
    }
}
