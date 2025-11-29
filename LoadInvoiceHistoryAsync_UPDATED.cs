// Substitua o método LoadInvoiceHistoryAsync (linhas 861-883) pelo código abaixo:

private async Task LoadInvoiceHistoryAsync(int invoiceId)
{
    try
    {
        var historyItems = new List<InvoiceHistoryItemDto>();

        // Buscar a fatura para obter informações de auditoria
        var invoiceResult = await _invoiceService.GetByIdAsync(invoiceId);
        if (invoiceResult.Success && invoiceResult.Data != null)
        {
            var invoice = invoiceResult.Data;

            // Adicionar item de criação
            if (invoice.CreatedAt.HasValue)
            {
                var creatorName = string.IsNullOrEmpty(invoice.CreatedBy) ? "Sistema" : invoice.CreatedBy;

                historyItems.Add(new InvoiceHistoryItemDto
                {
                    ActionDescription = "Fatura criada",
                    ActionIcon = "Plus",
                    UserName = creatorName,
                    Timestamp = invoice.CreatedAt.Value
                });
            }

            // Adicionar item de atualização (se houver e for diferente da criação)
            if (invoice.UpdatedAt.HasValue && 
                invoice.UpdatedAt.Value != invoice.CreatedAt)
            {
                var updaterName = string.IsNullOrEmpty(invoice.UpdatedBy) ? "Sistema" : invoice.UpdatedBy;

                historyItems.Add(new InvoiceHistoryItemDto
                {
                    ActionDescription = "Fatura atualizada",
                    ActionIcon = "Pencil",
                    UserName = updaterName,
                    Timestamp = invoice.UpdatedAt.Value
                });
            }
        }

        // Ordenar por data (mais recente primeiro)
        SelectedInvoiceHistory = new ObservableCollection<InvoiceHistoryItemDto>(
            historyItems.OrderByDescending(h => h.Timestamp)
        );

        await Task.CompletedTask;
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Erro ao carregar histórico: {ex.Message}");
        SelectedInvoiceHistory = new ObservableCollection<InvoiceHistoryItemDto>();
    }
}
