# Implementação do Sistema de Gestão de Faturas

## 📋 Visão Geral

A **InvoiceManagementViewModel** foi implementada para suportar todas as funcionalidades da View de gestão de faturas, incluindo listagem, filtragem, paginação, gerenciamento de pagamentos e ações diversas sobre faturas.

## 🎯 Funcionalidades Implementadas

### 1. **Listagem e Filtragem de Faturas**

#### Filtros Disponíveis:
- **Data Inicial/Final**: Filtra faturas por período
- **Número da Fatura**: Busca por número específico
- **Status**: Todos, Paga, Pendente, Cancelada
- **Nome do Cliente**: Filtra por nome do cliente
- **NIF do Cliente**: Filtra por documento fiscal
- **Operador**: Filtra por usuário que criou a fatura
- **Forma de Pagamento**: Filtra por tipo de pagamento

#### Funcionalidades:
- ✅ Pesquisa com múltiplos critérios
- ✅ Limpeza rápida de filtros
- ✅ Ordenação por Data, Número ou Total (crescente/decrescente)

### 2. **Paginação**

- **Navegação**:
  - Primeira página
  - Página anterior
  - Próxima página
  - Última página
- **Tamanho de página**: 10, 20, 50 ou 100 itens
- **Indicadores**: Mostra "Página X de Y"

### 3. **Estatísticas em Tempo Real**

Chips no cabeçalho mostram:
- 📗 **Faturas Pagas**: Total de faturas com status Pago
- 📙 **Faturas Pendentes**: Total de faturas pendentes/confirmadas
- 📕 **Faturas Canceladas**: Total de faturas canceladas

### 4. **Detalhes da Fatura**

Quando uma fatura é selecionada, exibe:

#### Cards de Resumo:
- **Total**: Valor total da fatura
- **Pago**: Valor já pago
- **Saldo**: Valor pendente
- **Itens**: Quantidade de produtos

#### Abas:

##### **Aba Pagamentos**:
- Lista todos os pagamentos da fatura
- Adicionar novo pagamento:
  - Selecionar forma de pagamento
  - Informar valor
  - Adicionar referência (opcional)
- Remover pagamentos existentes
- Validação: não permite pagamento maior que saldo

##### **Aba Cliente**:
- Avatar com iniciais do cliente
- Nome completo
- NIF (documento fiscal)
- Telefone
- E-mail
- Endereço
- Informações do operador que realizou a venda

##### **Aba Histórico**:
- Linha do tempo com todas as ações realizadas na fatura
- Ícone e descrição da ação
- Usuário responsável
- Data e hora

##### **Aba Ajustes**:
- **Desconto Pós-Venda**:
  - Valor do desconto
  - Justificativa obrigatória
- **Acréscimo Pós-Venda**:
  - Valor do acréscimo
  - Justificativa obrigatória
- **Alterar Forma de Pagamento**:
  - Selecionar pagamento a alterar
  - Selecionar nova forma de pagamento

##### **Aba Impacto**:
- Impacto no estoque (produtos vendidos)
- Impacto financeiro
- Relatórios

### 5. **Ações Rápidas**

No cabeçalho de detalhes:
- 🖨️ **Imprimir**: Imprime a fatura
- 📄 **Gerar PDF**: Exporta fatura em PDF

No menu de mais ações:
- 📋 **Duplicar Fatura**: Cria uma nova fatura com os mesmos dados
- 📂 **Reabrir Fatura**: Reabre uma fatura fechada
- 💳 **Nota de Crédito**: Emite nota de crédito
- 💳 **Nota de Débito**: Emite nota de débito
- ❌ **Cancelar Fatura**: Cancela a fatura (com confirmação)

### 6. **Exportação**

- **Exportar**: Exporta lista de faturas para CSV
- Inclui: Número, Data, Cliente, Operador, Status, Total, Pago, Saldo

### 7. **Feedback ao Usuário**

- **Loading Overlay**: Exibe progresso em operações demoradas
- **Snackbar**: Mensagens de sucesso/erro na parte inferior
- **Diálogos de Confirmação**: Para ações críticas (cancelar, remover pagamento)

## 🏗️ Arquitetura

### Serviços Utilizados:

```csharp
- IInvoiceService          // Gestão de faturas
- IPaymentService          // Gestão de pagamentos
- IPaymentTypeService      // Tipos de pagamento
- IInvoiceProductService   // Produtos da fatura
- IPersonService           // Dados de clientes
- IProductService          // Informações de produtos
- IStockService            // Controle de estoque
- ISessionService          // Sessão do usuário
- ICompanyConfigService    // Configurações da empresa
- IReceiptPrintService     // Impressão
```

### Commands Implementados:

```csharp
RefreshCommand             // Atualizar dados
ExportCommand              // Exportar para CSV
SearchCommand              // Buscar com filtros
ClearFiltersCommand        // Limpar filtros
FirstPageCommand           // Primeira página
PreviousPageCommand        // Página anterior
NextPageCommand            // Próxima página
LastPageCommand            // Última página
PrintInvoiceCommand        // Imprimir fatura
GeneratePdfCommand         // Gerar PDF
DuplicateInvoiceCommand    // Duplicar fatura
ReopenInvoiceCommand       // Reabrir fatura
IssueCreditNoteCommand     // Nota de crédito
IssueDebitNoteCommand      // Nota de débito
CancelInvoiceCommand       // Cancelar fatura
AddPaymentCommand          // Adicionar pagamento
RemovePaymentCommand       // Remover pagamento
ApplyDiscountCommand       // Aplicar desconto
ApplySurchargeCommand      // Aplicar acréscimo
ChangePaymentTypeCommand   // Alterar forma de pagamento
```

## 📦 DTOs Auxiliares Criados

### InvoiceListItemDto
DTO otimizado para listagem de faturas com dados principais e do cliente/operador.

### PaymentListItemDto
DTO para exibir pagamentos na lista, com tipo de pagamento e referência.

### InvoiceHistoryItemDto
DTO para linha do tempo de histórico com ação, ícone e usuário.

### NewPaymentDto
DTO para formulário de novo pagamento com binding bidirecional.

### StockImpactItemDto
DTO para exibir impacto da fatura no estoque.

## 🔄 Fluxo de Funcionamento

### Inicialização:
1. Carrega tipos de pagamento ativos
2. Executa busca inicial de faturas
3. Atualiza estatísticas do cabeçalho

### Seleção de Fatura:
1. Usuário seleciona fatura na lista
2. Carrega detalhes completos da fatura
3. Carrega produtos, pagamentos e histórico
4. Atualiza cards de resumo

### Adicionar Pagamento:
1. Usuário preenche formulário
2. Validações:
   - Forma de pagamento selecionada
   - Valor maior que zero
   - Valor não excede saldo
3. Registra pagamento
4. Recarrega detalhes da fatura
5. Atualiza estatísticas

### Aplicar Filtros:
1. Usuário define critérios de filtro
2. Clica em "Buscar"
3. Sistema aplica filtros e ordenação
4. Calcula paginação
5. Exibe resultados

## 🎨 Recursos Visuais

### Status de Fatura:
- ✅ **Paga**: Ícone verde CheckCircle
- 🕐 **Pendente**: Ícone laranja ClockOutline
- ❌ **Cancelada**: Ícone vermelho Cancel
- 📝 **Rascunho**: Ícone cinza FileDocumentEditOutline
- 💸 **Reembolsada**: Ícone roxo CashRefund

### Cores dos Cards:
- **Total**: Azul claro (`PrimaryHueLightBrush`)
- **Pago**: Verde claro (`#E8F5E9`)
- **Saldo**: Laranja claro (`#FFF3E0`)
- **Itens**: Azul claro (`#E3F2FD`)

## 📝 TODOs Pendentes

Funcionalidades marcadas para implementação futura:

### 1. Filtros Avançados
- Implementar filtros no backend (`IInvoiceService`)
- Adicionar paginação no servidor
- Otimizar consultas com índices

### 2. Detalhes da Fatura
- Carregar produtos da fatura (`IInvoiceProductService`)
- Implementar histórico de ações
- Calcular impacto no estoque

### 3. Ações de Fatura
- Implementar impressão real
- Geração de PDF com template
- Duplicação de fatura completa
- Reabertura com validações
- Notas de crédito/débito

### 4. Integração com Cliente/Operador
- Carregar dados completos do Person
- Carregar dados do User
- Exibir avatar personalizado

### 5. Ajustes Pós-Venda
- Implementar lógica de desconto
- Implementar lógica de acréscimo
- Validar permissões do usuário
- Registrar no histórico

### 6. Relatórios
- Relatório de impacto financeiro
- Relatório de impacto no estoque
- Exportação avançada (Excel, PDF)

## 🔒 Validações Implementadas

- ✅ Valor de pagamento não pode ser zero
- ✅ Valor de pagamento não pode exceder saldo
- ✅ Forma de pagamento deve ser selecionada
- ✅ Confirmação para ações destrutivas (cancelar, remover)
- ✅ Justificativa obrigatória para ajustes

## 🚀 Próximos Passos

1. **Implementar métodos pendentes** nos serviços
2. **Adicionar testes unitários** para a ViewModel
3. **Implementar logs de auditoria** para todas as ações
4. **Adicionar validações de permissões** por role de usuário
5. **Implementar cache** para melhorar performance
6. **Adicionar indicadores visuais** de carregamento por seção
7. **Implementar busca em tempo real** (debounce nos filtros)
8. **Adicionar exportação para Excel** com formatação
9. **Implementar impressão térmica** para recibos
10. **Adicionar notificações** para ações importantes

## 📚 Referências

- **View**: `VendaFlex\UI\Views\Sales\InvoiceManagementView.xaml`
- **ViewModel**: `VendaFlex\ViewModels\Sales\InvoiceManagementViewModel.cs`
- **Converters**: 
  - `InvoiceStatusToColorConverter.cs`
  - `InvoiceStatusToIconConverter.cs`

## ✅ Status da Implementação

- [x] Estrutura base da ViewModel
- [x] Propriedades e binding
- [x] Comandos
- [x] Filtros e busca
- [x] Paginação
- [x] Estatísticas
- [x] Detalhes da fatura
- [x] Gestão de pagamentos
- [x] Ações de fatura (estrutura)
- [x] Exportação básica
- [x] Feedback ao usuário
- [ ] Implementação completa das ações
- [ ] Integração com todos os serviços
- [ ] Testes unitários

---

**Data de Implementação**: 22 de novembro de 2025
**Desenvolvido para**: VendaFlex - Sistema de Gestão Comercial
