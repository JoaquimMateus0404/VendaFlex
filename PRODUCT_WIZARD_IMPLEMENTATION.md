# Implementação do Wizard de Criação de Produtos

## 📋 Resumo

Implementado um wizard de 3 etapas para a criação completa de produtos, garantindo que todas as informações essenciais (produto, estoque inicial e data de validade) sejam capturadas durante o processo de cadastro.

## 🎯 Problema Resolvido

Antes desta implementação, ao criar um novo produto, o sistema **não solicitava**:
- ✗ Estoque inicial (quantidade e custo)
- ✗ Data de validade para produtos com controle de validade
- ✗ Número de lote
- ✗ Movimentação de entrada no histórico

Isso resultava em produtos cadastrados sem estoque e sem informações de validade, exigindo passos adicionais posteriores.

## 🔄 Solução Implementada

### Wizard de 3 Etapas

#### **Step 1: Informações Básicas do Produto**
- Identificação (código, nome, barcode, SKU)
- Categoria e fornecedor
- Status e ordem de exibição
- Preços (custo, venda, imposto, desconto)
- Configurações (controla estoque, tem validade, destaque, backorder)

**Validação:** Nome, Categoria e Preço de Venda são obrigatórios

#### **Step 2: Estoque Inicial**
- Quantidade inicial
- Custo unitário
- Observações
- Criação automática de:
  - Registro de estoque (Stock)
  - Movimentação de entrada (StockMovement com Type = Entry)

**Validação:** Quantidade inicial > 0

#### **Step 3: Data de Validade** (Condicional)
- Exibido apenas se `HasExpirationDate = true`
- Data de validade
- Número do lote
- Observações
- Aviso visual se o produto não controla validade

**Validação:** Se o produto tem validade, data e lote são obrigatórios

### Indicador de Progresso Visual

- **Step Indicator:** Numeração visual (1, 2, 3) com cores diferentes
  - Step 1: Roxo (#667eea)
  - Step 2: Verde (#43e97b)
  - Step 3: Rosa (#f093fb)
- **Progress Bar:** Barra de progresso linear (33%, 66%, 100%)
- **Conectores:** Linhas que conectam os steps e mudam de cor conforme o progresso

### Navegação

- **Botão "Anterior":** Volta para o step anterior (invisível no Step 1)
- **Botão "Próximo":** Avança para o próximo step (Steps 1 e 2)
- **Botão "Finalizar":** Completa o processo no Step 3
- **Botão "Cancelar":** Cancela e fecha o wizard

## 📁 Arquivos Modificados

### 1. ProductManagementViewModel.cs

**Propriedades Adicionadas:**
```csharp
// Controle de Steps
private int _currentStep = 1;
public int CurrentStep { get; set; }
public bool IsStep1 => CurrentStep == 1;
public bool IsStep2 => CurrentStep == 2;
public bool IsStep3 => CurrentStep == 3;
public double StepProgress => (CurrentStep / 3.0) * 100;

// Step 2: Estoque Inicial
private int _initialStockQuantity;
private decimal _initialStockCost;
private string? _initialStockNotes;

// Step 3: Validade
private DateTime _initialExpirationDate = DateTime.Now.AddMonths(6);
private string? _initialBatchNumber;
private string? _initialExpirationNotes;
```

**Comandos Adicionados:**
```csharp
public ICommand NextStepCommand { get; private set; }
public ICommand PreviousStepCommand { get; private set; }
public ICommand FinishWizardCommand { get; private set; }
```

**Métodos Implementados:**
- `NextStep()` - Avança para próximo step
- `PreviousStep()` - Volta para step anterior
- `CanGoToNextStep()` - Valida se pode avançar
- `ValidateStep1()` - Valida informações básicas
- `ValidateStep2()` - Valida estoque inicial
- `CanFinishWizard()` - Valida se pode finalizar
- `FinishWizardAsync()` - Executa criação completa
- `ResetWizard()` - Reseta wizard para estado inicial

**Fluxo de Criação (FinishWizardAsync):**
```csharp
1. Criar ProductDto e chamar _productService.AddAsync()
2. Se ControlsStock:
   - Criar StockDto
   - Criar StockMovementDto (Type = Entry)
   - Chamar _stockService.AddAsync()
   - Chamar _stockMovementService.AddAsync()
3. Se HasExpirationDate:
   - Criar ExpirationDto
   - Chamar _expirationService.AddAsync()
4. Mostrar mensagem de sucesso
5. Recarregar dados
6. Fechar wizard
7. Reset wizard
```

### 2. ProductManagementView.xaml

**Header com Progresso:**
- Título dinâmico (Novo Produto / Editar Produto)
- Indicador visual de 3 steps com números e labels
- Conectores entre steps
- Progress bar linear

**Conteúdo dos Steps:**
- Step 1: Grid 2 colunas com campos de identificação e preços
- Step 2: Grid 2 colunas com campos de estoque inicial
- Step 3: Grid 2 colunas com campos de validade (condicional)

**Avisos Contextuais:**
- Informação sobre estoque inicial (Step 2)
- Aviso se produto não tem controle de validade (Step 3)
- Informação sobre importância da validade (Step 3)

**Botões de Navegação:**
- Layout responsivo com Grid
- Visibilidade condicional baseada no step atual
- Ícones do Material Design

### 3. App.xaml

**Converter Adicionado:**
```xml
<converters:BoolToColorConverter x:Key="BoolToColorConverter"/>
```

Usado para colorir os steps ativos dinamicamente.

## 🎨 Design

### Cores Utilizadas

| Elemento | Cor | Uso |
|----------|-----|-----|
| Step 1 Active | `#667eea` (Roxo) | Informações Básicas |
| Step 2 Active | `#43e97b` (Verde) | Estoque Inicial |
| Step 3 Active | `#f093fb` (Rosa) | Data de Validade |
| Inactive | `#E2E8F0` (Cinza) | Steps inativos |
| Text Primary | `#1E293B` | Títulos |
| Text Secondary | `#475569` | Subtítulos |
| Text Muted | `#64748B` | Descrições |

### Espaçamento
- Padding do Card: 32px
- Margin entre sections: 24px
- Margin entre fields: 16px
- Grid column gap: 24px (12px cada lado)

## 🔍 Validações

### Step 1
- [x] Nome do produto obrigatório
- [x] Categoria obrigatória
- [x] Preço de venda obrigatório (> 0)

### Step 2
- [x] Quantidade inicial obrigatória (> 0)
- [x] Custo unitário obrigatório (> 0)

### Step 3
- [x] Se HasExpirationDate = true:
  - Data de validade obrigatória
  - Número do lote obrigatório
- [x] Se HasExpirationDate = false:
  - Permite finalizar sem preencher

## 🚀 Comportamento

### Criação de Novo Produto (ProductId = 0)
1. Abre wizard no Step 1
2. CurrentStep = 1
3. Wizard resetado (ResetWizard())
4. Preenche informações em cada step
5. Finaliza com FinishWizardCommand

### Edição de Produto Existente (ProductId > 0)
1. **NÃO usa wizard**
2. Exibe formulário tradicional completo
3. Salva com SaveProductCommand
4. Mantém compatibilidade com fluxo anterior

## ✅ Benefícios

1. **Completude de Dados:** Garante que produtos sejam criados com todas as informações necessárias
2. **UX Melhorada:** Processo guiado passo a passo
3. **Validação por Etapa:** Feedback imediato em cada step
4. **Visual Profissional:** Design moderno com indicadores visuais
5. **Histórico Completo:** Movimentação de entrada registrada automaticamente
6. **Flexibilidade:** Step 3 opcional se produto não tem validade
7. **Compatibilidade:** Edição de produtos mantém fluxo anterior

## 🧪 Testes Recomendados

- [ ] Criar produto simples (sem estoque, sem validade)
- [ ] Criar produto com estoque (sem validade)
- [ ] Criar produto com estoque e validade
- [ ] Testar navegação: Próximo → Anterior → Próximo
- [ ] Testar validações em cada step
- [ ] Cancelar wizard em diferentes steps
- [ ] Editar produto existente (não deve usar wizard)
- [ ] Verificar criação de StockMovement com Type correto
- [ ] Verificar criação de Expiration quando HasExpirationDate = true

## 📝 Notas Técnicas

- **DTOs usados:** ProductDto, StockDto, StockMovementDto, ExpirationDto
- **Serviços:** IProductService, IStockService, IStockMovementService, IExpirationService
- **Enum:** StockMovementType.Entry (não string "Entrada")
- **Binding:** OneWay para propriedades computadas (IsStep1/2/3, StepProgress)
- **Converters:** BoolToColorConverter para cores dinâmicas dos steps
- **Material Design:** PackIcon para ícones, Card para dialog, OutlinedTextBox para campos

## 🎓 Padrões Seguidos

- [x] MVVM Pattern
- [x] Separação de responsabilidades (View/ViewModel)
- [x] Uso de DTOs para comunicação com serviços
- [x] Async/await para operações assíncronas
- [x] ICommand com RelayCommand
- [x] ObservableCollection para binding
- [x] PropertyChanged notifications
- [x] Material Design guidelines
- [x] Validação por camadas

---

**Data de Implementação:** 2024
**Versão:** 1.0
**Status:** ✅ Completo e funcional
