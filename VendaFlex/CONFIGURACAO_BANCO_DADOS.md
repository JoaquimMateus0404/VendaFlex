# Configura��o do Banco de Dados - VendaFlex

## Vis�o Geral

O VendaFlex foi reconfigurado para trabalhar exclusivamente com **SQL Server** como banco de dados principal.

## Altera��es Realizadas

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
- Removidas configura��es de SQLite
- Removidas configura��es de sincroniza��o (`Sync` section)
- Mantida apenas a connection string do SQL Server: `DefaultConnection`
- Simplificadas as configura��es de database

#### `DatabaseConfiguration.cs`
- Removida l�gica de dual database (SQLite/SQL Server)
- Removido enum `DatabaseProvider`
- Removidas interfaces e classes de sincroniza��o
- Configura��o simplificada apenas para SQL Server

#### `DependencyInjection.cs`
- Removidos servi�os de sincroniza��o
- Removido `DatabaseStatusService`
- Mantidos apenas os servi�os essenciais de dom�nio

#### `Program.cs`
- Removida l�gica de sincroniza��o autom�tica
- Removida detec��o de provider
- Simplificada a inicializa��o do banco de dados
- Mantido apenas o fluxo de inicializa��o do SQL Server

## Configura��o

### Connection String

Configure a connection string no `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=VendaFlexDB;Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=true;"
  }
}
```

### Configura��es de Retry e Timeout

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

## Migra��es

### Criar Nova Migra��o

```bash
dotnet ef migrations add NomeDaMigracao --project VendaFlex
```

### Aplicar Migra��es

As migra��es s�o aplicadas automaticamente na inicializa��o do aplicativo. Voc� tamb�m pode aplic�-las manualmente:

```bash
dotnet ef database update --project VendaFlex
```

### Remover �ltima Migra��o

```bash
dotnet ef migrations remove --project VendaFlex
```

## Inicializa��o do Banco de Dados

Na primeira execu��o, o sistema:

1. ? Verifica migra��es pendentes e as aplica automaticamente
2. ? Cria a configura��o inicial da empresa
3. ? Cria o usu�rio administrador padr�o
   - **Username**: `admin`
   - **Senha**: `Admin@123`
   - ?? **IMPORTANTE**: Altere a senha ap�s o primeiro login!

## Estrutura de Dados

O banco de dados inclui as seguintes tabelas principais:

### Gest�o
- `CompanyConfigs` - Configura��es da empresa
- `Users` - Usu�rios do sistema
- `Persons` - Pessoas (clientes, fornecedores, funcion�rios)
- `Privileges` - Privil�gios do sistema
- `UserPrivileges` - Privil�gios atribu�dos aos usu�rios

### Produtos e Estoque
- `Categories` - Categorias de produtos
- `Products` - Produtos
- `Stock` - Estoque de produtos
- `StockMovements` - Movimenta��es de estoque
- `Expirations` - Controle de validade
- `PriceHistory` - Hist�rico de pre�os

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

## Backup e Recupera��o

### Backup Autom�tico

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

**Solu��o**: Verifique se o SQL Server est� em execu��o e se a connection string est� correta.

```bash
# Verificar servi�os do SQL Server
Get-Service | Where-Object {$_.Name -like "*SQL*"}
```

### Erro: "Login failed for user"

**Solu��o**: Verifique as credenciais na connection string ou use `Integrated Security=true` para autentica��o Windows.

### Erro: "Migrations pending"

**Solu��o**: As migra��es s�o aplicadas automaticamente. Se persistir, aplique manualmente:

```bash
dotnet ef database update --project VendaFlex
```

## Logs

Os logs do sistema ficam em:
- Windows: `%LOCALAPPDATA%\VendaFlex\logs\`
- Exemplo: `C:\Users\SeuUsuario\AppData\Local\VendaFlex\logs\`

Os logs de banco de dados incluem:
- Inicializa��o do banco
- Aplica��o de migra��es
- Erros de conex�o
- Opera��es de seed

## Considera��es de Performance

### �ndices

O sistema cria automaticamente �ndices para:
- Chaves prim�rias
- Chaves estrangeiras
- Campos de busca frequente (Name, Code, Barcode)
- Campos de auditoria (CreatedAt, UpdatedAt)

### Connection Pooling

O SQL Server usa connection pooling automaticamente. Configure se necess�rio:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Min Pool Size=5;Max Pool Size=100;..."
  }
}
```

### Retry Logic

O sistema est� configurado para retry autom�tico em caso de falhas transit�rias:
- **Max Retry Count**: 5 tentativas
- **Max Retry Delay**: 30 segundos

## Suporte

Para problemas relacionados ao banco de dados, consulte:
1. Logs do sistema em `%LOCALAPPDATA%\VendaFlex\logs\`
2. Event Viewer do Windows
3. SQL Server Profiler (para an�lise avan�ada)
