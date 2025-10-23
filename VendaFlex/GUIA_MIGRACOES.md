# Guia de Migra��es - VendaFlex

## ? Problema Resolvido

O projeto foi configurado corretamente para trabalhar com Entity Framework Core Migrations e SQL Server.

### Corre��es Aplicadas

1. ? **appsettings.json copiado para output**
   - Adicionada configura��o no `.csproj` para copiar o arquivo
   - O arquivo agora � copiado automaticamente para `bin\Debug\net8.0-windows\`

2. ? **DesignTimeDbContextFactory criado**
   - Permite que o EF Tools funcione em design-time
   - Configurado para tratar warnings como logs
   - Usa connection string do appsettings.json

3. ? **Migra��o inicial criada e aplicada**
   - Migra��o `InitialCreate` criada com sucesso
   - Banco de dados criado no SQL Server LocalDB
   - Todas as tabelas criadas com seed data

## Comandos �teis

### Instalar EF Core Tools (se necess�rio)

```bash
dotnet tool install --global dotnet-ef
```

### Criar Nova Migra��o

```bash
cd VendaFlex
dotnet ef migrations add NomeDaMigracao
```

**Exemplos:**
```bash
dotnet ef migrations add AddNewFieldToProduct
dotnet ef migrations add CreateReportTable
dotnet ef migrations add UpdateInvoiceConstraints
```

### Aplicar Migra��es ao Banco

```bash
cd VendaFlex
dotnet ef database update
```

**Aplicar at� uma migra��o espec�fica:**
```bash
dotnet ef database update NomeDaMigracao
```

**Reverter todas as migra��es:**
```bash
dotnet ef database update 0
```

### Remover �ltima Migra��o

```bash
cd VendaFlex
dotnet ef migrations remove
```

**For�ar remo��o (mesmo que aplicada):**
```bash
dotnet ef migrations remove --force
```

### Listar Migra��es

```bash
cd VendaFlex
dotnet ef migrations list
```

### Gerar Script SQL

```bash
cd VendaFlex
dotnet ef migrations script
```

**Gerar script para migra��o espec�fica:**
```bash
dotnet ef migrations script Migra��oInicial Migra��oFinal
```

**Gerar script idempotente (seguro para re-executar):**
```bash
dotnet ef migrations script --idempotent
```

### Ver Informa��es do DbContext

```bash
cd VendaFlex
dotnet ef dbcontext info
```

### Gerar Classes a partir do Banco (Reverse Engineering)

```bash
cd VendaFlex
dotnet ef dbcontext scaffold "Server=(localdb)\MSSQLLocalDB;Database=VendaFlexDB;Integrated Security=true" Microsoft.EntityFrameworkCore.SqlServer
```

## Estrutura de Migra��es

### Localiza��o
As migra��es ficam em:
```
VendaFlex/
  ??? Migrations/
      ??? 20251023010013_InitialCreate.cs
      ??? 20251023010013_InitialCreate.Designer.cs
      ??? ApplicationDbContextModelSnapshot.cs
```

### Anatomia de uma Migra��o

```csharp
public partial class NomeDaMigracao : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // C�digo para aplicar a migra��o
        migrationBuilder.CreateTable(...);
        migrationBuilder.AddColumn(...);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // C�digo para reverter a migra��o
        migrationBuilder.DropTable(...);
        migrationBuilder.DropColumn(...);
    }
}
```

## Boas Pr�ticas

### ? FAZER

1. **Criar migra��es pequenas e focadas**
   ```bash
   dotnet ef migrations add AddProductDescription
   dotnet ef migrations add AddProductWeight
   ```

2. **Nomear migra��es claramente**
   - ? `AddEmailToUser`
   - ? `CreateAuditLogTable`
   - ? `UpdateProductIndexes`
   - ? `Migration1`
   - ? `Update`
   - ? `Fix`

3. **Testar migra��es antes de commitar**
   ```bash
   # Aplicar
   dotnet ef database update
   
   # Testar rollback
   dotnet ef database update PreviousMigration
   
   # Re-aplicar
   dotnet ef database update
   ```

4. **Revisar o c�digo gerado**
   - Abrir o arquivo da migra��o
   - Verificar se o Up() e Down() est�o corretos
   - Adicionar c�digo customizado se necess�rio

5. **Criar backup antes de migra��es em produ��o**
   ```sql
   BACKUP DATABASE VendaFlexDB TO DISK = 'C:\Backups\VendaFlexDB_PreMigration.bak'
   ```

### ? N�O FAZER

1. **N�o editar migra��es j� aplicadas**
   - Se a migra��o j� foi aplicada, crie uma nova
   - N�o altere o c�digo de migra��es commitadas

2. **N�o usar valores din�micos em HasData**
   ```csharp
   // ? ERRADO
   .HasData(new Product { 
       CreatedAt = DateTime.UtcNow  // Din�mico!
   });
   
   // ? CORRETO
   .HasData(new Product { 
       CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
   });
   ```

3. **N�o commitar sem testar**
   - Sempre teste localmente
   - Verifique se o banco est� no estado esperado

4. **N�o fazer migrations de produ��o diretamente**
   - Use scripts SQL gerados
   - Teste em ambiente de staging primeiro

## Troubleshooting

### Erro: "Build failed"
**Solu��o:** Compile o projeto primeiro
```bash
dotnet build
dotnet ef migrations add NomeDaMigracao
```

### Erro: "No DbContext was found"
**Solu��o:** Certifique-se de estar no diret�rio correto
```bash
cd C:\Users\Duarte Gauss\source\repos\VendaFlex\VendaFlex
dotnet ef migrations add NomeDaMigracao
```

### Erro: "Unable to create a DbContext"
**Solu��o:** Verifique o `DesignTimeDbContextFactory.cs` e `appsettings.json`

### Erro: "PendingModelChangesWarning"
**Solu��o:** J� est� tratado no `DesignTimeDbContextFactory`. Se persistir:
```csharp
// No ApplicationDbContext.cs
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.ConfigureWarnings(w => 
        w.Log(RelationalEventId.PendingModelChangesWarning));
}
```

### Erro: "Migration already applied"
**Solu��o:** Use --force para remover
```bash
dotnet ef migrations remove --force
```

### Erro: "There is already an object named..."
**Solu��o:** O banco j� tem a tabela. Op��es:
1. Dropar o banco e recriar
   ```bash
   dotnet ef database drop
   dotnet ef database update
   ```
2. Remover a migra��o e criar nova
3. Editar a migra��o para incluir verifica��o

## Workflow Recomendado

### Desenvolvimento Local

```bash
# 1. Fazer altera��es no modelo
# Editar entidades em Data/Entities/

# 2. Criar migra��o
cd VendaFlex
dotnet ef migrations add DescricaoDaAlteracao

# 3. Revisar c�digo gerado
# Abrir Migrations/YYYYMMDD_DescricaoDaAlteracao.cs

# 4. Aplicar ao banco local
dotnet ef database update

# 5. Testar a aplica��o
# Executar e testar as mudan�as

# 6. Testar rollback (opcional)
dotnet ef database update MigracaoAnterior
dotnet ef database update

# 7. Commitar
git add .
git commit -m "Add: DescricaoDaAlteracao migration"
```

### Deploy em Produ��o

```bash
# 1. Gerar script SQL
dotnet ef migrations script --idempotent -o migration.sql

# 2. Revisar o script
# Abrir migration.sql e revisar

# 3. Fazer backup do banco
# Via SQL Server Management Studio

# 4. Aplicar em staging
# Testar em ambiente de staging

# 5. Aplicar em produ��o
# Executar o script SQL no banco de produ��o
```

## Connection Strings

### LocalDB (Desenvolvimento)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=VendaFlexDB;Integrated Security=true;TrustServerCertificate=true;MultipleActiveResultSets=true;"
  }
}
```

### SQL Server Local
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=VendaFlexDB;User Id=sa;Password=SuaSenha123;TrustServerCertificate=true;MultipleActiveResultSets=true;"
  }
}
```

### SQL Server Remoto
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=servidor.exemplo.com,1433;Database=VendaFlexDB;User Id=usuario;Password=senha;TrustServerCertificate=true;MultipleActiveResultSets=true;"
  }
}
```

### Azure SQL Database
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:servidor.database.windows.net,1433;Database=VendaFlexDB;User Id=usuario@servidor;Password=senha;Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true;"
  }
}
```

## Status Atual

? **Banco de Dados**: Criado e configurado  
? **Migra��o Inicial**: Aplicada com sucesso  
? **Seed Data**: Carregado automaticamente  
? **EF Tools**: Instalado e funcionando  

### Pr�ximos Passos

1. Execute a aplica��o para testar
2. Fa�a login com `admin` / `Admin@123`
3. Altere a senha padr�o
4. Configure os dados da empresa

**O projeto est� pronto para desenvolvimento!** ??
