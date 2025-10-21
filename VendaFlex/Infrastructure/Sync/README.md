# Sistema de Sincroniza��o VendaFlex

## Vis�o Geral

O VendaFlex implementa um sistema robusto de sincroniza��o bidirecional entre SQL Server (servidor principal) e SQLite (banco local offline), permitindo que o sistema funcione mesmo sem conex�o com o servidor.

## Arquitetura

### Componentes Principais

1. **IAdvancedSyncService** - Interface principal para sincroniza��o avan�ada
2. **AdvancedSyncService** - Implementa��o completa com controle de conflitos
3. **SyncConfiguration** - Configura��es personaliz�veis de sincroniza��o
4. **SyncResult** - Resultado detalhado de opera��es de sincroniza��o
5. **ISyncableEntity** - Interface para entidades que suportam sincroniza��o

### Fluxo de Sincroniza��o

```
???????????????           ????????????????           ???????????????
?   SQLite    ? ??????????? Sync Service ? ??????????? SQL Server  ?
?  (Offline)  ?           ?              ?           ?  (Online)   ?
???????????????           ????????????????           ???????????????
      ?                          ?                          ?
 Dados Locais         Controle de Conflitos         Dados Centralizados
```

## Recursos

### ? Funcionalidades Implementadas

- **Sincroniza��o Bidirecional** - Dados fluem em ambas as dire��es
- **Detec��o de Conflitos** - Identifica quando o mesmo registro foi modificado em ambos os lados
- **Resolu��o Autom�tica de Conflitos** - Estrat�gias configur�veis (ServerWins, ClientWins, LastWriteWins, etc.)
- **Versionamento** - Controle de vers�o de entidades para Optimistic Concurrency
- **Sincroniza��o em Lote** - Processa registros em lotes configur�veis
- **Retry Policy** - Tenta novamente em caso de falhas transit�rias
- **Logs Detalhados** - Rastreamento completo de todas as opera��es
- **Hist�rico de Sincroniza��o** - Mant�m registro de todas as sincroniza��es
- **Sincroniza��o Autom�tica** - Pode sincronizar automaticamente ao iniciar o app
- **Timeout Configur�vel** - Evita travamentos em conex�es lentas

### ?? Melhorias Implementadas

Comparado ao sistema anterior:

1. **Controle de Conflitos Robusto** - Detecta e resolve conflitos automaticamente
2. **Versionamento de Entidades** - Cada registro tem uma vers�o para detec��o de mudan�as
3. **Hash de Dados** - Detec��o r�pida de mudan�as sem comparar todos os campos
4. **Timestamps UTC** - Todas as datas em UTC para evitar problemas de fuso hor�rio
5. **GUID Global** - Identificadores �nicos que funcionam mesmo offline
6. **Estat�sticas Detalhadas** - M�tricas completas de cada sincroniza��o
7. **Tratamento de Erros Avan�ado** - Erros categorizados e tratados adequadamente
8. **Sincroniza��o Seletiva** - Sincroniza apenas dados dos �ltimos N dias (configur�vel)

## Configura��o

### appsettings.json

```json
{
  "Sync": {
    "EnableAutoSync": true,
    "AutoSyncIntervalMinutes": 30,
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 5,
    "BatchSize": 100,
    "TimeoutSeconds": 300,
    "ConflictResolution": "ServerWins",
    "SyncLastDaysOnly": 30,
    "EnableCompression": false,
    "SyncAttachments": true,
    "Mode": "Bidirectional",
    "SyncOnStartup": true
  }
}
```

### Estrat�gias de Resolu��o de Conflitos

- **ServerWins** - O servidor sempre prevalece (padr�o)
- **ClientWins** - O cliente sempre prevalece
- **LastWriteWins** - A modifica��o mais recente prevalece
- **HighestVersionWins** - A vers�o com maior n�mero prevalece
- **ManualResolution** - Requer interven��o do usu�rio

### Modos de Sincroniza��o

- **Bidirectional** - Sincroniza��o completa em ambas as dire��es (padr�o)
- **UploadOnly** - Apenas envia dados locais para o servidor
- **DownloadOnly** - Apenas baixa dados do servidor

## Uso

### 1. Inje��o de Depend�ncia

O servi�o j� est� registrado automaticamente no container de DI:

```csharp
// Em DependencyInjection.cs
services.AddSingleton<IAdvancedSyncService, AdvancedSyncService>();
```

### 2. Usar no C�digo

```csharp
public class MyViewModel
{
    private readonly IAdvancedSyncService _syncService;

    public MyViewModel(IAdvancedSyncService syncService)
    {
        _syncService = syncService;
    }

    // Sincronizar para o servidor
    public async Task SyncToServerAsync()
    {
        var result = await _syncService.SyncToServerAsync();
        
        if (result.Success)
        {
            Console.WriteLine($"Sincronizado: {result.Statistics.TotalRecordsProcessed} registros");
        }
        else
        {
            Console.WriteLine($"Erro: {result.Message}");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - {error.EntityType}: {error.ErrorMessage}");
            }
        }
    }

    // Sincronizar do servidor
    public async Task SyncFromServerAsync()
    {
        var result = await _syncService.SyncToClientAsync();
        // Processar resultado...
    }

    // Sincroniza��o bidirecional
    public async Task FullSyncAsync()
    {
        var result = await _syncService.SyncBidirectionalAsync();
        // Processar resultado...
    }

    // Verificar se h� mudan�as pendentes
    public async Task<bool> HasPendingChangesAsync()
    {
        return await _syncService.HasPendingChangesAsync();
    }

    // Obter informa��es detalhadas sobre mudan�as pendentes
    public async Task<SyncPendingInfo> GetPendingInfoAsync()
    {
        return await _syncService.GetPendingChangesInfoAsync();
    }
}
```

### 3. Sincroniza��o Autom�tica ao Iniciar

A sincroniza��o autom�tica j� est� configurada no `Program.cs`:

```csharp
private static async Task PerformInitialSync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var syncService = scope.ServiceProvider.GetRequiredService<IDatabaseSyncService>();

    var syncOnStartup = configuration.GetValue<bool>("Sync:SyncOnStartup", true);
    if (!syncOnStartup)
    {
        Log.Information("Sincroniza��o autom�tica desabilitada");
        return;
    }

    try
    {
        Log.Information("Verificando necessidade de sincroniza��o...");
        var hasPendingChanges = await syncService.HasPendingChangesAsync();
        
        if (hasPendingChanges)
        {
            Log.Information("Sincronizando dados pendentes...");
            await syncService.SyncToSqlServerAsync();
            Log.Information("Sincroniza��o conclu�da com sucesso");
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Erro ao realizar sincroniza��o inicial (continuando normalmente)");
    }
}
```

## Implementando Entidades Sincroniz�veis

Para que uma entidade suporte sincroniza��o avan�ada, implemente `ISyncableEntity`:

```csharp
public class MinhaEntidade : AuditableEntity, ISyncableEntity
{
    // Propriedades da entidade...
    
    // Propriedades de sincroniza��o
    public DateTime LastModifiedUtc { get; set; }
    public int Version { get; set; }
    public bool IsPendingSync { get; set; }
    public DateTime? LastSyncedUtc { get; set; }
    public Guid SyncGuid { get; set; } = Guid.NewGuid();
    public DataSource DataSource { get; set; }
    public string? DataHash { get; set; }
}
```

### Configura��o no DbContext

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configurar �ndices para melhor performance de sincroniza��o
    modelBuilder.Entity<MinhaEntidade>()
        .HasIndex(e => e.SyncGuid)
        .IsUnique();
        
    modelBuilder.Entity<MinhaEntidade>()
        .HasIndex(e => new { e.IsPendingSync, e.LastModifiedUtc });
}
```

## Monitoramento e Diagn�stico

### Logs

Todos os logs de sincroniza��o s�o gravados com o prefixo `[SYNC]`:

```
[SYNC?SERVER] Iniciando sincroniza��o para SQL Server...
[SYNC?SERVER] Invoice: 10 inser��es, 5 atualiza��es
[SYNC?SERVER] Conclu�da: 15 inser��es, 10 atualiza��es, 0 erros
```

### Estat�sticas

O `SyncResult` fornece estat�sticas detalhadas:

```csharp
var result = await syncService.SyncToServerAsync();

Console.WriteLine($"Processados: {result.Statistics.TotalRecordsProcessed}");
Console.WriteLine($"Inseridos: {result.Statistics.RecordsInserted}");
Console.WriteLine($"Atualizados: {result.Statistics.RecordsUpdated}");
Console.WriteLine($"Falhados: {result.Statistics.RecordsFailed}");
Console.WriteLine($"Conflitos: {result.Statistics.ConflictsResolved}");
Console.WriteLine($"Dura��o: {result.Duration}");

foreach (var entity in result.Statistics.EntitiesProcessed)
{
    Console.WriteLine($"{entity.Key}: {entity.Value} registros");
}
```

## Tratamento de Erros

### Tipos de Erros

O sistema categoriza erros em:

- **NetworkError** - Problemas de conectividade
- **DatabaseError** - Erros do banco de dados
- **ValidationError** - Dados inv�lidos
- **ConflictError** - Conflitos de sincroniza��o
- **TimeoutError** - Timeout de opera��o
- **AuthenticationError** - Falha de autentica��o

### Retry Policy

O sistema tenta novamente automaticamente em caso de erros transit�rios:

```csharp
"MaxRetryAttempts": 3,
"RetryDelaySeconds": 5  // Exponential backoff
```

## Boas Pr�ticas

### 1. Sempre Verificar Conectividade

```csharp
if (await syncService.TestServerConnectionAsync())
{
    await syncService.SyncToServerAsync();
}
else
{
    // Trabalhar offline
}
```

### 2. Sincronizar em Background

```csharp
_ = Task.Run(async () =>
{
    try
    {
        await syncService.SyncBidirectionalAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro na sincroniza��o em background");
    }
});
```

### 3. Mostrar Progresso ao Usu�rio

```csharp
var result = await syncService.SyncToServerAsync(cancellationToken);

if (result.HasConflicts)
{
    MessageBox.Show($"Sincroniza��o conclu�da com {result.Conflicts.Count} conflitos resolvidos");
}

if (result.HasErrors)
{
    MessageBox.Show($"Alguns erros ocorreram durante a sincroniza��o");
}
```

### 4. Limpar Dados Antigos

```csharp
// Executar periodicamente
await syncService.CleanupOldSyncDataAsync(daysToKeep: 30);
```

## Resolu��o de Problemas

### Problema: Sincroniza��o Lenta

**Solu��o**: Ajuste o tamanho do lote e sincronize apenas dados recentes:

```json
{
  "Sync": {
    "BatchSize": 50,
    "SyncLastDaysOnly": 7
  }
}
```

### Problema: Muitos Conflitos

**Solu��o**: Use estrat�gia `LastWriteWins` ou sincronize com mais frequ�ncia:

```json
{
  "Sync": {
    "ConflictResolution": "LastWriteWins",
    "AutoSyncIntervalMinutes": 15
  }
}
```

### Problema: Falhas de Conex�o

**Solu��o**: Aumente timeout e retry attempts:

```json
{
  "Sync": {
    "TimeoutSeconds": 600,
    "MaxRetryAttempts": 5,
    "RetryDelaySeconds": 10
  }
}
```

## Roadmap

### Funcionalidades Futuras

- [ ] Sincroniza��o incremental baseada em timestamps
- [ ] Compress�o de dados durante transfer�ncia
- [ ] Sincroniza��o de anexos e arquivos
- [ ] Interface gr�fica para resolu��o manual de conflitos
- [ ] Sincroniza��o seletiva por tipo de entidade
- [ ] M�tricas e dashboard de sincroniza��o
- [ ] Suporte para sincroniza��o via API REST
- [ ] Sincroniza��o peer-to-peer entre m�ltiplos clientes

## Suporte

Para d�vidas ou problemas, consulte:

- Logs do sistema em `%LOCALAPPDATA%\VendaFlex\logs`
- Hist�rico de sincroniza��o via `GetSyncHistoryAsync()`
- Status dos bancos via `IDatabaseStatusService`

## Licen�a

Propriedade do VendaFlex � 2025
