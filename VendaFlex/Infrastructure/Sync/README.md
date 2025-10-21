# Sistema de Sincronização VendaFlex

## Visão Geral

O VendaFlex implementa um sistema robusto de sincronização bidirecional entre SQL Server (servidor principal) e SQLite (banco local offline), permitindo que o sistema funcione mesmo sem conexão com o servidor.

## Arquitetura

### Componentes Principais

1. **IAdvancedSyncService** - Interface principal para sincronização avançada
2. **AdvancedSyncService** - Implementação completa com controle de conflitos
3. **SyncConfiguration** - Configurações personalizáveis de sincronização
4. **SyncResult** - Resultado detalhado de operações de sincronização
5. **ISyncableEntity** - Interface para entidades que suportam sincronização

### Fluxo de Sincronização

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

- **Sincronização Bidirecional** - Dados fluem em ambas as direções
- **Detecção de Conflitos** - Identifica quando o mesmo registro foi modificado em ambos os lados
- **Resolução Automática de Conflitos** - Estratégias configuráveis (ServerWins, ClientWins, LastWriteWins, etc.)
- **Versionamento** - Controle de versão de entidades para Optimistic Concurrency
- **Sincronização em Lote** - Processa registros em lotes configuráveis
- **Retry Policy** - Tenta novamente em caso de falhas transitórias
- **Logs Detalhados** - Rastreamento completo de todas as operações
- **Histórico de Sincronização** - Mantém registro de todas as sincronizações
- **Sincronização Automática** - Pode sincronizar automaticamente ao iniciar o app
- **Timeout Configurável** - Evita travamentos em conexões lentas

### ?? Melhorias Implementadas

Comparado ao sistema anterior:

1. **Controle de Conflitos Robusto** - Detecta e resolve conflitos automaticamente
2. **Versionamento de Entidades** - Cada registro tem uma versão para detecção de mudanças
3. **Hash de Dados** - Detecção rápida de mudanças sem comparar todos os campos
4. **Timestamps UTC** - Todas as datas em UTC para evitar problemas de fuso horário
5. **GUID Global** - Identificadores únicos que funcionam mesmo offline
6. **Estatísticas Detalhadas** - Métricas completas de cada sincronização
7. **Tratamento de Erros Avançado** - Erros categorizados e tratados adequadamente
8. **Sincronização Seletiva** - Sincroniza apenas dados dos últimos N dias (configurável)

## Configuração

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

### Estratégias de Resolução de Conflitos

- **ServerWins** - O servidor sempre prevalece (padrão)
- **ClientWins** - O cliente sempre prevalece
- **LastWriteWins** - A modificação mais recente prevalece
- **HighestVersionWins** - A versão com maior número prevalece
- **ManualResolution** - Requer intervenção do usuário

### Modos de Sincronização

- **Bidirectional** - Sincronização completa em ambas as direções (padrão)
- **UploadOnly** - Apenas envia dados locais para o servidor
- **DownloadOnly** - Apenas baixa dados do servidor

## Uso

### 1. Injeção de Dependência

O serviço já está registrado automaticamente no container de DI:

```csharp
// Em DependencyInjection.cs
services.AddSingleton<IAdvancedSyncService, AdvancedSyncService>();
```

### 2. Usar no Código

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

    // Sincronização bidirecional
    public async Task FullSyncAsync()
    {
        var result = await _syncService.SyncBidirectionalAsync();
        // Processar resultado...
    }

    // Verificar se há mudanças pendentes
    public async Task<bool> HasPendingChangesAsync()
    {
        return await _syncService.HasPendingChangesAsync();
    }

    // Obter informações detalhadas sobre mudanças pendentes
    public async Task<SyncPendingInfo> GetPendingInfoAsync()
    {
        return await _syncService.GetPendingChangesInfoAsync();
    }
}
```

### 3. Sincronização Automática ao Iniciar

A sincronização automática já está configurada no `Program.cs`:

```csharp
private static async Task PerformInitialSync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var syncService = scope.ServiceProvider.GetRequiredService<IDatabaseSyncService>();

    var syncOnStartup = configuration.GetValue<bool>("Sync:SyncOnStartup", true);
    if (!syncOnStartup)
    {
        Log.Information("Sincronização automática desabilitada");
        return;
    }

    try
    {
        Log.Information("Verificando necessidade de sincronização...");
        var hasPendingChanges = await syncService.HasPendingChangesAsync();
        
        if (hasPendingChanges)
        {
            Log.Information("Sincronizando dados pendentes...");
            await syncService.SyncToSqlServerAsync();
            Log.Information("Sincronização concluída com sucesso");
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Erro ao realizar sincronização inicial (continuando normalmente)");
    }
}
```

## Implementando Entidades Sincronizáveis

Para que uma entidade suporte sincronização avançada, implemente `ISyncableEntity`:

```csharp
public class MinhaEntidade : AuditableEntity, ISyncableEntity
{
    // Propriedades da entidade...
    
    // Propriedades de sincronização
    public DateTime LastModifiedUtc { get; set; }
    public int Version { get; set; }
    public bool IsPendingSync { get; set; }
    public DateTime? LastSyncedUtc { get; set; }
    public Guid SyncGuid { get; set; } = Guid.NewGuid();
    public DataSource DataSource { get; set; }
    public string? DataHash { get; set; }
}
```

### Configuração no DbContext

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configurar índices para melhor performance de sincronização
    modelBuilder.Entity<MinhaEntidade>()
        .HasIndex(e => e.SyncGuid)
        .IsUnique();
        
    modelBuilder.Entity<MinhaEntidade>()
        .HasIndex(e => new { e.IsPendingSync, e.LastModifiedUtc });
}
```

## Monitoramento e Diagnóstico

### Logs

Todos os logs de sincronização são gravados com o prefixo `[SYNC]`:

```
[SYNC?SERVER] Iniciando sincronização para SQL Server...
[SYNC?SERVER] Invoice: 10 inserções, 5 atualizações
[SYNC?SERVER] Concluída: 15 inserções, 10 atualizações, 0 erros
```

### Estatísticas

O `SyncResult` fornece estatísticas detalhadas:

```csharp
var result = await syncService.SyncToServerAsync();

Console.WriteLine($"Processados: {result.Statistics.TotalRecordsProcessed}");
Console.WriteLine($"Inseridos: {result.Statistics.RecordsInserted}");
Console.WriteLine($"Atualizados: {result.Statistics.RecordsUpdated}");
Console.WriteLine($"Falhados: {result.Statistics.RecordsFailed}");
Console.WriteLine($"Conflitos: {result.Statistics.ConflictsResolved}");
Console.WriteLine($"Duração: {result.Duration}");

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
- **ValidationError** - Dados inválidos
- **ConflictError** - Conflitos de sincronização
- **TimeoutError** - Timeout de operação
- **AuthenticationError** - Falha de autenticação

### Retry Policy

O sistema tenta novamente automaticamente em caso de erros transitórios:

```csharp
"MaxRetryAttempts": 3,
"RetryDelaySeconds": 5  // Exponential backoff
```

## Boas Práticas

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
        _logger.LogError(ex, "Erro na sincronização em background");
    }
});
```

### 3. Mostrar Progresso ao Usuário

```csharp
var result = await syncService.SyncToServerAsync(cancellationToken);

if (result.HasConflicts)
{
    MessageBox.Show($"Sincronização concluída com {result.Conflicts.Count} conflitos resolvidos");
}

if (result.HasErrors)
{
    MessageBox.Show($"Alguns erros ocorreram durante a sincronização");
}
```

### 4. Limpar Dados Antigos

```csharp
// Executar periodicamente
await syncService.CleanupOldSyncDataAsync(daysToKeep: 30);
```

## Resolução de Problemas

### Problema: Sincronização Lenta

**Solução**: Ajuste o tamanho do lote e sincronize apenas dados recentes:

```json
{
  "Sync": {
    "BatchSize": 50,
    "SyncLastDaysOnly": 7
  }
}
```

### Problema: Muitos Conflitos

**Solução**: Use estratégia `LastWriteWins` ou sincronize com mais frequência:

```json
{
  "Sync": {
    "ConflictResolution": "LastWriteWins",
    "AutoSyncIntervalMinutes": 15
  }
}
```

### Problema: Falhas de Conexão

**Solução**: Aumente timeout e retry attempts:

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

- [ ] Sincronização incremental baseada em timestamps
- [ ] Compressão de dados durante transferência
- [ ] Sincronização de anexos e arquivos
- [ ] Interface gráfica para resolução manual de conflitos
- [ ] Sincronização seletiva por tipo de entidade
- [ ] Métricas e dashboard de sincronização
- [ ] Suporte para sincronização via API REST
- [ ] Sincronização peer-to-peer entre múltiplos clientes

## Suporte

Para dúvidas ou problemas, consulte:

- Logs do sistema em `%LOCALAPPDATA%\VendaFlex\logs`
- Histórico de sincronização via `GetSyncHistoryAsync()`
- Status dos bancos via `IDatabaseStatusService`

## Licença

Propriedade do VendaFlex © 2025
