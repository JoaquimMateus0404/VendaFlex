# Implementação Completa do Contexto de Despesas (Expense)

## 📋 Visão Geral

Implementação profissional e completa do sistema de gestão de despesas, incluindo repositórios, serviços, validações e integração com o sistema de injeção de dependências.

## 🗂️ Arquivos Implementados

### 1. **ExpenseRepository.cs**
**Localização:** `VendaFlex/Data/Repositories/ExpenseRepository.cs`

**Responsabilidades:**
- CRUD completo de despesas
- Consultas especializadas (por tipo, usuário, status de pagamento, datas)
- Cálculos de totais (geral, pagas, pendentes, por período, por tipo)
- Paginação e busca textual

**Métodos Principais:**
- `GetByIdAsync(int id)` - Busca com includes
- `GetAllAsync()` - Todas as despesas ordenadas por data
- `AddAsync(Expense entity)` - Adiciona e recarrega com relações
- `UpdateAsync(Expense entity)` - Atualiza e recarrega
- `DeleteAsync(int id)` - Remove despesa
- `GetByExpenseTypeAsync(int expenseTypeId)` - Despesas por tipo
- `GetByUserAsync(int userId)` - Despesas por usuário
- `GetPaidAsync()` - Despesas pagas
- `GetUnpaidAsync()` - Despesas pendentes
- `GetByDateRangeAsync(DateTime startDate, DateTime endDate)` - Por período
- `GetByReferenceAsync(string reference)` - Por referência
- `GetTotalAmountAsync()` - Total geral
- `GetTotalPaidAmountAsync()` - Total pago
- `GetTotalUnpaidAmountAsync()` - Total pendente
- `GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate)` - Total por período
- `GetTotalAmountByTypeAsync(int expenseTypeId)` - Total por tipo
- `GetPagedAsync(int pageNumber, int pageSize)` - Paginação
- `SearchAsync(string searchTerm)` - Busca textual

### 2. **ExpenseTypeRepository.cs**
**Localização:** `VendaFlex/Data/Repositories/ExpenseTypeRepository.cs`

**Responsabilidades:**
- CRUD completo de tipos de despesas
- Validação de nomes únicos
- Consultas de tipos ativos
- Verificação de despesas associadas

**Métodos Principais:**
- `GetByIdAsync(int id)` - Busca por ID
- `GetAllAsync()` - Todos os tipos
- `AddAsync(ExpenseType entity)` - Adiciona tipo
- `UpdateAsync(ExpenseType entity)` - Atualiza tipo
- `DeleteAsync(int id)` - Remove tipo
- `GetActiveAsync()` - Tipos ativos
- `GetByNameAsync(string name)` - Busca por nome
- `NameExistsAsync(string name, int? excludeId = null)` - Verifica nome duplicado
- `GetExpenseCountAsync(int expenseTypeId)` - Conta despesas associadas
- `HasExpensesAsync(int expenseTypeId)` - Verifica se tem despesas
- `SearchAsync(string searchTerm)` - Busca textual

### 3. **ExpenseService.cs**
**Localização:** `VendaFlex/Core/Services/ExpenseService.cs`

**Responsabilidades:**
- Lógica de negócio para despesas
- Validação com FluentValidation
- Retornos padronizados com OperationResult
- Tratamento de erros centralizado

**Métodos Implementados:**

#### CRUD de Despesas
- `GetByIdAsync(int id)` → `OperationResult<ExpenseDto>`
- `GetAllAsync()` → `OperationResult<IEnumerable<ExpenseDto>>`
- `CreateAsync(ExpenseDto dto)` → `OperationResult<ExpenseDto>`
  - Valida dados
  - Verifica se o tipo de despesa existe e está ativo
- `UpdateAsync(ExpenseDto dto)` → `OperationResult<ExpenseDto>`
  - Valida dados
  - Verifica existência da despesa e do tipo
- `DeleteAsync(int id)` → `OperationResult`

#### Consultas de Despesas
- `GetByExpenseTypeAsync(int expenseTypeId)` → `OperationResult<IEnumerable<ExpenseDto>>`
- `GetByUserAsync(int userId)` → `OperationResult<IEnumerable<ExpenseDto>>`
- `GetPaidExpensesAsync()` → `OperationResult<IEnumerable<ExpenseDto>>`
- `GetUnpaidExpensesAsync()` → `OperationResult<IEnumerable<ExpenseDto>>`
- `GetByDateRangeAsync(DateTime startDate, DateTime endDate)` → `OperationResult<IEnumerable<ExpenseDto>>`
- `SearchAsync(string searchTerm)` → `OperationResult<IEnumerable<ExpenseDto>>`

#### Cálculos Financeiros
- `GetTotalAmountAsync()` → `OperationResult<decimal>`
- `GetTotalPaidAmountAsync()` → `OperationResult<decimal>`
- `GetTotalUnpaidAmountAsync()` → `OperationResult<decimal>`
- `GetTotalAmountByDateRangeAsync(DateTime startDate, DateTime endDate)` → `OperationResult<decimal>`

#### Operações de Status
- `MarkAsPaidAsync(int expenseId, DateTime? paidDate = null)` → `OperationResult<ExpenseDto>`
- `MarkAsUnpaidAsync(int expenseId)` → `OperationResult<ExpenseDto>`

#### Operações de Tipos de Despesas
- `GetExpenseTypesAsync()` → `OperationResult<IEnumerable<ExpenseTypeDto>>`
- `GetActiveExpenseTypesAsync()` → `OperationResult<IEnumerable<ExpenseTypeDto>>`
- `GetExpenseTypeByIdAsync(int id)` → `OperationResult<ExpenseTypeDto>`

### 4. **IExpenseService.cs**
**Localização:** `VendaFlex/Core/Interfaces/IExpenseService.cs`

**Responsabilidades:**
- Define o contrato do serviço de despesas
- Todos os métodos retornam `OperationResult` ou `OperationResult<T>`

### 5. **ExpenseDtoValidator.cs**
**Localização:** `VendaFlex/Core/DTOs/Validators/ExpenseDtoValidator.cs`

**Regras de Validação:**
- `ExpenseTypeId` - Obrigatório (> 0)
- `UserId` - Obrigatório (> 0)
- `Date` - Obrigatória, não pode ser futura
- `Value` - Obrigatório (> 0), máximo 999.999.999,99
- `Title` - Opcional, máximo 200 caracteres
- `Notes` - Opcional, máximo 1000 caracteres
- `Reference` - Opcional, máximo 100 caracteres
- `AttachmentUrl` - Opcional, máximo 500 caracteres, deve ser URL válida
- `PaidDate` - Não pode ser futura, não pode ser anterior à data da despesa
- `IsPaid` - Se true, PaidDate é obrigatória

### 6. **ExpenseTypeDtoValidator.cs**
**Localização:** `VendaFlex/Core/DTOs/Validators/ExpenseTypeDtoValidator.cs`

**Regras de Validação:**
- `Name` - Obrigatório, máximo 100 caracteres
- `Description` - Opcional, máximo 500 caracteres

## 🔧 Configurações

### DependencyInjection.cs
**Adicionado:**

```csharp
// Repositórios
services.AddScoped<ExpenseRepository>();
services.AddScoped<ExpenseTypeRepository>();

// Serviços
services.AddScoped<IExpenseService, ExpenseService>();

// Validadores
services.AddScoped<IValidator<ExpenseDto>, ExpenseDtoValidator>();
services.AddScoped<IValidator<ExpenseTypeDto>, ExpenseTypeDtoValidator>();
```

### AutoMapperProfile.cs
**Já configurado:**

```csharp
// Expense
CreateMap<Expense, ExpenseDto>();
CreateMap<ExpenseDto, Expense>()
    .ForMember(d => d.ExpenseType, o => o.Ignore())
    .ForMember(d => d.User, o => o.Ignore());

// ExpenseType
CreateMap<ExpenseType, ExpenseTypeDto>();
CreateMap<ExpenseTypeDto, ExpenseType>()
    .ForMember(d => d.Expenses, o => o.Ignore());
```

## 📊 Entidades

### Expense
**Propriedades:**
- `ExpenseId` (int, PK)
- `ExpenseTypeId` (int, FK, Required)
- `UserId` (int, FK, Required)
- `Date` (DateTime, Required)
- `Value` (decimal(18,2), Required)
- `Title` (string(200))
- `Notes` (string(1000))
- `Reference` (string(100))
- `AttachmentUrl` (string(500))
- `IsPaid` (bool, default: false)
- `PaidDate` (DateTime?)
- Propriedades de auditoria (herdadas de AuditableEntity)

**Relações:**
- `ExpenseType` (virtual)
- `User` (virtual)

### ExpenseType
**Propriedades:**
- `ExpenseTypeId` (int, PK)
- `Name` (string(100), Required)
- `Description` (string(500))
- `IsActive` (bool, default: true)

**Relações:**
- `Expenses` (ICollection<Expense>)

## ✅ Funcionalidades Implementadas

### ✔️ Repositórios
- [x] CRUD completo
- [x] Consultas especializadas
- [x] Includes automáticos para relacionamentos
- [x] NoTracking em queries de leitura
- [x] Métodos de agregação (Sum, Count)
- [x] Paginação
- [x] Busca textual

### ✔️ Serviços
- [x] Validação com FluentValidation
- [x] Retornos padronizados com OperationResult
- [x] Tratamento de exceções
- [x] Mensagens descritivas
- [x] Validações de negócio (tipo ativo, existência)
- [x] Operações de mudança de status (pago/não pago)

### ✔️ Validação
- [x] Validação de campos obrigatórios
- [x] Validação de tamanhos máximos
- [x] Validação de valores (positivos, limites)
- [x] Validação de datas (não futuras, consistência)
- [x] Validação de URLs
- [x] Validação de regras de negócio (IsPaid requer PaidDate)

### ✔️ Integração
- [x] Injeção de dependências configurada
- [x] AutoMapper configurado
- [x] Interface implementada
- [x] Documentação XML completa

## 🎯 Padrões Aplicados

1. **Repository Pattern** - Isolamento de acesso a dados
2. **Service Layer** - Lógica de negócio centralizada
3. **DTO Pattern** - Transferência de dados
4. **Dependency Injection** - Baixo acoplamento
5. **FluentValidation** - Validação declarativa
6. **Operation Result Pattern** - Respostas padronizadas
7. **Include Pattern** - Eager loading de relacionamentos
8. **AsNoTracking** - Performance em queries de leitura

## 📝 Exemplos de Uso

### Criar uma Despesa
```csharp
var expenseDto = new ExpenseDto
{
    ExpenseTypeId = 1,
    UserId = currentUserId,
    Date = DateTime.UtcNow,
    Value = 150.00m,
    Title = "Aluguel",
    Notes = "Aluguel do mês de dezembro",
    IsPaid = false
};

var result = await _expenseService.CreateAsync(expenseDto);
if (result.Success)
{
    MessageBox.Show(result.Message);
}
else
{
    MessageBox.Show(string.Join("\n", result.Errors));
}
```

### Marcar como Paga
```csharp
var result = await _expenseService.MarkAsPaidAsync(expenseId, DateTime.UtcNow);
if (result.Success)
{
    MessageBox.Show("Despesa marcada como paga!");
}
```

### Buscar Despesas Pendentes
```csharp
var result = await _expenseService.GetUnpaidExpensesAsync();
if (result.Success)
{
    var pendingExpenses = result.Data;
    // Processar lista
}
```

### Calcular Total do Mês
```csharp
var startDate = new DateTime(2025, 12, 1);
var endDate = new DateTime(2025, 12, 31);
var result = await _expenseService.GetTotalAmountByDateRangeAsync(startDate, endDate);
if (result.Success)
{
    var total = result.Data;
    MessageBox.Show($"Total do mês: {total:C}");
}
```

## 🚀 Próximos Passos Sugeridos

1. **ViewModels** - Criar ViewModels para as telas de despesas
2. **Views (XAML)** - Criar interfaces de usuário
3. **Relatórios** - Implementar relatórios de despesas
4. **Dashboard** - Adicionar widgets de despesas no dashboard
5. **Gráficos** - Visualização de despesas por categoria/período
6. **Anexos** - Implementar upload/download de comprovantes
7. **Notificações** - Alertas de despesas vencidas
8. **Exportação** - Excel/PDF de listagens

## ✨ Qualidade do Código

- ✅ Documentação XML completa
- ✅ Naming conventions seguidas
- ✅ Separação de responsabilidades
- ✅ SOLID principles
- ✅ Tratamento de erros
- ✅ Validações robustas
- ✅ Performance otimizada (AsNoTracking, includes apropriados)
- ✅ Mensagens em português
- ✅ Código limpo e legível

---

**Data de Implementação:** 09/12/2025  
**Status:** ✅ Completo e Pronto para Uso

