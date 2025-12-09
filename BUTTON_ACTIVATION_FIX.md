# Correção do Botão "Próximo/Salvar" Inativo

## 🐛 Problema Identificado

O botão "PRÓXIMO/SALVAR" no `PersonFormDialog` permanecia **inativo (não clicável)** mesmo após preencher todos os campos obrigatórios, tanto para funcionários quanto para clientes/fornecedores.

## 🔍 Causa Raiz

O `AsyncCommand` não estava sendo notificado para reavaliar o método `CanGoNext()` quando:
1. As propriedades mudavam (especialmente `Name` e `GeneratedUsername`)
2. O formulário era inicializado
3. O usuário navegava entre steps

## ✅ Solução Implementada

### 1. **Notificação na Propriedade `Name`**
```csharp
public string Name
{
    get => _name;
    set
    {
        if (Set(ref _name, value))
        {
            GenerateUsername();
            ((AsyncCommand)NextStepCommand).RaiseCanExecuteChanged(); // ✅ Adicionado
        }
    }
}
```

### 2. **Notificação na Propriedade `Type`**
```csharp
public PersonType Type
{
    get => _type;
    set
    {
        if (Set(ref _type, value))
        {
            OnPropertyChanged(nameof(IsEmployee));
            OnPropertyChanged(nameof(IsCustomer));
            OnPropertyChanged(nameof(IsSupplier));
            OnPropertyChanged(nameof(WindowSubtitle));
            OnPropertyChanged(nameof(NextButtonText));
            ((AsyncCommand)NextStepCommand).RaiseCanExecuteChanged(); // ✅ Adicionado
            if (value == PersonType.Employee && _allPrivileges.Count == 0)
            {
                _ = LoadPrivilegesAsync();
            }
        }
    }
}
```

### 3. **Notificação na Propriedade `GeneratedUsername`**
```csharp
public string GeneratedUsername
{
    get => _generatedUsername;
    set
    {
        if (Set(ref _generatedUsername, value))
        {
            ((AsyncCommand)NextStepCommand).RaiseCanExecuteChanged(); // ✅ Adicionado
        }
    }
}
```

### 4. **Notificação Após Carregar Dados (Modo Edição)**
```csharp
// Carregar dados se for edição
if (personToEdit != null)
{
    _isEditMode = true;
    _personId = personToEdit.PersonId;
    LoadPersonData(personToEdit);
    
    // Notificar que o comando pode ser executado
    ((AsyncCommand)NextStepCommand).RaiseCanExecuteChanged(); // ✅ Adicionado
}
```

### 5. **Reavaliação Inicial Forçada**
```csharp
// Forçar reavaliação inicial do comando após construção
Task.Run(async () =>
{
    await Task.Delay(100); // Pequeno delay para garantir que a UI está pronta
    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
    {
        ((AsyncCommand)NextStepCommand).RaiseCanExecuteChanged();
    });
});
```

### 6. **Notificação ao Navegar Entre Steps**
```csharp
private async Task NextStepAsync()
{
    if (!IsEmployee)
    {
        await SaveAsync();
        return;
    }

    if (CurrentStep < 3)
    {
        CurrentStep++;
        ((AsyncCommand)NextStepCommand).RaiseCanExecuteChanged(); // ✅ Adicionado
    }
    else
    {
        await SaveAsync();
    }
}

private void PreviousStep()
{
    if (CurrentStep > 1)
    {
        CurrentStep--;
        ((AsyncCommand)NextStepCommand).RaiseCanExecuteChanged(); // ✅ Adicionado
    }
}
```

## 📋 Validação do Método `CanGoNext()`

O método já estava correto, apenas precisava ser chamado:

```csharp
private bool CanGoNext()
{
    if (IsLoading) return false;

    switch (CurrentStep)
    {
        case 1:
            return !string.IsNullOrWhiteSpace(Name); // Valida nome obrigatório
        case 2:
            return !string.IsNullOrWhiteSpace(GeneratedUsername); // Valida username
        case 3:
            return true; // Privilégios são opcionais
        default:
            return false;
    }
}
```

## 🎯 Cenários Testados

### ✅ Cenário 1: Cliente/Fornecedor (Novo)
1. Abrir modal
2. Digitar nome
3. ✅ Botão "SALVAR" fica ativo imediatamente

### ✅ Cenário 2: Cliente/Fornecedor (Edição)
1. Abrir modal com dados existentes
2. ✅ Botão "SALVAR" já está ativo ao abrir

### ✅ Cenário 3: Funcionário (Novo) - Step 1
1. Abrir modal
2. Selecionar tipo "Funcionário"
3. Digitar nome
4. ✅ Botão "PRÓXIMO" fica ativo

### ✅ Cenário 4: Funcionário (Novo) - Step 2
1. Avançar para Step 2
2. Username gerado automaticamente
3. ✅ Botão "PRÓXIMO" está ativo

### ✅ Cenário 5: Funcionário (Novo) - Step 3
1. Avançar para Step 3
2. ✅ Botão "FINALIZAR" está ativo (privilégios opcionais)

### ✅ Cenário 6: Funcionário (Edição)
1. Abrir modal com funcionário existente
2. ✅ Botão "PRÓXIMO" já está ativo

## 🔧 Detalhes Técnicos

### Por que `RaiseCanExecuteChanged()`?
O `AsyncCommand` mantém um cache do estado `CanExecute`. Quando propriedades mudam, precisamos notificar o comando para reavaliar esse estado.

### Por que o `Task.Run` com Delay?
Para garantir que a UI esteja completamente inicializada antes de reavaliar o comando. Isso evita problemas de timing onde o binding ainda não está pronto.

### Por que notificar em múltiplos lugares?
Para cobrir todos os cenários possíveis:
- **Digitação**: Quando o usuário digita
- **Inicialização**: Quando o formulário abre
- **Navegação**: Quando muda de step
- **Edição**: Quando carrega dados existentes

## 📊 Impacto

| Situação | Antes | Depois |
|----------|-------|--------|
| **Digite Nome** | ❌ Botão inativo | ✅ Botão fica ativo |
| **Abrir Edição** | ❌ Botão inativo | ✅ Botão ativo |
| **Mude Step** | ❌ Botão inativo | ✅ Botão atualizado |
| **Username Gerado** | ❌ Botão inativo | ✅ Botão fica ativo |

## ✨ Resultado

O botão "PRÓXIMO/SALVAR" agora **funciona corretamente** em todos os cenários:
- ✅ Responde instantaneamente ao digitar
- ✅ Estado correto ao abrir o modal
- ✅ Atualiza ao mudar tipo de pessoa
- ✅ Funciona em modo criação e edição
- ✅ Navega corretamente entre steps

---

**Data:** 2025-12-09  
**Arquivo:** `PersonFormDialogViewModel.cs`  
**Alterações:** 7 pontos de notificação adicionados  
**Status:** ✅ Resolvido e Testado

