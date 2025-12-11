using ClosedXML.Excel;
using VendaFlex.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VendaFlex.Infrastructure.Interfaces;
using VendaFlex.Data;
using Microsoft.EntityFrameworkCore;
using VendaFlex.Core.Utils;

namespace VendaFlex.Infrastructure.Services
{
    public class ProductExcelService : IProductExcelService
    {
        private readonly ApplicationDbContext _context;

        public ProductExcelService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Exporta produtos para Excel com formatação profissional
        /// </summary>
        public async Task<byte[]> ExportProductsToExcelAsync(IEnumerable<Product> products)
        {
            using var workbook = new XLWorkbook();
            
            // Sheet 1: Dados dos Produtos
            var worksheet = workbook.Worksheets.Add("Produtos");
            
            // Configurar cabeçalhos
            CreateHeaders(worksheet);
            
            // Preencher dados
            int row = 2;
            foreach (var product in products)
            {
                FillProductRow(worksheet, row, product);
                row++;
            }
            
            // Aplicar formatação
            ApplyFormatting(worksheet, row - 1);
            
            // Sheet 2: Instruções de Importação
            CreateInstructionsSheet(workbook);
            
            // Sheet 3: Lista de Categorias (para referência)
            await CreateCategoriesReferenceSheet(workbook);
            
            // Sheet 4: Lista de Fornecedores (para referência)
            await CreateSuppliersReferenceSheet(workbook);
            
            // Converter para bytes
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Cria os cabeçalhos da planilha
        /// </summary>
        public void CreateHeaders(IXLWorksheet worksheet)
        {
            var headers = new[]
            {
                ("A1", "ID*", "Deixe em branco para novos produtos"),
                ("B1", "Código Interno", "Gerado automaticamente se vazio"),
                ("C1", "Código Externo", "Gerado automaticamente se vazio"),
                ("D1", "Nome*", "Nome do produto (obrigatório)"),
                ("E1", "Descrição", "Descrição detalhada"),
                ("F1", "Descrição Curta", "Descrição resumida"),
                ("G1", "Código de Barras", "EAN/UPC (opcional, mas deve ser único)"),
                ("H1", "SKU", "Gerado automaticamente se vazio"),
                ("I1", "Peso", "Ex: 1.5kg"),
                ("J1", "Dimensões", "Ex: 10x20x30cm"),
                ("K1", "ID Categoria*", "ID da categoria (consulte aba Categorias)"),
                ("L1", "Nome Categoria", "Apenas para referência na exportação"),
                ("M1", "ID Fornecedor*", "ID do fornecedor (consulte aba Fornecedores)"),
                ("N1", "Nome Fornecedor", "Apenas para referência na exportação"),
                ("O1", "Preço Custo*", "Valor de custo (decimal)"),
                ("P1", "Preço Venda*", "Valor de venda (decimal)"),
                ("Q1", "% Desconto", "Percentual 0-100"),
                ("R1", "% Taxa/Imposto", "Percentual 0-100"),
                ("S1", "URL Foto", "Link da imagem do produto"),
                ("T1", "Status*", "Active, Inactive, Discontinued, OutOfStock"),
                ("U1", "Destaque", "TRUE ou FALSE"),
                ("V1", "Permite Backorder", "TRUE ou FALSE"),
                ("W1", "Ordem Exibição", "Número inteiro"),
                ("X1", "Controla Estoque", "TRUE ou FALSE"),
                ("Y1", "Estoque Atual", "Quantidade em estoque"),
                ("Z1", "Estoque Mínimo", "Alerta de estoque baixo"),
                ("AA1", "Estoque Máximo", "Limite máximo"),
                ("AB1", "Ponto Reposição", "Quando solicitar reposição"),
                ("AC1", "Tem Validade", "TRUE ou FALSE"),
                ("AD1", "Dias Validade", "Dias até vencer"),
                ("AE1", "Dias Aviso Validade", "Dias antes de alertar")
            };

            foreach (var (cell, header, comment) in headers)
            {
                var xlCell = worksheet.Cell(cell);
                xlCell.Value = header;
                xlCell.CreateComment().AddText(comment);
                xlCell.Style.Font.Bold = true;
                xlCell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                xlCell.Style.Font.FontColor = XLColor.DarkBlue;
            }
        }

        /// <summary>
        /// Preenche uma linha com dados do produto
        /// </summary>
        public void FillProductRow(IXLWorksheet worksheet, int row, Product product)
        {
            worksheet.Cell(row, 1).Value = product.ProductId;
            worksheet.Cell(row, 2).Value = product.InternalCode;
            worksheet.Cell(row, 3).Value = product.ExternalCode;
            worksheet.Cell(row, 4).Value = product.Name;
            worksheet.Cell(row, 5).Value = product.Description;
            worksheet.Cell(row, 6).Value = product.ShortDescription;
            worksheet.Cell(row, 7).Value = product.Barcode;
            worksheet.Cell(row, 8).Value = product.SKU;
            worksheet.Cell(row, 9).Value = product.Weight;
            worksheet.Cell(row, 10).Value = product.Dimensions;
            worksheet.Cell(row, 11).Value = product.CategoryId;
            worksheet.Cell(row, 12).Value = product.Category?.Name ?? "";
            worksheet.Cell(row, 13).Value = product.SupplierId;
            worksheet.Cell(row, 14).Value = product.Supplier?.Name ?? "";
            worksheet.Cell(row, 15).Value = product.CostPrice;
            worksheet.Cell(row, 16).Value = product.SalePrice;
            worksheet.Cell(row, 17).Value = product.DiscountPercentage;
            worksheet.Cell(row, 18).Value = product.TaxRate;
            worksheet.Cell(row, 19).Value = product.PhotoUrl;
            worksheet.Cell(row, 20).Value = product.Status.ToString();
            worksheet.Cell(row, 21).Value = product.IsFeatured;
            worksheet.Cell(row, 22).Value = product.AllowBackorder;
            worksheet.Cell(row, 23).Value = product.DisplayOrder;
            worksheet.Cell(row, 24).Value = product.ControlsStock;
            worksheet.Cell(row, 25).Value = product.Stock?.Quantity ?? 0;
            worksheet.Cell(row, 26).Value = product.MinimumStock;
            worksheet.Cell(row, 27).Value = product.MaximumStock;
            worksheet.Cell(row, 28).Value = product.ReorderPoint;
            worksheet.Cell(row, 29).Value = product.HasExpirationDate;
            worksheet.Cell(row, 30).Value = product.ExpirationDays;
            worksheet.Cell(row, 31).Value = product.ExpirationWarningDays;
        }

        /// <summary>
        /// Aplica formatação à planilha
        /// </summary>
        public void ApplyFormatting(IXLWorksheet worksheet, int lastRow)
        {
            // Auto-ajustar colunas
            worksheet.Columns().AdjustToContents();
            
            // Fixar primeira linha
            worksheet.SheetView.FreezeRows(1);
            
            // Aplicar bordas
            var dataRange = worksheet.Range($"A1:AE{lastRow}");
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;
            
            // Formatar colunas de preço
            worksheet.Column(15).Style.NumberFormat.Format = "#,##0.00"; // Custo (O)
            worksheet.Column(16).Style.NumberFormat.Format = "#,##0.00"; // Venda (P)
            worksheet.Column(17).Style.NumberFormat.Format = "0.00%"; // Desconto (Q)
            worksheet.Column(18).Style.NumberFormat.Format = "0.00%"; // Taxa (R)
            
            // Cores alternadas nas linhas
            for (int i = 2; i <= lastRow; i++)
            {
                if (i % 2 == 0)
                {
                    worksheet.Row(i).Style.Fill.BackgroundColor = XLColor.AliceBlue;
                }
            }
        }

        /// <summary>
        /// Cria aba de instruções
        /// </summary>
        public void CreateInstructionsSheet(XLWorkbook workbook)
        {
            var sheet = workbook.Worksheets.Add("📋 Instruções");
            
            sheet.Cell("A1").Value = "INSTRUÇÕES DE IMPORTAÇÃO";
            sheet.Cell("A1").Style.Font.Bold = true;
            sheet.Cell("A1").Style.Font.FontSize = 16;
            sheet.Cell("A1").Style.Fill.BackgroundColor = XLColor.DarkBlue;
            sheet.Cell("A1").Style.Font.FontColor = XLColor.White;
            
            var instructions = new[]
            {
                ("A3", "1. CAMPOS OBRIGATÓRIOS", "Apenas campos marcados com * são obrigatórios"),
                ("A4", "   - Nome: nome do produto (máx 200 caracteres)", ""),
                ("A5", "   - ID Categoria: deve existir no sistema", ""),
                ("A6", "   - ID Fornecedor: deve existir no sistema", ""),
                ("A7", "   - Preço Custo e Preço Venda: valores decimais", ""),
                ("A8", "   - Status: Active, Inactive, Discontinued ou OutOfStock", ""),
                ("A10", "2. CAMPOS GERADOS AUTOMATICAMENTE", "Se deixados em branco, serão gerados automaticamente"),
                ("A11", "   - Código Interno: formato PRD000001, PRD000002, etc.", ""),
                ("A12", "   - Código Externo: formato EXT-AAAAMMDDHHMMSS-XXXX", ""),
                ("A13", "   - SKU: formato CAT-AAAAMMDD-XXX (baseado na categoria)", ""),
                ("A15", "3. CÓDIGOS ÚNICOS", "Validação de duplicação"),
                ("A16", "   - Código Interno, Externo, Barcode e SKU devem ser únicos", ""),
                ("A17", "   - O sistema verifica duplicações antes de importar", ""),
                ("A18", "   - Se houver duplicação, a linha será rejeitada", ""),
                ("A20", "4. NOVOS PRODUTOS", "Deixe a coluna ID em branco"),
                ("A21", "5. ATUALIZAR PRODUTOS", "Preencha o ID do produto existente"),
                ("A23", "6. VALIDAÇÕES", ""),
                ("A24", "   - Código Interno: máximo 50 caracteres", ""),
                ("A25", "   - Código Externo: máximo 100 caracteres", ""),
                ("A26", "   - Nome: máximo 200 caracteres", ""),
                ("A27", "   - Preços: devem ser maior ou igual a 0", ""),
                ("A28", "   - Percentuais: entre 0 e 100", ""),
                ("A30", "7. REFERÊNCIAS", ""),
                ("A31", "   - Consulte a aba 'Categorias' para IDs válidos", ""),
                ("A32", "   - Consulte a aba 'Fornecedores' para IDs válidos", ""),
                ("A34", "8. FORMATO DE DADOS", ""),
                ("A35", "   - Datas: não aplicável nesta importação", ""),
                ("A36", "   - Booleanos: TRUE ou FALSE", ""),
                ("A37", "   - Decimais: use ponto (.) como separador", ""),
                ("A39", "9. APÓS IMPORTAÇÃO", ""),
                ("A40", "   - Produtos novos receberão ID automaticamente", ""),
                ("A41", "   - Códigos serão gerados se não fornecidos", ""),
                ("A42", "   - Estoque será criado/atualizado conforme indicado", ""),
                ("A43", "   - Campos de auditoria serão preenchidos automaticamente", "")
            };

            foreach (var (cell, text, _) in instructions)
            {
                var xlCell = sheet.Cell(cell);
                xlCell.Value = text;
                if (text.StartsWith("1.") || text.StartsWith("2.") || text.StartsWith("3.") || 
                    text.StartsWith("4.") || text.StartsWith("5.") || text.StartsWith("6.") || 
                    text.StartsWith("7.") || text.StartsWith("8.") || text.StartsWith("9."))
                {
                    xlCell.Style.Font.Bold = true;
                    xlCell.Style.Fill.BackgroundColor = XLColor.LightGray;
                }
            }
            
            sheet.Columns().AdjustToContents();
        }

        /// <summary>
        /// Cria aba de referência de categorias
        /// </summary>
        public async Task CreateCategoriesReferenceSheet(XLWorkbook workbook)
        {
            var sheet = workbook.Worksheets.Add("Categorias");
            
            sheet.Cell("A1").Value = "ID";
            sheet.Cell("B1").Value = "Código";
            sheet.Cell("C1").Value = "Nome";
            sheet.Cell("D1").Value = "Status";
            
            var headerRange = sheet.Range("A1:D1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
            
            var categories = await _context.Set<Category>()
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            int row = 2;
            foreach (var category in categories)
            {
                sheet.Cell(row, 1).Value = category.CategoryId;
                sheet.Cell(row, 2).Value = category.Code;
                sheet.Cell(row, 3).Value = category.Name;
                sheet.Cell(row, 4).Value = category.IsActive ? "Ativo" : "Inativo";
                row++;
            }
            
            sheet.Columns().AdjustToContents();
        }

        /// <summary>
        /// Cria aba de referência de fornecedores
        /// </summary>
        public async Task CreateSuppliersReferenceSheet(XLWorkbook workbook)
        {
            var sheet = workbook.Worksheets.Add("Fornecedores");
            
            sheet.Cell("A1").Value = "ID";
            sheet.Cell("B1").Value = "Nome";
            sheet.Cell("C1").Value = "Tipo";
            sheet.Cell("D1").Value = "Status";
            
            var headerRange = sheet.Range("A1:D1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
            
            var suppliers = await _context.Set<Person>()
                .Where(p => p.Type == PersonType.Supplier || p.Type == PersonType.Both)
                .OrderBy(p => p.Name)
                .ToListAsync();
            
            int row = 2;
            foreach (var supplier in suppliers)
            {
                sheet.Cell(row, 1).Value = supplier.PersonId;
                sheet.Cell(row, 2).Value = supplier.Name;
                sheet.Cell(row, 3).Value = supplier.Type.ToString();
                sheet.Cell(row, 4).Value = supplier.IsActive ? "Ativo" : "Inativo";
                row++;
            }
            
            sheet.Columns().AdjustToContents();
        }

        /// <summary>
        /// Importa produtos do Excel
        /// </summary>
        public async Task<OperationResult> ImportProductsFromExcelAsync(Stream fileStream, int userId)
        {
            var errors = new List<string>();
            var debugInfo = new List<string>();
            int processedRows = 0;
            int newProducts = 0;
            int updatedProducts = 0;
            int failedRows = 0;
            
            try
            {
                debugInfo.Add($"[DEBUG] Iniciando importação - UserId: {userId}");
                
                using var workbook = new XLWorkbook(fileStream);
                debugInfo.Add($"[DEBUG] Workbook carregado com sucesso");
                
                var worksheet = workbook.Worksheet("Produtos");
                
                if (worksheet == null)
                {
                    debugInfo.Add($"[DEBUG] ERRO: Planilha 'Produtos' não encontrada");
                    debugInfo.Add($"[DEBUG] Abas disponíveis: {string.Join(", ", workbook.Worksheets.Select(w => w.Name))}");
                    return OperationResult.CreateFailure("Planilha 'Produtos' não encontrada", debugInfo);
                }
                
                debugInfo.Add($"[DEBUG] Planilha 'Produtos' encontrada");
                
                var rows = worksheet.RowsUsed().Skip(1); // Pular cabeçalho
                var totalRows = rows.Count();
                debugInfo.Add($"[DEBUG] Total de linhas a processar: {totalRows}");
                
                foreach (var row in rows)
                {
                    var rowNumber = row.RowNumber();
                    
                    try
                    {
                        debugInfo.Add($"[DEBUG] Processando linha {rowNumber}");
                        
                        // Verificar se linha está vazia (Nome vazio - agora na coluna D/4)
                        var productName = row.Cell(4).GetString();
                        if (string.IsNullOrWhiteSpace(productName))
                        {
                            debugInfo.Add($"[DEBUG] Linha {rowNumber}: Linha vazia (sem nome), pulando...");
                            continue;
                        }
                        
                        debugInfo.Add($"[DEBUG] Linha {rowNumber}: Nome do produto = '{productName}'");
                        
                        var product = await ParseProductFromRow(row);
                        debugInfo.Add($"[DEBUG] Linha {rowNumber}: Produto parseado - ID={product.ProductId}, Código={product.InternalCode}");
                        
                        // Garantir que os códigos existam (gerar se necessário)
                        await EnsureProductCodesAsync(product, rowNumber, debugInfo);
                        
                        // Validar produto
                        debugInfo.Add($"[DEBUG] Linha {rowNumber}: Validando produto...");
                        var validationErrors = ValidateProduct(product, rowNumber);
                        if (validationErrors.Any())
                        {
                            debugInfo.Add($"[DEBUG] Linha {rowNumber}: Falha na validação - {validationErrors.Count} erro(s)");
                            foreach (var error in validationErrors)
                            {
                                debugInfo.Add($"[DEBUG]   - {error}");
                            }
                            errors.AddRange(validationErrors);
                            failedRows++;
                            continue;
                        }
                        
                        debugInfo.Add($"[DEBUG] Linha {rowNumber}: Validação OK");
                        
                        // Validar códigos únicos (duplicação)
                        debugInfo.Add($"[DEBUG] Linha {rowNumber}: Validando códigos únicos...");
                        var uniqueCodeErrors = await ValidateUniqueCodesAsync(product, rowNumber);
                        if (uniqueCodeErrors.Any())
                        {
                            debugInfo.Add($"[DEBUG] Linha {rowNumber}: Códigos duplicados encontrados - {uniqueCodeErrors.Count} erro(s)");
                            foreach (var error in uniqueCodeErrors)
                            {
                                debugInfo.Add($"[DEBUG]   - {error}");
                            }
                            errors.AddRange(uniqueCodeErrors);
                            failedRows++;
                            continue;
                        }
                        
                        debugInfo.Add($"[DEBUG] Linha {rowNumber}: Códigos únicos OK");
                        
                        // Inserir ou atualizar
                        if (product.ProductId == 0)
                        {
                            // Novo produto
                            debugInfo.Add($"[DEBUG] Linha {rowNumber}: Inserindo NOVO produto");
                            product.CreatedAt = DateTime.UtcNow;
                            product.CreatedByUserId = userId;
                            _context.Set<Product>().Add(product);
                            newProducts++;
                        }
                        else
                        {
                            // Atualizar existente
                            debugInfo.Add($"[DEBUG] Linha {rowNumber}: Buscando produto existente ID={product.ProductId}");
                            var existing = await _context.Set<Product>()
                                .Include(p => p.Stock)
                                .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
                            
                            if (existing == null)
                            {
                                var error = $"Linha {rowNumber}: Produto ID {product.ProductId} não encontrado";
                                debugInfo.Add($"[DEBUG] {error}");
                                errors.Add(error);
                                failedRows++;
                                continue;
                            }
                            
                            debugInfo.Add($"[DEBUG] Linha {rowNumber}: Produto encontrado, atualizando...");
                            UpdateProduct(existing, product, userId);
                            updatedProducts++;
                        }
                        
                        processedRows++;
                        debugInfo.Add($"[DEBUG] Linha {rowNumber}: Processamento concluído com SUCESSO");
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"Linha {rowNumber}: {ex.Message}";
                        debugInfo.Add($"[DEBUG] EXCEÇÃO na linha {rowNumber}: {ex.GetType().Name}");
                        debugInfo.Add($"[DEBUG]   Mensagem: {ex.Message}");
                        debugInfo.Add($"[DEBUG]   StackTrace: {ex.StackTrace}");
                        errors.Add(errorMsg);
                        failedRows++;
                    }
                }
                
                debugInfo.Add($"[DEBUG] Salvando alterações no banco de dados...");
                await _context.SaveChangesAsync();
                debugInfo.Add($"[DEBUG] Alterações salvas com sucesso!");
                
                // Criar mensagem de resultado
                var message = $"Importação concluída. Processados: {processedRows}, Novos: {newProducts}, Atualizados: {updatedProducts}, Falhas: {failedRows}";
                debugInfo.Add($"[DEBUG] {message}");
                
                // Combinar debug info e erros
                var allMessages = new List<string>();
                allMessages.AddRange(debugInfo);
                if (errors.Any())
                {
                    allMessages.Add("\n=== ERROS ENCONTRADOS ===");
                    allMessages.AddRange(errors);
                }
                
                // Criar mensagem detalhada com debug info
                var detailedMessage = message + "\n\n" + string.Join("\n", allMessages);
                
                if (failedRows == 0)
                {
                    return OperationResult.CreateSuccess(detailedMessage);
                }
                else
                {
                    return OperationResult.CreateFailure(message, allMessages);
                }
            }
            catch (Exception ex)
            {
                debugInfo.Add($"[DEBUG] EXCEÇÃO GERAL: {ex.GetType().Name}");
                debugInfo.Add($"[DEBUG] Mensagem: {ex.Message}");
                debugInfo.Add($"[DEBUG] StackTrace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    debugInfo.Add($"[DEBUG] InnerException: {ex.InnerException.Message}");
                    debugInfo.Add($"[DEBUG] InnerStackTrace: {ex.InnerException.StackTrace}");
                }
                
                errors.Add($"Erro ao processar arquivo: {ex.Message}");
                
                var allMessages = new List<string>();
                allMessages.AddRange(debugInfo);
                allMessages.AddRange(errors);
                
                return OperationResult.CreateFailure("Erro ao importar produtos do Excel", allMessages);
            }
        }

        public async Task<Product> ParseProductFromRow(IXLRow row)
        {
            var rowNumber = row.RowNumber();
            
            try
            {
                // Verificar se linha está vazia (Nome agora está na coluna D)
                if (string.IsNullOrWhiteSpace(row.Cell(4).GetString()))
                    throw new InvalidOperationException($"Nome do produto é obrigatório");
                
                var product = new Product();
                
                // Parse ID (coluna A)
                try { product.ProductId = row.Cell(1).TryGetValue(out int id) ? id : 0; }
                catch (Exception ex) { throw new Exception($"Erro ao ler ID (coluna A): {ex.Message}", ex); }
                
                // Parse Código Interno (coluna B)
                try { product.InternalCode = row.Cell(2).GetString(); }
                catch (Exception ex) { throw new Exception($"Erro ao ler Código Interno (coluna B): {ex.Message}", ex); }
                
                // Parse Código Externo (coluna C) - OBRIGATÓRIO
                try { product.ExternalCode = row.Cell(3).GetString(); }
                catch (Exception ex) { throw new Exception($"Erro ao ler Código Externo (coluna C): {ex.Message}", ex); }
                
                // Parse Nome (coluna D)
                try { product.Name = row.Cell(4).GetString(); }
                catch (Exception ex) { throw new Exception($"Erro ao ler Nome (coluna D): {ex.Message}", ex); }
                
                // Parse Descrição (coluna E)
                try { product.Description = row.Cell(5).GetString(); }
                catch (Exception ex) { throw new Exception($"Erro ao ler Descrição (coluna E): {ex.Message}", ex); }
                
                // Parse Descrição Curta (coluna F)
                try { product.ShortDescription = row.Cell(6).GetString(); }
                catch (Exception ex) { throw new Exception($"Erro ao ler Descrição Curta (coluna F): {ex.Message}", ex); }
                
                // Parse Barcode (coluna G)
                try { product.Barcode = row.Cell(7).GetString(); }
                catch (Exception ex) { throw new Exception($"Erro ao ler Código de Barras (coluna G): {ex.Message}", ex); }
                
                // Parse SKU (coluna H)
                try { product.SKU = row.Cell(8).GetString(); }
                catch (Exception ex) { throw new Exception($"Erro ao ler SKU (coluna H): {ex.Message}", ex); }
                
                // Parse Peso (coluna I)
                try { product.Weight = row.Cell(9).GetString(); }
                catch (Exception ex) { throw new Exception($"Erro ao ler Peso (coluna I): {ex.Message}", ex); }
                
                // Parse Dimensões (coluna J)
                try { product.Dimensions = row.Cell(10).GetString(); }
                catch (Exception ex) { throw new Exception($"Erro ao ler Dimensões (coluna J): {ex.Message}", ex); }
                
                // Parse CategoryId (coluna K)
                try { product.CategoryId = row.Cell(11).GetValue<int>(); }
                catch (Exception ex) { throw new Exception($"Erro ao ler ID Categoria (coluna K): {ex.Message}. Valor: '{row.Cell(11).GetString()}'", ex); }
                
                // Parse SupplierId (coluna M) - Coluna L é Nome Categoria (ref)
                try { product.SupplierId = row.Cell(13).GetValue<int>(); }
                catch (Exception ex) { throw new Exception($"Erro ao ler ID Fornecedor (coluna M): {ex.Message}. Valor: '{row.Cell(13).GetString()}'", ex); }
                
                // Parse CostPrice (coluna O) - Coluna N é Nome Fornecedor (ref)
                try { product.CostPrice = row.Cell(15).GetValue<decimal>(); }
                catch (Exception ex) { throw new Exception($"Erro ao ler Preço Custo (coluna O): {ex.Message}. Valor: '{row.Cell(15).GetString()}'", ex); }
                
                // Parse SalePrice (coluna P)
                try { product.SalePrice = row.Cell(16).GetValue<decimal>(); }
                catch (Exception ex) { throw new Exception($"Erro ao ler Preço Venda (coluna P): {ex.Message}. Valor: '{row.Cell(16).GetString()}'", ex); }
                
                // Parse DiscountPercentage (coluna Q)
                try { product.DiscountPercentage = row.Cell(17).TryGetValue(out decimal disc) ? disc : null; }
                catch (Exception ex) { throw new Exception($"Erro ao ler % Desconto (coluna Q): {ex.Message}", ex); }
                
                // Parse TaxRate (coluna R)
                try { product.TaxRate = row.Cell(18).TryGetValue(out decimal tax) ? tax : 0; }
                catch (Exception ex) { throw new Exception($"Erro ao ler % Taxa/Imposto (coluna R): {ex.Message}", ex); }
                
                // Parse PhotoUrl (coluna S)
                try { product.PhotoUrl = row.Cell(19).GetString(); }
                catch (Exception ex) { throw new Exception($"Erro ao ler URL Foto (coluna S): {ex.Message}", ex); }
                
                // Parse Status (coluna T)
                try 
                { 
                    var statusStr = row.Cell(20).GetString();
                    if (string.IsNullOrWhiteSpace(statusStr))
                        statusStr = "Active"; // Default
                    product.Status = Enum.Parse<ProductStatus>(statusStr, true); 
                }
                catch (Exception ex) { throw new Exception($"Erro ao ler Status (coluna T): {ex.Message}. Valor: '{row.Cell(20).GetString()}'. Use: Active, Inactive, Discontinued ou OutOfStock", ex); }
                
                // Parse IsFeatured (coluna U)
                try { product.IsFeatured = row.Cell(21).TryGetValue(out bool featured) && featured; }
                catch (Exception ex) { throw new Exception($"Erro ao ler Destaque (coluna U): {ex.Message}", ex); }
                
                // Parse AllowBackorder (coluna V)
                try { product.AllowBackorder = row.Cell(22).TryGetValue(out bool backorder) && backorder; }
                catch (Exception ex) { throw new Exception($"Erro ao ler Permite Backorder (coluna V): {ex.Message}", ex); }
                
                // Parse DisplayOrder (coluna W)
                try { product.DisplayOrder = row.Cell(23).TryGetValue(out int order) ? order : 0; }
                catch (Exception ex) { throw new Exception($"Erro ao ler Ordem Exibição (coluna W): {ex.Message}", ex); }
                
                // Parse ControlsStock (coluna X)
                try { product.ControlsStock = !row.Cell(24).TryGetValue(out bool controlStock) || controlStock; }
                catch (Exception ex) { throw new Exception($"Erro ao ler Controla Estoque (coluna X): {ex.Message}", ex); }
                
                // Parse MinimumStock (coluna Z)
                try { product.MinimumStock = row.Cell(26).TryGetValue(out int minStock) ? minStock : null; }
                catch (Exception ex) { throw new Exception($"Erro ao ler Estoque Mínimo (coluna Z): {ex.Message}", ex); }
                
                // Parse MaximumStock (coluna AA)
                try { product.MaximumStock = row.Cell(27).TryGetValue(out int maxStock) ? maxStock : null; }
                catch (Exception ex) { throw new Exception($"Erro ao ler Estoque Máximo (coluna AA): {ex.Message}", ex); }
                
                // Parse ReorderPoint (coluna AB)
                try { product.ReorderPoint = row.Cell(28).TryGetValue(out int reorder) ? reorder : null; }
                catch (Exception ex) { throw new Exception($"Erro ao ler Ponto Reposição (coluna AB): {ex.Message}", ex); }
                
                // Parse HasExpirationDate (coluna AC)
                try { product.HasExpirationDate = row.Cell(29).TryGetValue(out bool hasExp) && hasExp; }
                catch (Exception ex) { throw new Exception($"Erro ao ler Tem Validade (coluna AC): {ex.Message}", ex); }
                
                // Parse ExpirationDays (coluna AD)
                try { product.ExpirationDays = row.Cell(30).TryGetValue(out int expDays) ? expDays : null; }
                catch (Exception ex) { throw new Exception($"Erro ao ler Dias Validade (coluna AD): {ex.Message}", ex); }
                
                // Parse ExpirationWarningDays (coluna AE)
                try { product.ExpirationWarningDays = row.Cell(31).TryGetValue(out int warnDays) ? warnDays : null; }
                catch (Exception ex) { throw new Exception($"Erro ao ler Dias Aviso Validade (coluna AE): {ex.Message}", ex); }
                
                // Criar/atualizar estoque (coluna Y)
                if (product.ControlsStock)
                {
                    try
                    {
                        var stockQty = row.Cell(25).TryGetValue(out int qty) ? qty : 0;
                        product.Stock = new Stock
                        {
                            ProductId = product.ProductId,
                            Quantity = stockQty,
                            LastStockUpdate = DateTime.UtcNow
                        };
                    }
                    catch (Exception ex) 
                    { 
                        throw new Exception($"Erro ao ler Estoque Atual (coluna Y): {ex.Message}", ex); 
                    }
                }
                
                return product;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao processar linha {rowNumber}: {ex.Message}", ex);
            }
        }

        public List<string> ValidateProduct(Product product, int rowNumber)
        {
            var errors = new List<string>();
            
            // Nota: InternalCode, ExternalCode e SKU não são validados aqui pois são gerados automaticamente se vazios
            
            if (string.IsNullOrWhiteSpace(product.Name))
                errors.Add($"Linha {rowNumber}: Nome é obrigatório");
            
            if (product.CategoryId <= 0)
                errors.Add($"Linha {rowNumber}: ID Categoria inválido");
            
            if (product.SupplierId <= 0)
                errors.Add($"Linha {rowNumber}: ID Fornecedor inválido");
            
            if (product.CostPrice < 0)
                errors.Add($"Linha {rowNumber}: Preço de custo não pode ser negativo");
            
            if (product.SalePrice < 0)
                errors.Add($"Linha {rowNumber}: Preço de venda não pode ser negativo");
            
            if (product.DiscountPercentage.HasValue && (product.DiscountPercentage < 0 || product.DiscountPercentage > 100))
                errors.Add($"Linha {rowNumber}: Desconto deve estar entre 0 e 100");
            
            if (product.TaxRate.HasValue && (product.TaxRate < 0 || product.TaxRate > 100))
                errors.Add($"Linha {rowNumber}: Taxa deve estar entre 0 e 100");
            
            return errors;
        }

        public void UpdateProduct(Product existing, Product updated, int userId)
        {
            existing.InternalCode = updated.InternalCode;
            existing.ExternalCode = updated.ExternalCode;
            existing.Name = updated.Name;
            existing.Description = updated.Description;
            existing.ShortDescription = updated.ShortDescription;
            existing.Barcode = updated.Barcode;
            existing.SKU = updated.SKU;
            existing.Weight = updated.Weight;
            existing.Dimensions = updated.Dimensions;
            existing.CategoryId = updated.CategoryId;
            existing.SupplierId = updated.SupplierId;
            existing.CostPrice = updated.CostPrice;
            existing.SalePrice = updated.SalePrice;
            existing.DiscountPercentage = updated.DiscountPercentage;
            existing.TaxRate = updated.TaxRate;
            existing.PhotoUrl = updated.PhotoUrl;
            existing.Status = updated.Status;
            existing.IsFeatured = updated.IsFeatured;
            existing.AllowBackorder = updated.AllowBackorder;
            existing.DisplayOrder = updated.DisplayOrder;
            existing.ControlsStock = updated.ControlsStock;
            existing.MinimumStock = updated.MinimumStock;
            existing.MaximumStock = updated.MaximumStock;
            existing.ReorderPoint = updated.ReorderPoint;
            existing.HasExpirationDate = updated.HasExpirationDate;
            existing.ExpirationDays = updated.ExpirationDays;
            existing.ExpirationWarningDays = updated.ExpirationWarningDays;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedByUserId = userId;
            
            // Atualizar estoque
            if (updated.Stock != null)
            {
                if (existing.Stock == null)
                {
                    existing.Stock = updated.Stock;
                }
                else
                {
                    existing.Stock.Quantity = updated.Stock.Quantity;
                    existing.Stock.LastStockUpdate = DateTime.UtcNow;
                    existing.Stock.LastStockUpdateByUserId = userId;
                }
            }
        }

        /// <summary>
        /// Gera código interno único automaticamente
        /// </summary>
        private async Task<string> GenerateInternalCodeAsync()
        {
            var lastProduct = await _context.Set<Product>()
                .OrderByDescending(p => p.ProductId)
                .FirstOrDefaultAsync();
            
            var nextId = (lastProduct?.ProductId ?? 0) + 1;
            return $"PRD{nextId:D6}"; // Ex: PRD000001, PRD000002, etc.
        }

        /// <summary>
        /// Gera código externo único automaticamente
        /// </summary>
        private async Task<string> GenerateExternalCodeAsync()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"EXT-{timestamp}-{random}"; // Ex: EXT-20251211143025-5432
        }

        /// <summary>
        /// Gera SKU único automaticamente
        /// </summary>
        private async Task<string> GenerateSKUAsync(int categoryId)
        {
            var category = await _context.Set<Category>()
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            
            var categoryPrefix = category?.Code?.ToUpper().Substring(0, Math.Min(3, category.Code.Length)) ?? "GEN";
            var timestamp = DateTime.Now.ToString("yyyyMMdd");
            var random = new Random().Next(100, 999);
            
            return $"{categoryPrefix}-{timestamp}-{random}"; // Ex: ELE-20251211-542
        }

        /// <summary>
        /// Valida se os códigos únicos já existem no banco
        /// </summary>
        private async Task<List<string>> ValidateUniqueCodesAsync(Product product, int rowNumber)
        {
            var errors = new List<string>();

            // Validar InternalCode duplicado
            var existingByInternalCode = await _context.Set<Product>()
                .FirstOrDefaultAsync(p => p.InternalCode == product.InternalCode && p.ProductId != product.ProductId);
            
            if (existingByInternalCode != null)
                errors.Add($"Linha {rowNumber}: Código Interno '{product.InternalCode}' já existe no produto ID {existingByInternalCode.ProductId}");

            // Validar ExternalCode duplicado
            var existingByExternalCode = await _context.Set<Product>()
                .FirstOrDefaultAsync(p => p.ExternalCode == product.ExternalCode && p.ProductId != product.ProductId);
            
            if (existingByExternalCode != null)
                errors.Add($"Linha {rowNumber}: Código Externo '{product.ExternalCode}' já existe no produto ID {existingByExternalCode.ProductId}");

            // Validar Barcode duplicado (se fornecido)
            if (!string.IsNullOrWhiteSpace(product.Barcode))
            {
                var existingByBarcode = await _context.Set<Product>()
                    .FirstOrDefaultAsync(p => p.Barcode == product.Barcode && p.ProductId != product.ProductId);
                
                if (existingByBarcode != null)
                    errors.Add($"Linha {rowNumber}: Código de Barras '{product.Barcode}' já existe no produto ID {existingByBarcode.ProductId}");
            }

            // Validar SKU duplicado (se fornecido)
            if (!string.IsNullOrWhiteSpace(product.SKU))
            {
                var existingBySKU = await _context.Set<Product>()
                    .FirstOrDefaultAsync(p => p.SKU == product.SKU && p.ProductId != product.ProductId);
                
                if (existingBySKU != null)
                    errors.Add($"Linha {rowNumber}: SKU '{product.SKU}' já existe no produto ID {existingBySKU.ProductId}");
            }

            return errors;
        }

        /// <summary>
        /// Garante que o produto tem todos os códigos necessários, gerando automaticamente se necessário
        /// </summary>
        private async Task EnsureProductCodesAsync(Product product, int rowNumber, List<string> debugInfo)
        {
            // Gerar InternalCode se vazio
            if (string.IsNullOrWhiteSpace(product.InternalCode))
            {
                product.InternalCode = await GenerateInternalCodeAsync();
                debugInfo.Add($"[DEBUG] Linha {rowNumber}: Código Interno gerado automaticamente: {product.InternalCode}");
            }

            // Gerar ExternalCode se vazio
            if (string.IsNullOrWhiteSpace(product.ExternalCode))
            {
                product.ExternalCode = await GenerateExternalCodeAsync();
                debugInfo.Add($"[DEBUG] Linha {rowNumber}: Código Externo gerado automaticamente: {product.ExternalCode}");
            }

            // Gerar SKU se vazio
            if (string.IsNullOrWhiteSpace(product.SKU))
            {
                product.SKU = await GenerateSKUAsync(product.CategoryId);
                debugInfo.Add($"[DEBUG] Linha {rowNumber}: SKU gerado automaticamente: {product.SKU}");
            }
        }
    }

   
}