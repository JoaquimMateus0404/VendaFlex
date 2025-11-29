using AutoMapper;
using FluentValidation;
using System.Diagnostics;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.Utils;
using VendaFlex.Data.Entities;
using VendaFlex.Data.Repositories;

namespace VendaFlex.Core.Services
{
    /// <summary>
    /// Serviço para gestão de faturas e operações relacionadas.
    /// </summary>
    public class InvoiceService : IInvoiceService
    {
        private readonly InvoiceRepository _invoiceRepository;
        private readonly InvoiceProductRepository _invoiceProductRepository;
        private readonly StockRepository _stockRepository;
        private readonly IValidator<InvoiceDto> _invoiceValidator;
        private readonly IMapper _mapper;
        private readonly ICurrentUserContext _currentUserContext;
        
        public InvoiceService(
            InvoiceRepository invoiceRepository,
            InvoiceProductRepository invoiceProductRepository,
            StockRepository stockRepository,
            IValidator<InvoiceDto> invoiceValidator,
            IMapper mapper,
            ICurrentUserContext currentUserContext)
        {
            _invoiceRepository = invoiceRepository;
            _invoiceProductRepository = invoiceProductRepository;
            _stockRepository = stockRepository;
            _invoiceValidator = invoiceValidator;
            _mapper = mapper;
            _currentUserContext = currentUserContext;
        }

        public async Task<OperationResult<InvoiceDto>> AddAsync(InvoiceDto invoice)
        {
            try
            {
                if (invoice == null)
                    return OperationResult<InvoiceDto>.CreateFailure("Fatura é obrigatória.");

                var validation = await _invoiceValidator.ValidateAsync(invoice);
                if (!validation.IsValid)
                    return OperationResult<InvoiceDto>.CreateFailure("Dados inválidos.", validation.Errors.Select(e => e.ErrorMessage));

                if (await _invoiceRepository.NumberExistsAsync(invoice.InvoiceNumber))
                    return OperationResult<InvoiceDto>.CreateFailure("N�mero de fatura já está em uso.");

                var entity = _mapper.Map<Invoice>(invoice);
                var created = await _invoiceRepository.AddAsync(entity);
                var dto = _mapper.Map<InvoiceDto>(created);
                return OperationResult<InvoiceDto>.CreateSuccess(dto, "Fatura criada com sucesso.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Erro na class de servico Invoice: ", ex.Message);
                return OperationResult<InvoiceDto>.CreateFailure("Erro ao criar fatura.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<bool>> DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<bool>.CreateFailure("ID inv�lido.");

                var exists = await _invoiceRepository.ExistsAsync(id);
                if (!exists)
                    return OperationResult<bool>.CreateFailure("Fatura n�o encontrada.");

                var deleted = await _invoiceRepository.DeleteAsync(id);
                return deleted
                    ? OperationResult<bool>.CreateSuccess(true, "Fatura removida com sucesso.")
                    : OperationResult<bool>.CreateFailure("N�o foi poss�vel remover a fatura.");
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.CreateFailure("Erro ao remover fatura.", new[] { ex.Message });
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            try { return await _invoiceRepository.ExistsAsync(id); } catch { return false; }
        }

        public async Task<OperationResult<IEnumerable<InvoiceDto>>> GetAllAsync()
        {
            try
            {
                var entities = await _invoiceRepository.GetAllAsync();
                var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(entities);
                return OperationResult<IEnumerable<InvoiceDto>>.CreateSuccess(dtos, $"{dtos.Count()} fatura(s) encontrada(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Erro ao listar faturas.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<InvoiceDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                    return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Data inicial n�o pode ser maior que a final.");

                var entities = await _invoiceRepository.GetByDateRangeAsync(startDate, endDate);
                var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(entities);
                return OperationResult<IEnumerable<InvoiceDto>>.CreateSuccess(dtos, $"{dtos.Count()} fatura(s) no per�odo.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Erro ao buscar por per�odo.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<InvoiceDto>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return OperationResult<InvoiceDto>.CreateFailure("ID inv�lido.");

                var entity = await _invoiceRepository.GetByIdAsync(id);
                if (entity == null)
                    return OperationResult<InvoiceDto>.CreateFailure("Fatura n�o encontrada.");

                var dto = _mapper.Map<InvoiceDto>(entity);
                return OperationResult<InvoiceDto>.CreateSuccess(dto, "Fatura encontrada.");
            }
            catch (Exception ex)
            {
                return OperationResult<InvoiceDto>.CreateFailure("Erro ao buscar fatura.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<InvoiceDto>> GetByNumberAsync(string invoiceNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(invoiceNumber))
                    return OperationResult<InvoiceDto>.CreateFailure("N�mero da fatura � obrigat�rio.");

                var entity = await _invoiceRepository.GetByNumberAsync(invoiceNumber);
                if (entity == null)
                    return OperationResult<InvoiceDto>.CreateFailure("Fatura n�o encontrada.");

                var dto = _mapper.Map<InvoiceDto>(entity);
                return OperationResult<InvoiceDto>.CreateSuccess(dto, "Fatura encontrada.");
            }
            catch (Exception ex)
            {
                return OperationResult<InvoiceDto>.CreateFailure("Erro ao buscar fatura por n�mero.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<InvoiceDto>>> GetByPersonIdAsync(int personId)
        {
            try
            {
                if (personId <= 0)
                    return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Cliente inv�lido.");

                var entities = await _invoiceRepository.GetByPersonIdAsync(personId);
                var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(entities);
                return OperationResult<IEnumerable<InvoiceDto>>.CreateSuccess(dtos, $"{dtos.Count()} fatura(s) do cliente.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Erro ao buscar por cliente.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<InvoiceDto>>> GetByStatusAsync(InvoiceStatus status)
        {
            try
            {
                var entities = await _invoiceRepository.GetByStatusAsync(status);
                var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(entities);
                return OperationResult<IEnumerable<InvoiceDto>>.CreateSuccess(dtos, $"{dtos.Count()} fatura(s) com status {status}.");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Erro ao buscar por status.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<IEnumerable<InvoiceDto>>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            try
            {
                if (pageNumber < 1)
                    return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("P�gina deve ser >= 1.");
                if (pageSize < 1)
                    return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Tamanho da p�gina deve ser > 0.");

                var entities = await _invoiceRepository.GetPagedAsync(pageNumber, pageSize);
                var dtos = _mapper.Map<IEnumerable<InvoiceDto>>(entities);
                return OperationResult<IEnumerable<InvoiceDto>>.CreateSuccess(dtos, $"P�gina {pageNumber} com {dtos.Count()} fatura(s).");
            }
            catch (Exception ex)
            {
                return OperationResult<IEnumerable<InvoiceDto>>.CreateFailure("Erro ao buscar paginado.", new[] { ex.Message });
            }
        }

        public async Task<int> GetTotalCountAsync()
        {
            try { return await _invoiceRepository.GetTotalCountAsync(); } catch { return 0; }
        }

        public async Task<bool> NumberExistsAsync(string invoiceNumber, int? excludeId = null)
        {
            try { return await _invoiceRepository.NumberExistsAsync(invoiceNumber, excludeId); } catch { return false; }
        }

        public async Task<OperationResult<InvoiceDto>> UpdateAsync(InvoiceDto invoice)
        {
            try
            {
                if (invoice == null)
                    return OperationResult<InvoiceDto>.CreateFailure("Fatura é obrigatória.");

                var validation = await _invoiceValidator.ValidateAsync(invoice);
                if (!validation.IsValid)
                    return OperationResult<InvoiceDto>.CreateFailure("Dados inválidos.", validation.Errors.Select(e => e.ErrorMessage));

                var existing = await _invoiceRepository.GetByIdAsync(invoice.InvoiceId);
                if (existing == null)
                    return OperationResult<InvoiceDto>.CreateFailure("Fatura não encontrada.");

                if (await _invoiceRepository.NumberExistsAsync(invoice.InvoiceNumber, invoice.InvoiceId))
                    return OperationResult<InvoiceDto>.CreateFailure("Número de fatura já está em uso.");

                _mapper.Map(invoice, existing);
                var updated = await _invoiceRepository.UpdateAsync(existing);
                var dto = _mapper.Map<InvoiceDto>(updated);
                return OperationResult<InvoiceDto>.CreateSuccess(dto, "Fatura atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<InvoiceDto>.CreateFailure("Erro ao atualizar fatura.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<bool>> CancelAsync(int invoiceId, string reason)
        {
            try
            {
                if (invoiceId <= 0)
                    return OperationResult<bool>.CreateFailure("ID inválido.");

                if (string.IsNullOrWhiteSpace(reason))
                    return OperationResult<bool>.CreateFailure("Motivo do cancelamento é obrigatório.");

                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null)
                    return OperationResult<bool>.CreateFailure("Fatura não encontrada.");

                if (invoice.Status == InvoiceStatus.Cancelled)
                    return OperationResult<bool>.CreateFailure("Fatura já está cancelada.");

                Debug.WriteLine($"[CANCEL] Iniciando cancelamento da fatura #{invoiceId} ({invoice.InvoiceNumber})");
                Debug.WriteLine($"[CANCEL] Motivo: {reason}");
                Debug.WriteLine($"[CANCEL] Status atual: {invoice.Status}");

                // Buscar produtos da fatura para restaurar o estoque
                var invoiceProducts = await _invoiceProductRepository.GetByInvoiceIdAsync(invoiceId);
                
                if (!invoiceProducts.Any())
                {
                    Debug.WriteLine($"[CANCEL] AVISO: Fatura sem produtos");
                }

                var userId = _currentUserContext.UserId ?? invoice.UserId;
                var stockRestorations = new List<string>();
                var stockErrors = new List<string>();

                // Restaurar estoque de todos os produtos
                foreach (var product in invoiceProducts)
                {
                    try
                    {
                        var stock = await _stockRepository.GetByProductIdAsync(product.ProductId);
                        if (stock != null)
                        {
                            var previousQty = stock.Quantity;
                            var restoredQty = (int)product.Quantity;
                            var newQty = stock.Quantity + restoredQty;

                            Debug.WriteLine($"[CANCEL] Restaurando estoque - Produto: {product.ProductId}, " +
                                          $"Qtd Anterior: {previousQty}, Nova Qtd: {newQty}, Restaurado: +{restoredQty}");

                            // Atualizar quantidade com nota explicativa detalhada
                            await _stockRepository.UpdateQuantityAsync(
                                product.ProductId,
                                newQty,
                                userId,
                                $"Restauração - Cancelamento da fatura #{invoice.InvoiceNumber}. Motivo: {reason}");

                            var productName = product.Product?.Name ?? $"Produto {product.ProductId}";
                            stockRestorations.Add($"{productName}: {previousQty} → {newQty} (+{restoredQty})");
                        }
                        else
                        {
                            var productName = product.Product?.Name ?? $"Produto {product.ProductId}";
                            var errorMsg = $"{productName}: estoque não encontrado";
                            stockErrors.Add(errorMsg);
                            Debug.WriteLine($"[CANCEL] ERRO: {errorMsg}");
                        }
                    }
                    catch (Exception ex)
                    {
                        var productName = product.Product?.Name ?? $"Produto {product.ProductId}";
                        var errorMsg = $"{productName}: {ex.Message}";
                        stockErrors.Add(errorMsg);
                        Debug.WriteLine($"[CANCEL] ERRO ao restaurar produto {product.ProductId}: {ex.Message}");
                    }
                }

                // Atualizar status da fatura
                var previousStatus = invoice.Status;
                invoice.Status = InvoiceStatus.Cancelled;
                
                // Adicionar informações detalhadas nas notas internas
                var cancellationNote = $"Cancelado em {DateTime.Now:dd/MM/yyyy HH:mm} por usuário #{userId}.\n" +
                                     $"Motivo: {reason}\n" +
                                     $"Status anterior: {previousStatus}\n" +
                                     $"Produtos restaurados: {stockRestorations.Count}/{invoiceProducts.Count()}";
                
                if (stockErrors.Any())
                {
                    cancellationNote += $"\nErros na restauração: {stockErrors.Count}";
                }

                invoice.InternalNotes = cancellationNote + "\n---\n" + (invoice.InternalNotes ?? string.Empty);

                // Salvar alterações na fatura
                await _invoiceRepository.UpdateAsync(invoice);

                Debug.WriteLine($"[CANCEL] Cancelamento concluído!");
                Debug.WriteLine($"[CANCEL] - Status: {previousStatus} → {invoice.Status}");
                Debug.WriteLine($"[CANCEL] - Produtos processados: {invoiceProducts.Count()}");
                Debug.WriteLine($"[CANCEL] - Estoques restaurados: {stockRestorations.Count}");
                
                if (stockRestorations.Any())
                {
                    Debug.WriteLine($"[CANCEL] Restaurações de estoque:");
                    foreach (var restoration in stockRestorations)
                    {
                        Debug.WriteLine($"[CANCEL]   ✓ {restoration}");
                    }
                }

                if (stockErrors.Any())
                {
                    Debug.WriteLine($"[CANCEL] Erros durante restauração:");
                    foreach (var error in stockErrors)
                    {
                        Debug.WriteLine($"[CANCEL]   ✗ {error}");
                    }
                }

                // Mensagem de sucesso com informações relevantes
                var successMessage = "Fatura cancelada com sucesso!";
                if (stockRestorations.Any())
                {
                    successMessage += $" {stockRestorations.Count} produto(s) restaurado(s) ao estoque.";
                }
                
                if (stockErrors.Any())
                {
                    successMessage += $" Atenção: {stockErrors.Count} erro(s) ao restaurar alguns produtos.";
                }

                return OperationResult<bool>.CreateSuccess(true, successMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CANCEL] ERRO GERAL ao cancelar fatura: {ex.Message}");
                Debug.WriteLine($"[CANCEL] Stack trace: {ex.StackTrace}");
                return OperationResult<bool>.CreateFailure(
                    "Erro ao cancelar fatura.", 
                    new[] { ex.Message });
            }
        }

        public async Task<OperationResult<bool>> ReopenAsync(int invoiceId)
        {
            try
            {
                if (invoiceId <= 0)
                    return OperationResult<bool>.CreateFailure("ID inválido.");

                var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (invoice == null)
                    return OperationResult<bool>.CreateFailure("Fatura não encontrada.");

                if (invoice.Status != InvoiceStatus.Cancelled)
                    return OperationResult<bool>.CreateFailure("Apenas faturas canceladas podem ser reabertas.");

                // Atualizar status para rascunho ou confirmado
                invoice.Status = InvoiceStatus.Draft;
                // Adicionar nota de reabertura
                invoice.InternalNotes = $"Reaberto em {DateTime.Now:dd/MM/yyyy HH:mm}. " + invoice.InternalNotes;

                await _invoiceRepository.UpdateAsync(invoice);

                return OperationResult<bool>.CreateSuccess(true, "Fatura reaberta com sucesso.");
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.CreateFailure("Erro ao reabrir fatura.", new[] { ex.Message });
            }
        }

        public async Task<OperationResult<InvoiceDto>> DuplicateAsync(int invoiceId)
        {
            try
            {
                if (invoiceId <= 0)
                    return OperationResult<InvoiceDto>.CreateFailure("ID inválido.");

                var originalInvoice = await _invoiceRepository.GetByIdAsync(invoiceId);
                if (originalInvoice == null)
                    return OperationResult<InvoiceDto>.CreateFailure("Fatura não encontrada.");

                // Buscar produtos da fatura original
                var originalProducts = await _invoiceProductRepository.GetByInvoiceIdAsync(invoiceId);
                
                if (!originalProducts.Any())
                {
                    return OperationResult<InvoiceDto>.CreateFailure(
                        "Não é possível duplicar uma fatura sem produtos.");
                }

                Debug.WriteLine($"[DUPLICATE] Iniciando duplicação da fatura #{invoiceId} com {originalProducts.Count()} produtos");

                // Verificar disponibilidade de estoque para todos os produtos
                var stockIssues = new List<string>();
                foreach (var product in originalProducts)
                {
                    var availableQty = await _stockRepository.GetAvailableQuantityAsync(product.ProductId);
                    if (availableQty < product.Quantity)
                    {
                        // Buscar nome do produto para mensagem mais clara
                        var productName = product.Product?.Name ?? $"Produto {product.ProductId}";
                        stockIssues.Add($"{productName}: disponível {availableQty}, necessário {product.Quantity}");
                    }
                }

                if (stockIssues.Any())
                {
                    return OperationResult<InvoiceDto>.CreateFailure(
                        $"Estoque insuficiente para duplicar a fatura:\n{string.Join("\n", stockIssues)}");
                }

                // Obter usuário atual ou usar o da fatura original
                var userId = _currentUserContext.UserId ?? originalInvoice.UserId;

                // Gerar número de fatura inteligente e compacto
                // Remover sufixos anteriores de cópias para evitar números muito longos
                var baseInvoiceNumber = originalInvoice.InvoiceNumber;
                
                // Se já é uma cópia, extrair o número base
                var copyIndex = baseInvoiceNumber.IndexOf("-C", StringComparison.OrdinalIgnoreCase);
                if (copyIndex > 0)
                {
                    baseInvoiceNumber = baseInvoiceNumber.Substring(0, copyIndex);
                }
                
                // Gerar novo número compacto: BASE-C-TIMESTAMP_CURTO
                // Exemplo: INV-2025-0003-C-1129160435 (max 30 caracteres)
                var timestamp = DateTime.Now.ToString("MMddHHmmss"); // 10 caracteres
                var newInvoiceNumber = $"{baseInvoiceNumber}-C-{timestamp}";
                
                // Garantir que não exceda 50 caracteres (limite do banco)
                if (newInvoiceNumber.Length > 50)
                {
                    // Se ainda assim for muito longo, truncar o base e adicionar timestamp
                    var maxBaseLength = 50 - 13; // 13 = "-C-" + 10 dígitos timestamp
                    baseInvoiceNumber = baseInvoiceNumber.Substring(0, Math.Min(maxBaseLength, baseInvoiceNumber.Length));
                    newInvoiceNumber = $"{baseInvoiceNumber}-C-{timestamp}";
                }

                Debug.WriteLine($"[DUPLICATE] Número da fatura original: {originalInvoice.InvoiceNumber}");
                Debug.WriteLine($"[DUPLICATE] Número base extraído: {baseInvoiceNumber}");
                Debug.WriteLine($"[DUPLICATE] Novo número gerado: {newInvoiceNumber} (length: {newInvoiceNumber.Length})");

                // Verificar se o número já existe (improvável, mas seguro)
                var attempts = 0;
                while (await _invoiceRepository.NumberExistsAsync(newInvoiceNumber) && attempts < 10)
                {
                    attempts++;
                    timestamp = DateTime.Now.AddSeconds(attempts).ToString("MMddHHmmss");
                    newInvoiceNumber = $"{baseInvoiceNumber}-C-{timestamp}";
                    Debug.WriteLine($"[DUPLICATE] Número já existe, tentativa {attempts}: {newInvoiceNumber}");
                }

                // Criar nova fatura baseada na original
                var newInvoice = new Invoice
                {
                    PersonId = originalInvoice.PersonId,
                    UserId = userId,
                    Date = DateTime.UtcNow,
                    DueDate = originalInvoice.DueDate.HasValue 
                        ? DateTime.UtcNow.AddDays((originalInvoice.DueDate.Value - originalInvoice.Date).Days)
                        : null,
                    InvoiceNumber = newInvoiceNumber,
                    Status = InvoiceStatus.Draft,
                    SubTotal = originalInvoice.SubTotal,
                    TaxAmount = originalInvoice.TaxAmount,
                    DiscountAmount = originalInvoice.DiscountAmount,
                    ShippingCost = originalInvoice.ShippingCost,
                    Total = originalInvoice.Total,
                    PaidAmount = 0,
                    Notes = $"Cópia da fatura {originalInvoice.InvoiceNumber}" ?? string.Empty,
                    InternalNotes = $"Duplicada da fatura #{originalInvoice.InvoiceNumber} pelo usuário {userId}. " + 
                                  (originalInvoice.InternalNotes ?? string.Empty),
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = userId,
                    UpdatedAt = null,
                    UpdatedByUserId = 0
                };

                Debug.WriteLine($"[DUPLICATE] Salvando nova fatura...");
                Debug.WriteLine($"[DUPLICATE] - InvoiceNumber: {newInvoice.InvoiceNumber} (length: {newInvoice.InvoiceNumber.Length})");
                Debug.WriteLine($"[DUPLICATE] - PersonId: {newInvoice.PersonId}");
                Debug.WriteLine($"[DUPLICATE] - UserId: {newInvoice.UserId}");
                Debug.WriteLine($"[DUPLICATE] - Status: {newInvoice.Status}");
                Debug.WriteLine($"[DUPLICATE] - Total: {newInvoice.Total}");

                // Salvar a nova fatura
                Invoice created;
                try
                {
                    created = await _invoiceRepository.AddAsync(newInvoice);
                    Debug.WriteLine($"[DUPLICATE] Nova fatura criada com sucesso: #{created.InvoiceNumber} (ID: {created.InvoiceId})");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DUPLICATE] ERRO ao salvar nova fatura: {ex.Message}");
                    Debug.WriteLine($"[DUPLICATE] Inner Exception: {ex.InnerException?.Message}");
                    
                    var errorMessage = ex.InnerException?.Message ?? ex.Message;
                    return OperationResult<InvoiceDto>.CreateFailure(
                        $"Erro ao salvar nova fatura: {errorMessage}");
                }

                // Copiar todos os produtos da fatura original E atualizar o estoque
                var copiedProductsCount = 0;
                var stockUpdates = new List<string>();
                
                foreach (var originalProduct in originalProducts)
                {
                    try
                    {
                        // Copiar o produto para a nova fatura
                        var newProduct = new InvoiceProduct
                        {
                            InvoiceId = created.InvoiceId,
                            ProductId = originalProduct.ProductId,
                            Quantity = originalProduct.Quantity,
                            UnitPrice = originalProduct.UnitPrice,
                            DiscountPercentage = originalProduct.DiscountPercentage,
                            TaxRate = originalProduct.TaxRate
                        };

                        await _invoiceProductRepository.AddAsync(newProduct);
                        copiedProductsCount++;

                        // Atualizar o estoque (deduzir a quantidade)
                        var stock = await _stockRepository.GetByProductIdAsync(originalProduct.ProductId);
                        if (stock != null)
                        {
                            var previousQty = stock.Quantity;
                            var newQty = stock.Quantity - (int)originalProduct.Quantity;

                            Debug.WriteLine($"[DUPLICATE] Atualizando estoque - Produto: {originalProduct.ProductId}, " +
                                          $"Qtd Anterior: {previousQty}, Nova Qtd: {newQty}, Deduzido: {originalProduct.Quantity}");

                            // Atualizar quantidade com nota explicativa
                            await _stockRepository.UpdateQuantityAsync(
                                originalProduct.ProductId,
                                newQty,
                                userId,
                                $"Dedução - Duplicação de fatura #{originalInvoice.InvoiceNumber}");

                            var productName = originalProduct.Product?.Name ?? $"Produto {originalProduct.ProductId}";
                            stockUpdates.Add($"{productName}: {previousQty} → {newQty} (-{originalProduct.Quantity})");
                        }
                        else
                        {
                            Debug.WriteLine($"[DUPLICATE] AVISO: Stock não encontrado para produto {originalProduct.ProductId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[DUPLICATE] ERRO ao processar produto {originalProduct.ProductId}: {ex.Message}");
                    }
                }

                Debug.WriteLine($"[DUPLICATE] Duplicação concluída com sucesso!");
                Debug.WriteLine($"[DUPLICATE] - {copiedProductsCount} produtos copiados");
                Debug.WriteLine($"[DUPLICATE] - {stockUpdates.Count} atualizações de estoque realizadas");

                var dto = _mapper.Map<InvoiceDto>(created);
                
                return OperationResult<InvoiceDto>.CreateSuccess(
                    dto, 
                    $"Fatura duplicada com sucesso! {copiedProductsCount} produto(s) copiado(s) e estoque atualizado.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DUPLICATE] ERRO GERAL: {ex.Message}");
                Debug.WriteLine($"[DUPLICATE] Inner: {ex.InnerException?.Message}");
                
                var errorMessage = ex.InnerException?.Message ?? ex.Message;
                return OperationResult<InvoiceDto>.CreateFailure(
                    $"Erro ao duplicar fatura: {errorMessage}");
            }
        }
    }
}
