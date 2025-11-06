# Detecção Automática de Entrada (Entry) vs Saída (Exit) de Estoque

## ✅ Solução Implementada

O sistema **VendaFlex** agora detecta **automaticamente** se uma movimentação de estoque é uma **Entrada (Entry)** ou **Saída (Exit)** baseando-se na diferença entre a quantidade anterior e a nova quantidade.

## 🎯 Como Funciona

### 1. Fluxo Completo do Ajuste de Estoque

```
[Usuário] → [Modal de Ajuste] → [StockManagementViewModel] → [StockService] → 
[StockRepository] → [StockAuditService] → [StockMovement criado automaticamente]
```

### 2. Detecção Inteligente no StockAuditService

O método `LogQuantityChangeAsync` no `StockAuditService` implementa a lógica de detecção:

```csharp
var quantityDifference = newQuantity - previousQuantity;

// Detectar automaticamente Entry ou Exit
StockMovementType movementType = type;
if (type == StockMovementType.Adjustment)
{
    movementType = quantityDifference > 0 ? StockMovementType.Entry :    // Aumentou = Entrada
                   quantityDifference < 0 ? StockMovementType.Exit :     // Diminuiu = Saída
                   StockMovementType.Adjustment;                          // Igual = Ajuste
}
```

### 3. Exemplos Práticos

#### Exemplo 1: Entrada de Estoque (Entry)
```
Quantidade Anterior: 50
Nova Quantidade: 100
Diferença: +50
Tipo Detectado: Entry
Movimentação: "Entrada de estoque: 50 unidades adicionadas"
Referência: "STOCK-ENT-123-20251105120000"
```

#### Exemplo 2: Saída de Estoque (Exit)
```
Quantidade Anterior: 100
Nova Quantidade: 50
Diferença: -50
Tipo Detectado: Exit
Movimentação: "Saída de estoque: 50 unidades removidas"
Referência: "STOCK-EXT-123-20251105120100"
```

#### Exemplo 3: Criação Inicial
```
Quantidade Anterior: 0
Nova Quantidade: 100
Diferença: +100
Tipo Detectado: Entry
Movimentação: "Entrada de estoque: 100 unidades adicionadas"
Referência: "STOCK-ENT-123-20251105120200"
```

## 📝 Implementação nos Arquivos

### StockAuditService.cs

**Método atualizado: `LogQuantityChangeAsync`**

- ✅ Calcula a diferença: `newQuantity - previousQuantity`
- ✅ Detecta automaticamente o tipo:
  - `> 0` = **Entry** (Entrada)
  - `< 0` = **Exit** (Saída)
  - `= 0` = **Adjustment** (sem mudança)
- ✅ Gera referência apropriada:
  - Entry: `STOCK-ENT-{productId}-{timestamp}`
  - Exit: `STOCK-EXT-{productId}-{timestamp}`
  - Adjustment: `STOCK-ADJ-{productId}-{timestamp}`
- ✅ Gera nota padrão descritiva baseada no tipo

### StockRepository.cs

**Novo método: `UpdateQuantityAsync(productId, quantity, userId, notes)`**

- ✅ Sobrecarga que aceita **nota personalizada** do usuário
- ✅ Captura quantidade anterior
- ✅ Atualiza o estoque
- ✅ Chama `StockAuditService.LogQuantityChangeAsync` com a nota
- ✅ O tipo Entry/Exit é determinado automaticamente

### StockService.cs

**Novo método: `UpdateQuantityAsync(productId, quantity, userId, notes)`**

- ✅ Interface pública que aceita nota do usuário
- ✅ Delega para StockRepository
- ✅ Transparente para quem chama

### IStockService.cs

**Nova assinatura:**
```csharp
Task<bool> UpdateQuantityAsync(int productId, int quantity, int? userId, string? notes);
```

### StockManagementViewModel.cs

**Método `SaveAdjustmentAsync` atualizado:**

```csharp
var success = await _stockService.UpdateQuantityAsync(
    AdjustmentProductId, 
    AdjustmentNewQuantity, 
    _currentUserContext.UserId,
    $"Ajuste de estoque: {AdjustmentReason}");  // ← Nota do usuário
```

- ❌ **Não chama mais** `_stockMovementService.AddAsync()` (obsoleto)
- ✅ **Apenas atualiza** o estoque via `_stockService`
- ✅ A movimentação é criada **automaticamente** pelo sistema
- ✅ O tipo é detectado **automaticamente** (Entry ou Exit)

## 🔄 Tipos de Movimentação Gerados

| Situação | Tipo Gerado | Descrição |
|----------|-------------|-----------|
| Adicionar produtos pela primeira vez | **Entry** | Criação inicial do estoque |
| Comprar mais produtos | **Entry** | Entrada/aumento de estoque |
| Vender produtos | **Exit** | Saída via reserva |
| Ajuste manual (aumento) | **Entry** | Correção aumentando quantidade |
| Ajuste manual (diminuição) | **Exit** | Correção diminuindo quantidade |
| Perda/dano | **Exit** | Remoção de estoque |
| Devolução de venda | **Entry** | Retorno ao estoque |
| Cancelamento de reserva | **Return** | Liberação de quantidade reservada |

## 🎨 Interface do Usuário

O modal de **Ajuste de Estoque** é simples e direto:

1. **Nova Quantidade**: Usuário informa a quantidade final desejada
2. **Motivo do Ajuste**: Usuário explica por que está ajustando

O sistema **automaticamente**:
- ✅ Compara com a quantidade atual
- ✅ Calcula a diferença
- ✅ Determina se é Entry ou Exit
- ✅ Cria a movimentação com tipo correto
- ✅ Registra o motivo fornecido pelo usuário

## 💡 Vantagens da Abordagem

1. **Simplicidade para o Usuário**
   - Não precisa escolher "Entrada" ou "Saída"
   - Apenas informa a quantidade final desejada
   - O sistema decide automaticamente

2. **Precisão**
   - Sempre gera o tipo correto
   - Impossível criar movimentação errada
   - Auditoria confiável

3. **Rastreabilidade**
   - Quantidade anterior sempre registrada
   - Quantidade nova sempre registrada
   - Diferença calculada automaticamente
   - Motivo do usuário preservado

4. **Flexibilidade**
   - Funciona para qualquer cenário:
     - Primeira vez criando estoque
     - Aumentar estoque existente
     - Diminuir estoque existente
     - Ajustes positivos ou negativos

## 🚀 Exemplo de Uso Completo

### Cenário: Ajustar estoque de um produto

1. **Usuário** seleciona produto com 100 unidades
2. **Usuário** abre modal "Ajustar Estoque"
3. **Usuário** informa "Nova Quantidade: 150"
4. **Usuário** informa "Motivo: Compra de fornecedor XYZ"
5. **Usuário** clica em "Salvar"

**Sistema executa automaticamente:**
```
1. StockRepository.UpdateQuantityAsync(productId: 123, quantity: 150, userId: 5, 
   notes: "Ajuste de estoque: Compra de fornecedor XYZ")
2. Captura quantidade anterior: 50
3. Atualiza Stock.Quantity = 150
4. StockAuditService.LogQuantityChangeAsync(..., previousQty: 100, newQty: 150, ...)
5. Detecta: 150 - 100 = +50 → Entry
6. Cria StockMovement:
   - Type: Entry
   - Quantity: 50
   - PreviousQuantity: 100
   - NewQuantity: 150
   - Notes: "Ajuste de estoque: Compra de fornecedor XYZ"
   - Reference: "STOCK-ENT-123-20251105143500"
```

## ✅ Conclusão

O sistema está **totalmente automático** e **inteligente**:

- ❌ Usuário **NÃO escolhe** Entry ou Exit
- ✅ Sistema **detecta automaticamente** baseado na diferença
- ✅ Movimentações sempre com tipo correto
- ✅ Auditoria completa e confiável
- ✅ Interface simples e intuitiva

Agora você tem um sistema de auditoria de estoque que é:
- **Automático** - sem intervenção manual
- **Inteligente** - detecta tipo automaticamente
- **Confiável** - impossível errar o tipo
- **Rastreável** - histórico completo preservado
