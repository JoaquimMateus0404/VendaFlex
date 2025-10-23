# Guia de Migrações - VendaFlex

## ? Problema Resolvido

O projeto foi configurado corretamente para trabalhar com Entity Framework Core Migrations e SQL Server.

### Correções Aplicadas

1. ? **appsettings.json copiado para output**
   - Adicionada configuração no `.csproj` para copiar o arquivo
   - O arquivo agora é copiado automaticamente para `bin\Debug\net8.0-windows\`

2. ? **DesignTimeDbContextFactory criado**
   - Permite que o EF Tools funcione em design-time
   - Configurado para tratar warnings como logs
   - Usa connection string do appsettings.json

3. ? **Migração inicial criada e aplicada**
   - Migração `InitialCreate` criada com sucesso
   - Banco de dados criado no SQL Server LocalDB
   - Todas as tabelas criadas com seed data

## Comandos Úteis

### Instalar EF Core Tools (se necessário)

```bash
dotnet tool install --global dotnet-ef
```

### Criar Nova Migração

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

### Aplicar Migrações ao Banco

```bash
cd VendaFlex
dotnet ef database update
```

**Aplicar até uma migração específica:**
```bash
dotnet ef database update NomeDaMigracao
```

**Reverter todas as migrações:**
```bash
dotnet ef database update 0
```

### Remover Última Migração

```bash
cd VendaFlex
dotnet ef migrations remove
```

**Forçar remoção (mesmo que aplicada):**
```bash
dotnet ef migrations remove --force
```

### Listar Migrações

```bash
cd VendaFlex
dotnet ef migrations list
```

### Gerar Script SQL

```bash
cd VendaFlex
dotnet ef migrations script
```

**Gerar script para migração específica:**
```bash
dotnet ef migrations script MigraçãoInicial MigraçãoFinal
```

**Gerar script idempotente (seguro para re-executar):**
```bash
dotnet ef migrations script --idempotent
```

### Ver Informações do DbContext

```bash
cd VendaFlex
dotnet ef dbcontext info
```

### Gerar Classes a partir do Banco (Reverse Engineering)

```bash
cd VendaFlex
dotnet ef dbcontext scaffold "Server=(localdb)\MSSQLLocalDB;Database=VendaFlexDB;Integrated Security=true" Microsoft.EntityFrameworkCore.SqlServer
```

## Estrutura de Migrações

### Localização
As migrações ficam em:
```
VendaFlex/
  ??? Migrations/
      ??? 20251023010013_InitialCreate.cs
      ??? 20251023010013_InitialCreate.Designer.cs
      ??? ApplicationDbContextModelSnapshot.cs
```

### Anatomia de uma Migração

```csharp
public partial class NomeDaMigracao : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Código para aplicar a migração
        migrationBuilder.CreateTable(...);
        migrationBuilder.AddColumn(...);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Código para reverter a migração
        migrationBuilder.DropTable(...);
        migrationBuilder.DropColumn(...);
    }
}
```

## Boas Práticas

### ? FAZER

1. **Criar migrações pequenas e focadas**
   ```bash
   dotnet ef migrations add AddProductDescription
   dotnet ef migrations add AddProductWeight
   ```

2. **Nomear migrações claramente**
   - ? `AddEmailToUser`
   - ? `CreateAuditLogTable`
   - ? `UpdateProductIndexes`
   - ? `Migration1`
   - ? `Update`
   - ? `Fix`

3. **Testar migrações antes de commitar**
   ```bash
   # Aplicar
   dotnet ef database update
   
   # Testar rollback
   dotnet ef database update PreviousMigration
   
   # Re-aplicar
   dotnet ef database update
   ```

4. **Revisar o código gerado**
   - Abrir o arquivo da migração
   - Verificar se o Up() e Down() estão corretos
   - Adicionar código customizado se necessário

5. **Criar backup antes de migrações em produção**
   ```sql
   BACKUP DATABASE VendaFlexDB TO DISK = 'C:\Backups\VendaFlexDB_PreMigration.bak'
   ```

### ? NÃO FAZER

1. **Não editar migrações já aplicadas**
   - Se a migração já foi aplicada, crie uma nova
   - Não altere o código de migrações commitadas

2. **Não usar valores dinâmicos em HasData**
   ```csharp
   // ? ERRADO
   .HasData(new Product { 
       CreatedAt = DateTime.UtcNow  // Dinâmico!
   });
   
   // ? CORRETO
   .HasData(new Product { 
       CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
   });
   ```

3. **Não commitar sem testar**
   - Sempre teste localmente
   - Verifique se o banco está no estado esperado

4. **Não fazer migrations de produção diretamente**
   - Use scripts SQL gerados
   - Teste em ambiente de staging primeiro

## Troubleshooting

### Erro: "Build failed"
**Solução:** Compile o projeto primeiro
```bash
dotnet build
dotnet ef migrations add NomeDaMigracao
```

### Erro: "No DbContext was found"
**Solução:** Certifique-se de estar no diretório correto
```bash
cd C:\Users\Duarte Gauss\source\repos\VendaFlex\VendaFlex
dotnet ef migrations add NomeDaMigracao
```

### Erro: "Unable to create a DbContext"
**Solução:** Verifique o `DesignTimeDbContextFactory.cs` e `appsettings.json`

### Erro: "PendingModelChangesWarning"
**Solução:** Já está tratado no `DesignTimeDbContextFactory`. Se persistir:
```csharp
// No ApplicationDbContext.cs
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.ConfigureWarnings(w => 
        w.Log(RelationalEventId.PendingModelChangesWarning));
}
```

### Erro: "Migration already applied"
**Solução:** Use --force para remover
```bash
dotnet ef migrations remove --force
```

### Erro: "There is already an object named..."
**Solução:** O banco já tem a tabela. Opções:
1. Dropar o banco e recriar
   ```bash
   dotnet ef database drop
   dotnet ef database update
   ```
2. Remover a migração e criar nova
3. Editar a migração para incluir verificação

## Workflow Recomendado

### Desenvolvimento Local

```bash
# 1. Fazer alterações no modelo
# Editar entidades em Data/Entities/

# 2. Criar migração
cd VendaFlex
dotnet ef migrations add DescricaoDaAlteracao

# 3. Revisar código gerado
# Abrir Migrations/YYYYMMDD_DescricaoDaAlteracao.cs

# 4. Aplicar ao banco local
dotnet ef database update

# 5. Testar a aplicação
# Executar e testar as mudanças

# 6. Testar rollback (opcional)
dotnet ef database update MigracaoAnterior
dotnet ef database update

# 7. Commitar
git add .
git commit -m "Add: DescricaoDaAlteracao migration"
```

### Deploy em Produção

```bash
# 1. Gerar script SQL
dotnet ef migrations script --idempotent -o migration.sql

# 2. Revisar o script
# Abrir migration.sql e revisar

# 3. Fazer backup do banco
# Via SQL Server Management Studio

# 4. Aplicar em staging
# Testar em ambiente de staging

# 5. Aplicar em produção
# Executar o script SQL no banco de produção
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
? **Migração Inicial**: Aplicada com sucesso  
? **Seed Data**: Carregado automaticamente  
? **EF Tools**: Instalado e funcionando  

### Próximos Passos

1. Execute a aplicação para testar
2. Faça login com `admin` / `Admin@123`
3. Altere a senha padrão
4. Configure os dados da empresa

**O projeto está pronto para desenvolvimento!** ??
