# Configuração do Banco de Dados - VendaFlex

## Visão Geral

O VendaFlex foi reconfigurado para trabalhar exclusivamente com **SQL Server** como banco de dados principal.

## Alterações Realizadas

### 1. Pacotes Removidos
- ? `Microsoft.EntityFrameworkCore.Sqlite` - Removido completamente

### 2. Arquivos Removidos
- ? `Infrastructure/Sync/AdvancedSyncService.cs`
- ? `Infrastructure/Sync/IAdvancedSyncService.cs`
- ? `Infrastructure/Sync/SyncConfiguration.cs`
- ? `Infrastructure/Sync/SyncResult.cs`
- ? `Infrastructure/DatabaseStatusService.cs`
- ? `Infrastructure/DatabaseInitializer.cs`
- ? `Core/Interfaces/ISyncService.cs`
- ? `Data/Entities/ISyncableEntity.cs`

### 3. Arquivos Simplificados

#### `appsettings.json`
- Removidas configurações de SQLite
- Removidas configurações de sincronização (`Sync` section)
- Mantida apenas a connection string do SQL Server: `DefaultConnection`
- Simplificadas as configurações de database

#### `DatabaseConfiguration.cs`
- Removida lógica de dual database (SQLite/SQL Server)
- Removido enum `DatabaseProvider`
- Removidas interfaces e classes de sincronização
- Configuração simplificada apenas para SQL Server

#### `DependencyInjection.cs`
- Removidos serviços de sincronização
- Removido `DatabaseStatusService`
- Mantidos apenas os serviços essenciais de domínio

#### `Program.cs`
- Removida lógica de sincronização automática
- Removida detecção de provider
- Simplificada a inicialização do banco de dados
- Mantido apenas o fluxo de inicialização do SQL Server

## Configuração

### Connection String

Configure a connection string no `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=VendaFlexDB;Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=true;"
  }
}
```

### Configurações de Retry e Timeout

```json
{
  "Database": {
    "CommandTimeout": 60,
    "EnableRetryOnFailure": true,
    "MaxRetryCount": 5,
    "MaxRetryDelay": 30
  }
}
```

## Migrações

### Criar Nova Migração

```bash
dotnet ef migrations add NomeDaMigracao --project VendaFlex
```

### Aplicar Migrações

As migrações são aplicadas automaticamente na inicialização do aplicativo. Você também pode aplicá-las manualmente:

```bash
dotnet ef database update --project VendaFlex
```

### Remover Última Migração

```bash
dotnet ef migrations remove --project VendaFlex
```

## Inicialização do Banco de Dados

Na primeira execução, o sistema:

1. ? Verifica migrações pendentes e as aplica automaticamente
2. ? Cria a configuração inicial da empresa
3. ? Cria o usuário administrador padrão
   - **Username**: `admin`
   - **Senha**: `Admin@123`
   - ?? **IMPORTANTE**: Altere a senha após o primeiro login!

## Estrutura de Dados

O banco de dados inclui as seguintes tabelas principais:

### Gestão
- `CompanyConfigs` - Configurações da empresa
- `Users` - Usuários do sistema
- `Persons` - Pessoas (clientes, fornecedores, funcionários)
- `Privileges` - Privilégios do sistema
- `UserPrivileges` - Privilégios atribuídos aos usuários

### Produtos e Estoque
- `Categories` - Categorias de produtos
- `Products` - Produtos
- `Stock` - Estoque de produtos
- `StockMovements` - Movimentações de estoque
- `Expirations` - Controle de validade
- `PriceHistory` - Histórico de preços

### Vendas
- `Invoices` - Faturas/Vendas
- `InvoiceProducts` - Produtos da fatura
- `Payments` - Pagamentos
- `PaymentTypes` - Tipos de pagamento

### Financeiro
- `Expenses` - Despesas
- `ExpenseTypes` - Tipos de despesa

### Auditoria
- `AuditLogs` - Logs de auditoria

## Backup e Recuperação

### Backup Automático

Configure no `appsettings.json`:

```json
{
  "Backup": {
    "Enabled": true,
    "AutoBackupEnabled": true,
    "BackupIntervalHours": 24,
    "BackupPath": "%LOCALAPPDATA%/VendaFlex/backups",
    "RetainedBackupDays": 30,
    "CompressBackups": true
  }
}
```

### Backup Manual do SQL Server

```bash
# Via SQL Server Management Studio (SSMS)
# Ou via comando T-SQL:
BACKUP DATABASE VendaFlexDB TO DISK = 'C:\Backups\VendaFlexDB.bak'
```

## Troubleshooting

### Erro: "Cannot open database"

**Solução**: Verifique se o SQL Server está em execução e se a connection string está correta.

```bash
# Verificar serviços do SQL Server
Get-Service | Where-Object {$_.Name -like "*SQL*"}
```

### Erro: "Login failed for user"

**Solução**: Verifique as credenciais na connection string ou use `Integrated Security=true` para autenticação Windows.

### Erro: "Migrations pending"

**Solução**: As migrações são aplicadas automaticamente. Se persistir, aplique manualmente:

```bash
dotnet ef database update --project VendaFlex
```

## Logs

Os logs do sistema ficam em:
- Windows: `%LOCALAPPDATA%\VendaFlex\logs\`
- Exemplo: `C:\Users\SeuUsuario\AppData\Local\VendaFlex\logs\`

Os logs de banco de dados incluem:
- Inicialização do banco
- Aplicação de migrações
- Erros de conexão
- Operações de seed

## Considerações de Performance

### Índices

O sistema cria automaticamente índices para:
- Chaves primárias
- Chaves estrangeiras
- Campos de busca frequente (Name, Code, Barcode)
- Campos de auditoria (CreatedAt, UpdatedAt)

### Connection Pooling

O SQL Server usa connection pooling automaticamente. Configure se necessário:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Min Pool Size=5;Max Pool Size=100;..."
  }
}
```

### Retry Logic

O sistema está configurado para retry automático em caso de falhas transitórias:
- **Max Retry Count**: 5 tentativas
- **Max Retry Delay**: 30 segundos

## Suporte

Para problemas relacionados ao banco de dados, consulte:
1. Logs do sistema em `%LOCALAPPDATA%\VendaFlex\logs\`
2. Event Viewer do Windows
3. SQL Server Profiler (para análise avançada)
