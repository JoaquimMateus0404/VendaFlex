# ?? Implementação de Notas de Crédito e Débito - CORREÇÃO DE ERROS

## ? Erros Identificados

```
Erro CS0272: A propriedade ou o indexador "OperationResult<string>.Data" não pode ser usado neste contexto porque o acessador set é inacessível
Linhas: 1240 e 1390
```

## ? Solução

O problema ocorre porque `OperationResult<T>.Data` tem o setter **privado** (somente leitura após criação). Não podemos fazer:

```csharp
creditNoteNumber.Data = "NC-..."; // ? ERRO
```

### Correção para IssueCreditNoteAsync() - Linha ~1230-1250

Substituir:
```csharp
var userId = _currentUserContext.UserId ?? originalInvoice.UserId;
var creditNoteNumber = await _companyConfigService.GenerateInvoiceNumberAsync();

if (!creditNoteNumber.Success || string.IsNullOrEmpty(creditNoteNumber.Data))
{
    creditNoteNumber.Data = $"NC-{DateTime.Now:yyyyMMdd}-{originalInvoice.InvoiceId}";  // ? ERRO
}

var creditNote = new InvoiceDto
{
    InvoiceNumber = creditNoteNumber.Data,  // Usar aqui
```

Por:
```csharp
var userId = _currentUserContext.UserId ?? originalInvoice.UserId;

// Gerar número da nota de crédito
string invoiceNumber;
var generatedNumber = await _companyConfigService.GenerateInvoiceNumberAsync();

if (generatedNumber.Success && !string.IsNullOrEmpty(generatedNumber.Data))
{
    invoiceNumber = generatedNumber.Data;  // ? CORRETO
}
else
{
    // Fallback: gerar manualmente
    invoiceNumber = $"NC-{DateTime.Now:yyyyMMdd}-{originalInvoice.InvoiceId}";  // ? CORRETO
}

var creditNote = new InvoiceDto
{
    InvoiceNumber = invoiceNumber,  // ? Usar variável local
```

### Correção para IssueDebitNoteAsync() - Linha ~1380-1400

Substituir:
```csharp
var userId = _currentUserContext.UserId ?? originalInvoice.UserId;

// 10% do valor original como exemplo
decimal debitAmount = originalInvoice.Total * 0.10m;

var debitNoteNumber = await _companyConfigService.GenerateInvoiceNumberAsync();
if (!debitNoteNumber.Success || string.IsNullOrEmpty(debitNoteNumber.Data))
{
    debitNoteNumber.Data = $"ND-{DateTime.Now:yyyyMMdd}-{originalInvoice.InvoiceId}";  // ? ERRO
}

var debitNote = new InvoiceDto
{
    InvoiceNumber = debitNoteNumber.Data,  // Usar aqui
```

Por:
```csharp
var userId = _currentUserContext.UserId ?? originalInvoice.UserId;

// 10% do valor original como exemplo
decimal debitAmount = originalInvoice.Total * 0.10m;

// Gerar número da nota de débito
string invoiceNumber;
var generatedNumber = await _companyConfigService.GenerateInvoiceNumberAsync();

if (generatedNumber.Success && !string.IsNullOrEmpty(generatedNumber.Data))
{
    invoiceNumber = generatedNumber.Data;  // ? CORRETO
}
else
{
    // Fallback: gerar manualmente
    invoiceNumber = $"ND-{DateTime.Now:yyyyMMdd}-{originalInvoice.InvoiceId}";  // ? CORRETO
}

var debitNote = new InvoiceDto
{
    InvoiceNumber = invoiceNumber,  // ? Usar variável local
```

## ?? Resumo da Correção

### Problema
- `OperationResult<T>.Data` é **read-only** externamente
- Tentativa de atribuir valor causava erro CS0272

### Solução
- Criar variável local `string invoiceNumber`
- Atribuir o valor gerado ou fallback à variável
- Usar a variável local no DTO

### Código Correto Padrão
```csharp
// ? Pattern correto para usar OperationResult
string resultado;
var operationResult = await AlgumMetodo();

if (operationResult.Success && !string.IsNullOrEmpty(operationResult.Data))
{
    resultado = operationResult.Data;  // Ler o valor
}
else
{
    resultado = "valor_padrao";  // Fallback
}

// Usar resultado
```

## ?? Testes Recomendados

Após a correção, testar:

1. **Nota de Crédito:**
   - Selecionar fatura paga
   - Clicar em "Nota de Crédito"
   - Verificar criação com número `NC-...`
   - Confirmar restauração de estoque
   - Validar pagamento negativo criado

2. **Nota de Débito:**
   - Selecionar fatura
   - Clicar em "Nota de Débito"
   - Verificar criação com número `ND-...`
   - Confirmar cálculo de 10% de juros
   - Validar data de vencimento (15 dias)

## ?? Referências

- **IssueCreditNoteAsync**: Linhas ~1197-1298
- **IssueDebitNoteAsync**: Linhas ~1300-1400
- **OperationResult**: `VendaFlex.Core.Utils.OperationResult<T>`

---

**Status**: ? Documentado  
**Data**: 29/11/2024  
**Desenvolvedor**: GitHub Copilot
