# Infraestrutura de Navegação - VendaFlex

## Visão Geral

O VendaFlex utiliza um sistema de navegação centralizado baseado em `INavigationService`, que gerencia a transição entre as diferentes janelas da aplicação.

## Estrutura

### Janela Inicial: SplashView

A **SplashView** é a janela principal da aplicação e é responsável por:
- Inicializar os serviços essenciais
- Verificar status do banco de dados
- Sincronizar dados (quando aplicável)
- Exibir logs de inicialização no `ProgressText`
- Navegar para a próxima tela apropriada (Login ou Setup)

### Arquitetura de Navegação

```
Program.cs (Inicialização)
    ?
App.xaml.cs (OnStartup)
    ?
SplashView (Janela Principal)
    ?
SplashViewModel (Lógica de Inicialização)
    ?
NavigationService
    ?
LoginView / SetupView / MainView
```

## Componentes

### 1. INavigationService

Interface que define os métodos de navegação:

```csharp
public interface INavigationService
{
    void NavigateToLogin();
    void NavigateToSetup();
    void NavigateToMain();
    void CloseCurrentWindow();
    void SetCurrentWindow(Window window);
}
```

### 2. NavigationService

Implementação concreta que gerencia a navegação entre janelas:
- Registra a janela atual
- Cria e exibe novas janelas
- Resolve ViewModels do container de DI
- Registra logs de navegação

### 3. IDatabaseStatusService

Interface para obter informações sobre o status do banco de dados:
- Status de conexão do SQL Server
- Migrações aplicadas e pendentes
- Mensagens de erro

### 4. IDatabaseSyncService

Interface para sincronização de dados (legado, mantido por compatibilidade):
- `HasPendingChangesAsync()`
- `SyncToSqlServerAsync()`
- `SyncToSqliteAsync()`

**Nota:** A sincronização não é mais implementada ativamente, pois o sistema usa apenas SQL Server.

## Fluxo de Inicialização

1. **Program.cs**: Configura Serilog, serviços e inicia a aplicação WPF
2. **App.xaml.cs**: No `OnStartup`, cria a SplashView e injeta o SplashViewModel
3. **SplashViewModel**: 
   - Captura logs do Serilog
   - Exibe logs no `ProgressText`
   - Verifica configuração da empresa
   - Navega para Login (se configurado) ou Setup (se não configurado)

## Captura de Logs

O `SplashViewModel` configura um sink personalizado do Serilog para capturar logs e exibi-los no `ProgressText`:

```csharp
private void ConfigureLogCapture()
{
    var logSink = new LogEventSink(logEvent =>
    {
        Application.Current?.Dispatcher.InvokeAsync(() =>
        {
            var message = logEvent.RenderMessage();
            
            if (logEvent.Level >= LogEventLevel.Information)
            {
                _logBuffer.AppendLine($"[{logEvent.Timestamp:HH:mm:ss}] {message}");
                
                // Manter apenas as últimas 10 linhas
                var lines = _logBuffer.ToString().Split('\n')
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .TakeLast(10);
                
                _logBuffer.Clear();
                _logBuffer.AppendLine(string.Join("\n", lines));
                
                ProgressText = _logBuffer.ToString().TrimEnd();
            }
        });
    });
}
```

## Registro de Serviços

Os serviços de navegação e banco de dados são registrados no `DependencyInjection.cs`:

```csharp
// Serviços de infraestrutura
services.AddSingleton<INavigationService, NavigationService>();
services.AddScoped<IDatabaseStatusService, DatabaseStatusService>();
services.AddScoped<IDatabaseSyncService, DatabaseSyncService>();

// ViewModels
services.AddTransient<SplashViewModel>();
```

## Próximos Passos

Para adicionar novas telas ao sistema:

1. **Criar a View** (ex: `LoginView.xaml`)
2. **Criar o ViewModel** (ex: `LoginViewModel.cs`)
3. **Registrar o ViewModel** no `Program.cs`:
   ```csharp
   services.AddTransient<LoginViewModel>();
   ```
4. **Implementar a navegação** no `NavigationService`:
   ```csharp
   public void NavigateToLogin()
   {
       _logger.LogInformation("Navegando para Login");
       
       var loginView = new LoginView();
       var loginViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
       loginView.DataContext = loginViewModel;
       
       SetCurrentWindow(loginView);
       loginView.Show();
   }
   ```

## Estrutura de Pastas

```
VendaFlex/
??? Infrastructure/
?   ??? Navigation/
?   ?   ??? INavigationService.cs
?   ?   ??? NavigationService.cs
?   ??? Database/
?       ??? IDatabaseStatusService.cs
?       ??? DatabaseStatusService.cs
?       ??? IDatabaseSyncService.cs
?       ??? DatabaseSyncService.cs
??? UI/
?   ??? Views/
?       ??? Main/
?           ??? SplashView.xaml
?           ??? SplashView.xaml.cs
??? ViewModels/
?   ??? Base/
?   ?   ??? BaseViewModel.cs
?   ??? Main/
?       ??? SplashViewModel.cs
??? App.xaml
??? App.xaml.cs
??? Program.cs
```

## Observações

- **MainWindow removida**: A aplicação não usa mais uma janela principal padrão
- **SplashView como inicial**: Todas as outras janelas são criadas dinamicamente
- **Logs visíveis**: Logs de inicialização são exibidos na splash screen
- **DI completo**: Todas as janelas e ViewModels são resolvidos do container de DI
- **SQLite removido**: Sistema usa apenas SQL Server

## Configuração

### appsettings.json

```json
{
  "UI": {
    "SplashMinMilliseconds": 3000
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=VendaFlexDB;..."
  }
}
```

## Logs

Os logs são configurados no `Program.cs` e podem ser encontrados em:
- Console (durante desenvolvimento)
- Arquivo: `%LOCALAPPDATA%\VendaFlex\logs\vendaflex-YYYY-MM-DD.log`

## Tratamento de Erros

Se ocorrer um erro durante a inicialização:
1. O erro é registrado no log
2. O tempo mínimo de splash é respeitado
3. O usuário é redirecionado para a tela de Setup
4. Mensagens de erro são exibidas no `ProgressText`
