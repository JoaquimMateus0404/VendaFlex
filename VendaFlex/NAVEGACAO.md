# Infraestrutura de Navega��o - VendaFlex

## Vis�o Geral

O VendaFlex utiliza um sistema de navega��o centralizado baseado em `INavigationService`, que gerencia a transi��o entre as diferentes janelas da aplica��o.

## Estrutura

### Janela Inicial: SplashView

A **SplashView** � a janela principal da aplica��o e � respons�vel por:
- Inicializar os servi�os essenciais
- Verificar status do banco de dados
- Sincronizar dados (quando aplic�vel)
- Exibir logs de inicializa��o no `ProgressText`
- Navegar para a pr�xima tela apropriada (Login ou Setup)

### Arquitetura de Navega��o

```
Program.cs (Inicializa��o)
    ?
App.xaml.cs (OnStartup)
    ?
SplashView (Janela Principal)
    ?
SplashViewModel (L�gica de Inicializa��o)
    ?
NavigationService
    ?
LoginView / SetupView / MainView
```

## Componentes

### 1. INavigationService

Interface que define os m�todos de navega��o:

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

Implementa��o concreta que gerencia a navega��o entre janelas:
- Registra a janela atual
- Cria e exibe novas janelas
- Resolve ViewModels do container de DI
- Registra logs de navega��o

### 3. IDatabaseStatusService

Interface para obter informa��es sobre o status do banco de dados:
- Status de conex�o do SQL Server
- Migra��es aplicadas e pendentes
- Mensagens de erro

### 4. IDatabaseSyncService

Interface para sincroniza��o de dados (legado, mantido por compatibilidade):
- `HasPendingChangesAsync()`
- `SyncToSqlServerAsync()`
- `SyncToSqliteAsync()`

**Nota:** A sincroniza��o n�o � mais implementada ativamente, pois o sistema usa apenas SQL Server.

## Fluxo de Inicializa��o

1. **Program.cs**: Configura Serilog, servi�os e inicia a aplica��o WPF
2. **App.xaml.cs**: No `OnStartup`, cria a SplashView e injeta o SplashViewModel
3. **SplashViewModel**: 
   - Captura logs do Serilog
   - Exibe logs no `ProgressText`
   - Verifica configura��o da empresa
   - Navega para Login (se configurado) ou Setup (se n�o configurado)

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
                
                // Manter apenas as �ltimas 10 linhas
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

## Registro de Servi�os

Os servi�os de navega��o e banco de dados s�o registrados no `DependencyInjection.cs`:

```csharp
// Servi�os de infraestrutura
services.AddSingleton<INavigationService, NavigationService>();
services.AddScoped<IDatabaseStatusService, DatabaseStatusService>();
services.AddScoped<IDatabaseSyncService, DatabaseSyncService>();

// ViewModels
services.AddTransient<SplashViewModel>();
```

## Pr�ximos Passos

Para adicionar novas telas ao sistema:

1. **Criar a View** (ex: `LoginView.xaml`)
2. **Criar o ViewModel** (ex: `LoginViewModel.cs`)
3. **Registrar o ViewModel** no `Program.cs`:
   ```csharp
   services.AddTransient<LoginViewModel>();
   ```
4. **Implementar a navega��o** no `NavigationService`:
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

## Observa��es

- **MainWindow removida**: A aplica��o n�o usa mais uma janela principal padr�o
- **SplashView como inicial**: Todas as outras janelas s�o criadas dinamicamente
- **Logs vis�veis**: Logs de inicializa��o s�o exibidos na splash screen
- **DI completo**: Todas as janelas e ViewModels s�o resolvidos do container de DI
- **SQLite removido**: Sistema usa apenas SQL Server

## Configura��o

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

Os logs s�o configurados no `Program.cs` e podem ser encontrados em:
- Console (durante desenvolvimento)
- Arquivo: `%LOCALAPPDATA%\VendaFlex\logs\vendaflex-YYYY-MM-DD.log`

## Tratamento de Erros

Se ocorrer um erro durante a inicializa��o:
1. O erro � registrado no log
2. O tempo m�nimo de splash � respeitado
3. O usu�rio � redirecionado para a tela de Setup
4. Mensagens de erro s�o exibidas no `ProgressText`
