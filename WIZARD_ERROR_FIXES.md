# 🔧 Correções de Erro no Wizard de Produtos

## 📝 Problema Relatado

```
Erro ao criar produto no wizard: An error occurred while saving the entity changes. 
See the inner exception for details.
```

## 🔍 Análise do Problema

O erro "An error occurred while saving the entity changes" indica problemas ao salvar entidades no Entity Framework Core. Possíveis causas:

1. **Chaves estrangeiras inválidas** (CategoryId ou SupplierId = 0)
2. **Campos obrigatórios ausentes**
3. **Violação de restrições do banco de dados**
4. **Problemas com navegação de entidades**

## ✅ Correções Aplicadas

### 1. Validação de Chaves Estrangeiras

**Arquivo:** `ProductManagementViewModel.cs` (linha ~1908)

**Antes:**
```csharp
// Criava o produto diretamente sem validar CategoryId e SupplierId
var productDto = new ProductDto { ... };
```

**Depois:**
```csharp
// Validações extras antes de criar
if (ProductCategoryId <= 0)
{
    ShowStatusMessage("❌ Selecione uma categoria válida", true);
    return;
}

if (ProductSupplierId <= 0)
{
    ShowStatusMessage("❌ Selecione um fornecedor válido", true);
    return;
}
```

**Motivo:** Garante que as chaves estrangeiras sejam válidas antes de tentar criar o produto, evitando erro de integridade referencial.

---

### 2. Logging Detalhado de Erros

**Adicionado logging em 3 pontos críticos:**

#### A) Criação do Produto
```csharp
Debug.WriteLine($"[WIZARD] Tentando criar produto: {productDto.Name}");
var result = await _productService.AddAsync(productDto);

if (!result.Success || result.Data == null)
{
    ShowStatusMessage($"❌ {errorMsg}", true);
    Debug.WriteLine($"[WIZARD ERROR] Erro ao criar produto: {errorMsg}");
    if (result.Errors != null)
    {
        foreach (var error in result.Errors)
        {
            Debug.WriteLine($"[WIZARD ERROR] - {error}");
        }
    }
    return;
}
```

#### B) Criação do Estoque
```csharp
Debug.WriteLine($"[WIZARD] Criando estoque inicial - ProductId: {createdProduct.ProductId}, Quantity: {InitialStockQuantity}");

var stockResult = await _stockService.AddAsync(stockDto);

if (!stockResult.Success)
{
    Debug.WriteLine($"[WIZARD ERROR] Erro ao criar estoque:");
    if (stockResult.Errors != null)
    {
        foreach (var error in stockResult.Errors)
        {
            Debug.WriteLine($"[WIZARD ERROR] - {error}");
        }
    }
}
else
{
    Debug.WriteLine($"[WIZARD] ✅ Estoque criado com sucesso!");
}
```

#### C) Criação da Data de Validade
```csharp
Debug.WriteLine($"[WIZARD] Criando data de validade - ProductId: {createdProduct.ProductId}, Date: {InitialExpirationDate}, Batch: {InitialBatchNumber}");

var expirationResult = await _expirationService.AddAsync(expirationDto);

if (!expirationResult.Success)
{
    Debug.WriteLine($"[WIZARD ERROR] Erro ao criar validade:");
    if (expirationResult.Errors != null)
    {
        foreach (var error in expirationResult.Errors)
        {
            Debug.WriteLine($"[WIZARD ERROR] - {error}");
        }
    }
}
else
{
    Debug.WriteLine($"[WIZARD] ✅ Data de validade criada com sucesso!");
}
```

---

### 3. Tratamento de Exceção Detalhado

**Antes:**
```csharp
catch (Exception ex)
{
    ShowStatusMessage($"Erro ao salvar: {ex.Message}", true);
    Debug.WriteLine($"Erro no wizard: {ex.Message}");
}
```

**Depois:**
```csharp
catch (Exception ex)
{
    ShowStatusMessage($"❌ Erro ao salvar: {ex.Message}", true);
    Debug.WriteLine($"[WIZARD EXCEPTION] {ex.GetType().Name}: {ex.Message}");
    Debug.WriteLine($"[WIZARD EXCEPTION] StackTrace: {ex.StackTrace}");
    
    if (ex.InnerException != null)
    {
        Debug.WriteLine($"[WIZARD EXCEPTION] InnerException: {ex.InnerException.Message}");
        Debug.WriteLine($"[WIZARD EXCEPTION] InnerException StackTrace: {ex.InnerException.StackTrace}");
    }
}
```

**Motivo:** Captura detalhes completos da exceção, incluindo InnerException, que é crucial para identificar problemas do Entity Framework.

---

## 🧪 Como Testar

### Teste 1: Criar Produto Simples (SEM Estoque, SEM Validade)

1. Abra o gerenciamento de produtos
2. Clique em "Adicionar Produto"
3. **Step 1:**
   - Preencha: Nome, Código, Categoria, Fornecedor, Preço de Venda
   - **Desmarque:** "Controla Estoque"
   - **Desmarque:** "Possui Data de Validade"
   - Clique em "Próximo"
4. **Step 2:**
   - Como "Controla Estoque" está desmarcado, pule direto
   - Clique em "Próximo"
5. **Step 3:**
   - Clique em "Finalizar"
6. **Esperado:** Produto criado com sucesso ✅

### Teste 2: Criar Produto COM Estoque, SEM Validade

1. Clique em "Adicionar Produto"
2. **Step 1:**
   - Preencha: Nome, Código, Categoria, Fornecedor, Preço de Venda
   - **Marque:** "Controla Estoque"
   - **Desmarque:** "Possui Data de Validade"
   - Clique em "Próximo"
3. **Step 2:**
   - **Quantidade Inicial:** 100
   - **Custo Unitário:** 50.00
   - **Observações:** "Estoque inicial"
   - Clique em "Próximo"
4. **Step 3:**
   - Clique em "Finalizar"
5. **Esperado:** 
   - Produto criado ✅
   - Estoque criado com quantidade 100 ✅
   - Movimentação de entrada criada automaticamente ✅

### Teste 3: Criar Produto COM Estoque E Validade

1. Clique em "Adicionar Produto"
2. **Step 1:**
   - Preencha: Nome, Código, Categoria, Fornecedor, Preço de Venda
   - **Marque:** "Controla Estoque"
   - **Marque:** "Possui Data de Validade"
   - Clique em "Próximo"
3. **Step 2:**
   - **Quantidade Inicial:** 50
   - **Custo Unitário:** 25.00
   - Clique em "Próximo"
4. **Step 3:**
   - **Data de Validade:** 31/12/2025
   - **Número do Lote:** LOTE-001
   - **Observações:** "Primeiro lote"
   - Clique em "Finalizar"
5. **Esperado:** 
   - Produto criado ✅
   - Estoque criado ✅
   - Data de validade registrada ✅
   - Movimentação de entrada criada ✅

### Teste 4: Validação de Campos Obrigatórios

1. Clique em "Adicionar Produto"
2. **Step 1:**
   - Preencha apenas o Nome
   - **NÃO selecione** Categoria
   - Clique em "Próximo"
3. **Esperado:** Mensagem "❌ Selecione uma categoria válida" ❌

4. Selecione Categoria
5. **NÃO selecione** Fornecedor
6. Clique em "Próximo"
7. **Esperado:** Mensagem "❌ Selecione um fornecedor válido" ❌

---

## 📊 Logs de Debug

Agora, quando houver erro, você verá no **Output > Debug** logs como:

### Sucesso:
```
[WIZARD] Tentando criar produto: Produto Teste
[WIZARD] Criando estoque inicial - ProductId: 123, Quantity: 100
[WIZARD] ✅ Estoque criado com sucesso!
[WIZARD] Criando data de validade - ProductId: 123, Date: 31/12/2025, Batch: LOTE-001
[WIZARD] ✅ Data de validade criada com sucesso!
```

### Erro:
```
[WIZARD] Tentando criar produto: Produto Teste
[WIZARD ERROR] Erro ao criar produto: Dados inválidos.
[WIZARD ERROR] - Nome é obrigatório
[WIZARD ERROR] - Categoria é obrigatória
```

Ou:

```
[WIZARD EXCEPTION] DbUpdateException: An error occurred while saving the entity changes.
[WIZARD EXCEPTION] StackTrace: at Microsoft.EntityFrameworkCore...
[WIZARD EXCEPTION] InnerException: SqlException: The INSERT statement conflicted with FOREIGN KEY constraint...
[WIZARD EXCEPTION] InnerException StackTrace: at Microsoft.Data.SqlClient...
```

---

## 🔍 Identificando Problemas

### Se o erro ainda ocorrer:

1. **Abra a janela "Output"** no Visual Studio
2. **Selecione "Debug"** no dropdown
3. **Procure por linhas iniciadas com `[WIZARD ERROR]` ou `[WIZARD EXCEPTION]`**
4. **Copie a mensagem completa do InnerException**
5. **Compartilhe comigo para análise mais profunda**

### Erros comuns e soluções:

| Erro | Causa | Solução |
|------|-------|---------|
| `FOREIGN KEY constraint 'FK_Products_Categories_CategoryId'` | CategoryId inválido | Verificar se a categoria existe no banco |
| `FOREIGN KEY constraint 'FK_Products_Suppliers_SupplierId'` | SupplierId inválido | Verificar se o fornecedor existe no banco |
| `Cannot insert the value NULL into column 'Name'` | Nome não preenchido | Validação do Step 1 falhou |
| `Já existe registro de estoque para este produto` | Tentando criar estoque duplicado | Verificar se produto já tem estoque |
| `UserId é obrigatório para registrar movimentação` | CurrentUserContext não configurado | Verificar login do usuário |

---

## 📝 Próximos Passos

Se o erro persistir após estas correções, precisaremos investigar:

1. **Banco de dados:**
   - Verificar constraints das tabelas Products, Stocks, Expirations
   - Verificar se Categories e Suppliers existem
   - Verificar se CurrentUser está configurado

2. **Validadores:**
   - ProductValidator
   - StockValidator
   - ExpirationValidator

3. **AutoMapper:**
   - Mapeamento ProductDto → Product
   - Mapeamento StockDto → Stock
   - Mapeamento ExpirationDto → Expiration

---

**Data:** 2 de dezembro de 2025
**Status:** ✅ Logging implementado, aguardando teste
