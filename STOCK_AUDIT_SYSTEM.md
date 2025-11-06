# Sistema de Auditoria Automática de Estoque

## Visão Geral

O sistema VendaFlex agora implementa **auditoria automática** para todas as movimentações de estoque. Isso significa que a tabela `StockMovement` funciona como um **log de auditoria imutável**, registrando automaticamente todas as alterações feitas na tabela `Stock`.

## ⚠️ IMPORTANTE

- **Movimentações NÃO podem ser criadas manualmente** pelo usuário através da interface
- **Movimentações NÃO podem ser editadas ou deletadas** - são registros de auditoria imutáveis
- Todas as movimentações são criadas **automaticamente pelo sistema** quando há alterações no estoque

## Arquitetura

### 1. StockAuditService
**Localização:** `VendaFlex/Core/Services/StockAuditService.cs`

Serviço responsável por criar automaticamente os registros de auditoria. Possui os seguintes métodos:

#### Métodos Principais:

- **LogStockCreationAsync**: Registra a criação inicial de um estoque
- **LogQuantityChangeAsync**: Registra alterações na quantidade de estoque (ajustes manuais)
- **LogReserveAsync**: Registra reservas de estoque (vendas, separações)
- **LogReleaseAsync**: Registra liberação de quantidades reservadas (cancelamentos)
- **LogEntryAsync**: Registra entradas de estoque (compras, devoluções)
- **LogExitAsync**: Registra saídas de estoque (vendas, perdas)

#### Características:
- Todos os métodos capturam exceções para **não falhar a operação principal** se houver erro no log
- Gera referências automáticas para rastreabilidade (ex: `STOCK-ADJ-123-20251105120000`)
- Registra quantidade anterior, nova quantidade e diferença
- Associa o usuário responsável pela operação

### 2. StockRepository Modificado
**Localização:** `VendaFlex/Data/Repositories/StockRepository.cs`

Todas as operações que modificam o estoque agora chamam automaticamente o `StockAuditService`:

#### Operações Auditadas:

1. **AddAsync**: Criação de novo estoque
   - Chama `LogStockCreationAsync`
   - Tipo: `Entry`

2. **UpdateAsync**: Atualização de estoque
   - Chama `LogQuantityChangeAsync`
   - Tipo: `Adjustment`
   - Só registra se a quantidade mudou

3. **UpdateQuantityAsync**: Alteração direta de quantidade
   - Chama `LogQuantityChangeAsync`
   - Tipo: `Adjustment`

4. **ReserveQuantityAsync**: Reserva de estoque
   - Chama `LogReserveAsync`
   - Tipo: `Exit`

5. **ReleaseReservedQuantityAsync**: Liberação de reserva
   - Chama `LogReleaseAsync`
   - Tipo: `Return`

### 3. StockMovementService Modificado
**Localização:** `VendaFlex/Core/Services/StockMovementService.cs`

Este serviço agora é **somente para consulta**:

#### Mudanças:

- **AddAsync**: Marcado como `[Obsolete]` - só deve ser usado para migração de dados
- **UpdateAsync**: Marcado como `[Obsolete]` - retorna erro informando que movimentações não podem ser alteradas
- **DeleteAsync**: Marcado como `[Obsolete]` - sempre retorna `false`
- Todos os métodos de **consulta permanecem ativos** (GetAll, GetById, GetByProduct, etc.)

### 4. StockManagementViewModel Modificado
**Localização:** `VendaFlex/ViewModels/Stock/StockManagementViewModel.cs`

A interface foi modificada para impedir criação manual de movimentações:

#### Mudanças:

- **AddMovementCommand**: Agora mostra mensagem explicativa ao invés de abrir formulário
- **SaveMovementCommand**: Desabilitado completamente
- **SaveMovementAsync**: Marcado como `[Obsolete]` e mostra mensagem informativa

### 5. Dependency Injection
**Localização:** `VendaFlex/Infrastructure/DependencyInjection.cs`

O `StockAuditService` foi registrado usando `Lazy<T>` para evitar dependências circulares:

```csharp
services.AddScoped<StockAuditService>();
services.AddScoped<Lazy<StockAuditService>>(provider => 
    new Lazy<StockAuditService>(() => provider.GetRequiredService<StockAuditService>()));
```

## Tipos de Movimentação

A tabela `StockMovement` usa o enum `StockMovementType`:

| Tipo | Valor | Quando é Usado |
|------|-------|----------------|
| `Entry` | 1 | Criação inicial, entradas de estoque, compras |
| `Exit` | 2 | Saídas de estoque, vendas, reservas |
| `Adjustment` | 3 | Ajustes manuais de quantidade |
| `Transfer` | 4 | Transferências entre locais (futuro) |
| `Return` | 5 | Devoluções, liberação de reservas |
| `Loss` | 6 | Perdas, danos (futuro) |

## Fluxo de Auditoria

### Exemplo 1: Ajuste Manual de Estoque

```
1. Usuário ajusta estoque do Produto #123 de 50 para 75 unidades
2. StockRepository.UpdateQuantityAsync é chamado
3. Quantidade anterior (50) é capturada
4. Estoque é atualizado para 75
5. StockAuditService.LogQuantityChangeAsync é chamado automaticamente
6. Movimentação é criada:
   - ProductId: 123
   - PreviousQuantity: 50
   - NewQuantity: 75
   - Quantity: 25 (diferença)
   - Type: Adjustment
   - Reference: "STOCK-ADJ-123-20251105143000"
```

### Exemplo 2: Venda (Reserva de Estoque)

```
1. Sistema processa venda de 10 unidades do Produto #456
2. StockRepository.ReserveQuantityAsync(456, 10) é chamado
3. Quantidade disponível anterior (100) é capturada
4. ReservedQuantity aumenta de 0 para 10
5. StockAuditService.LogReserveAsync é chamado automaticamente
6. Movimentação é criada:
   - ProductId: 456
   - PreviousQuantity: 100
   - NewQuantity: 90
   - Quantity: 10
   - Type: Exit
   - Reference: "STOCK-RSV-456-20251105143100"
```

## Benefícios

1. ✅ **Rastreabilidade Completa**: Todas as mudanças são registradas com usuário e timestamp
2. ✅ **Integridade de Dados**: Movimentações não podem ser alteradas ou deletadas
3. ✅ **Auditoria Confiável**: Sistema automático elimina erros humanos
4. ✅ **Histórico Completo**: Quantidade anterior e nova sempre registradas
5. ✅ **Automação**: Desenvolvedores não precisam lembrar de criar movimentações
6. ✅ **Segurança**: Usuários não podem manipular o histórico de auditoria

## Considerações Técnicas

### Performance
- As operações de auditoria são **assíncronas** e não bloqueiam a operação principal
- Exceções na auditoria são capturadas para não afetar a operação de estoque
- Usa `Lazy<T>` para evitar overhead de inicialização

### Segurança
- Métodos de criação manual marcados como `[Obsolete]` para avisos em compile-time
- ViewModels desabilitam UI de criação manual
- Service retorna erros claros se tentar criar/editar/deletar

### Manutenibilidade
- Código centralizado no `StockAuditService`
- Fácil adicionar novos tipos de auditoria
- Documentação clara em todos os métodos

## Migração de Dados Existentes

Se houver dados antigos sem movimentações, você pode:

1. Criar script de migração que usa `StockMovementService.AddAsync` (ainda disponível para este propósito)
2. Executar uma vez para criar movimentações históricas
3. Após migração, o sistema funcionará automaticamente

## Próximos Passos (Futuro)

1. Implementar tipos `Transfer` e `Loss`
2. Adicionar relatórios de auditoria na UI
3. Implementar exportação de logs de auditoria
4. Adicionar alertas de movimentações suspeitas
5. Dashboard de auditoria com gráficos

## Resumo

O sistema agora garante que **toda e qualquer** alteração no estoque seja automaticamente registrada na tabela `StockMovement`, criando um histórico de auditoria completo, confiável e imutável. Usuários não podem mais adicionar movimentações manualmente - elas são criadas pelo sistema de forma transparente e automática.
