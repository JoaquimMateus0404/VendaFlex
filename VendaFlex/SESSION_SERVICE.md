# Integração do SessionService - VendaFlex

## Resumo das Alterações

Foi implementado um sistema completo de gerenciamento de sessão para controlar as informações do usuário logado na aplicação VendaFlex.

## Arquivos Criados

### 1. `VendaFlex\Core\Interfaces\ISessionService.cs`

Interface que define o contrato para gerenciamento de sessão com as seguintes funcionalidades:

- **Propriedades:**
  - `CurrentUser`: Usuário atualmente logado
  - `IsLoggedIn`: Verifica se há usuário logado
  - `LoginTime`: Data/hora do login
  - `LoginIpAddress`: IP do login
  - `IsAdministrator`: Verifica se o usuário é administrador

- **Métodos:**
  - `StartSession(UserDto, string?)`: Inicia uma nova sessão
  - `EndSession()`: Encerra a sessão atual
  - `UpdateCurrentUser(UserDto)`: Atualiza dados do usuário na sessão
  - `HasPrivilegeAsync(string)`: Verifica privilégios do usuário

- **Eventos:**
  - `SessionStarted`: Disparado ao iniciar sessão
  - `SessionEnded`: Disparado ao encerrar sessão

### 2. `VendaFlex\Core\Services\SessionService.cs`

Implementação completa do `ISessionService` com:

- Singleton para manter estado global da sessão
- Logging detalhado de todas as operações
- Validação de privilégios integrada com `IUserPrivilegeService`
- Verificação automática de administrador (UserId == 1 e Status == Active)
- Rastreamento de IP e tempo de sessão
- Eventos para notificar outras partes do sistema

## Arquivos Modificados

### 3. `VendaFlex\ViewModels\Authentication\LoginViewModel.cs`

**Alterações:**
- Injetado `ISessionService` no construtor
- Removidos usings desnecessários
- Método `ExecuteLoginAsync` atualizado para:
  - Validar credenciais usando `IUserService.LoginAsync`
  - Verificar status do usuário (Inactive, Suspended, Locked)
  - Iniciar sessão com `SessionService.StartSession`
  - Capturar IP local para auditoria

**Novo Método:**
- `GetLocalIpAddress()`: Obtém o IP local da máquina para registro

### 4. `VendaFlex\Infrastructure\DependencyInjection.cs`

**Alteração:**
- Registrado `SessionService` como **Singleton** no container de DI
- Posição: Logo após `INavigationService`

```csharp
services.AddSingleton<ISessionService, SessionService>();
```

## Fluxo de Login Atualizado

```
1. Usuário preenche credenciais na LoginView
   ?
2. LoginViewModel.ExecuteLoginAsync é chamado
   ?
3. Validação com UserService.LoginAsync
   ?
4. Verificações de status do usuário:
   - Status Inactive (2)
   - Status Suspended (3)
   - LockedUntil válido
   ?
5. SessionService.StartSession(user, ip)
   ?
6. Evento SessionStarted é disparado
   ?
7. Navegação para MainView
```

## Benefícios da Implementação

### 1. **Centralização de Sessão**
- Único ponto de acesso às informações do usuário logado
- Eliminação de código duplicado

### 2. **Auditoria Completa**
- Logs detalhados de todas as operações de sessão
- Rastreamento de IP e duração da sessão
- Informações para relatórios de segurança

### 3. **Segurança**
- Validação de privilégios centralizada
- Verificação automática de administrador
- Proteção contra acessos não autorizados

### 4. **Eventos**
- Permite que outras partes do sistema reajam ao login/logout
- Útil para atualizar UI, sincronizar dados, etc.

### 5. **Testabilidade**
- Interface bem definida facilita testes unitários
- Mock fácil para testes de componentes dependentes

## Como Usar o SessionService

### Verificar se há usuário logado:
```csharp
if (_sessionService.IsLoggedIn)
{
    var username = _sessionService.CurrentUser?.Username;
    // ...
}
```

### Verificar se é administrador:
```csharp
if (_sessionService.IsAdministrator)
{
    // Mostrar opções administrativas
}
```

### Verificar privilégios específicos:
```csharp
if (await _sessionService.HasPrivilegeAsync("MANAGE_PRODUCTS"))
{
    // Permitir gerenciamento de produtos
}
```

### Reagir a mudanças de sessão:
```csharp
public MyViewModel(ISessionService sessionService)
{
    sessionService.SessionStarted += OnSessionStarted;
    sessionService.SessionEnded += OnSessionEnded;
}

private void OnSessionStarted(object? sender, UserDto user)
{
    // Atualizar UI, carregar dados do usuário, etc.
}

private void OnSessionEnded(object? sender, EventArgs e)
{
    // Limpar dados, retornar ao login, etc.
}
```

### Encerrar sessão (Logout):
```csharp
_sessionService.EndSession();
_navigationService.NavigateToLogin();
```

## Próximos Passos Sugeridos

1. **Implementar Logout**: Criar botão/comando de logout que chama `EndSession()`

2. **Timeout de Sessão**: Implementar inatividade automática
   ```csharp
   // Em algum timer global
   if (_sessionService.LoginTime.HasValue && 
       DateTime.UtcNow - _sessionService.LoginTime.Value > TimeSpan.FromMinutes(30))
   {
       _sessionService.EndSession();
       // Mostrar mensagem de timeout
   }
   ```

3. **Atualizar Header/Menu**: Mostrar informações do usuário logado
   ```xaml
   <TextBlock Text="{Binding SessionService.CurrentUser.Username}"/>
   ```

4. **Auditoria de Ações**: Usar `CurrentUser` para registrar quem fez cada operação

5. **Controle de Acesso**: Usar `HasPrivilegeAsync` para habilitar/desabilitar funcionalidades

6. **MainViewModel**: Injetar `SessionService` e usar para personalizar a experiência

## Considerações de Segurança

- ? Sessão armazenada apenas em memória (não persiste)
- ? Logging de todas as operações sensíveis
- ? Validação de privilégios antes de ações críticas
- ? IP tracking para auditoria
- ?? Implementar timeout de sessão por inatividade
- ?? Considerar criptografia para "Remember Me"
- ?? Implementar proteção contra session hijacking

## Estrutura de Dados

### UserDto (já existente)
```csharp
- UserId: int
- PersonId: int
- Username: string
- Status: int (1=Active, 2=Inactive, 3=Suspended)
- LastLoginAt: DateTime?
- FailedLoginAttempts: int
- LockedUntil: DateTime?
- LastLoginIp: string
```

## Compatibilidade

- ? .NET 8
- ? C# 12
- ? WPF
- ? Entity Framework Core
- ? Dependency Injection
- ? Serilog Logging

## Compilação

Status: ? **Compilação bem-sucedida**

Todos os arquivos compilam sem erros ou warnings.
