# 🔧 Correção de Erros ao Salvar Produto

## 📋 Problemas Identificados

### 1. **DbUpdateException** - Erro ao salvar no banco de dados
```
An error occurred while saving the entity changes. See the inner exception for details.
```

### 2. **ImageSourceConverter** - Erro ao converter string vazia
```
ImageSourceConverter cannot convert from System.String
```

---

## ✅ Correções Aplicadas

### 1. Converter para Imagem (StringToImageSourceConverter)

**Problema:** 
Quando `ProductPhotoUrl` está vazio (`""`), o WPF tenta converter diretamente para `ImageSource` e falha.

**Solução:**
Criado converter personalizado que:
- Converte string vazia ou null para `null`
- Converte URL válida para `BitmapImage`
- Trata exceções de URL inválida

**Arquivo:** `Infrastructure/Converters/BooleanConverters.cs`

```csharp
public class StringToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
        {
            try
            {
                return new System.Windows.Media.Imaging.BitmapImage(
                    new Uri(stringValue, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                return null;
            }
        }
        return null;
    }
}
```

**Registrado em:** `App.xaml`
```xml
<converters:StringToImageSourceConverter x:Key="StringToImageSourceConverter"/>
```

**Usado em:** `ProductManagementView.xaml`
```xml
<Image Source="{Binding ProductPhotoUrl, Converter={StaticResource StringToImageSourceConverter}}"
       Stretch="UniformToFill"/>
```

---

### 2. Tratamento Detalhado de DbUpdateException

**Problema:**
Mensagem genérica não ajuda a identificar o problema real (chave estrangeira, constraint, etc.)

**Solução:**
Adicionado tratamento específico para `DbUpdateException` no `ProductService.AddAsync()`

**Arquivo:** `Core/Services/ProductService.cs`

#### Detecta e trata:

1. **FOREIGN KEY constraint:**
   - `FK_Products_Categories` → "Categoria inválida ou não encontrada."
   - `FK_Products_Suppliers` ou `FK_Products_People` → "Fornecedor inválido ou não encontrado."

2. **UNIQUE constraint:**
   - `Barcode` → "Código de barras já existe."
   - `SKU` → "SKU já existe."
   - `Code` → "Código já existe."

3. **Logging detalhado:**
   ```csharp
   Debug.WriteLine($"[PRODUCT SERVICE] DbUpdateException: {dbEx.Message}");
   Debug.WriteLine($"[PRODUCT SERVICE] InnerException: {dbEx.InnerException?.Message}");
   ```

---

## 🧪 Como Testar Novamente

### Teste 1: Produto com URL de Imagem Vazia

1. Criar novo produto
2. **NÃO preencher** o campo "URL da Imagem"
3. Preencher outros campos obrigatórios
4. Salvar
5. **Esperado:** ✅ Sem erro de ImageSourceConverter, produto salvo com sucesso

### Teste 2: Produto com Categoria Inválida

1. Criar novo produto
2. No console de debug do Visual Studio, verificar se CategoryId > 0
3. Se aparecer erro, deve mostrar: **"Categoria inválida ou não encontrada."**

### Teste 3: Produto com Fornecedor Inválido

1. Criar novo produto
2. No console de debug do Visual Studio, verificar se SupplierId > 0
3. Se aparecer erro, deve mostrar: **"Fornecedor inválido ou não encontrado."**

### Teste 4: Produto com Código Duplicado

1. Criar produto com código "PROD-001"
2. Tentar criar outro produto com mesmo código
3. **Esperado:** Mensagem "Código já existe." ou "Código de barras já existe."

---

## 📊 Logs de Debug Esperados

### Caso de Sucesso:
```
[WIZARD] Tentando criar produto: Asus PC
[PRODUCT SERVICE] Produto cadastrado com sucesso
[WIZARD] ✅ Produto criado com sucesso!
```

### Caso de Erro - Chave Estrangeira:
```
[WIZARD] Tentando criar produto: Asus PC
[PRODUCT SERVICE] DbUpdateException: An error occurred while saving...
[PRODUCT SERVICE] InnerException: The INSERT statement conflicted with FOREIGN KEY constraint "FK_Products_Categories_CategoryId"
[WIZARD ERROR] Erro ao criar produto: Categoria inválida ou não encontrada.
```

### Caso de Erro - Constraint Unique:
```
[WIZARD] Tentando criar produto: Asus PC
[PRODUCT SERVICE] DbUpdateException: An error occurred while saving...
[PRODUCT SERVICE] InnerException: Cannot insert duplicate key in object 'Products' with unique index 'IX_Products_Barcode'
[WIZARD ERROR] Erro ao criar produto: Código de barras já existe.
```

---

## 🔍 Próximos Passos para Diagnóstico

Se o erro **ainda persistir**, por favor:

1. **Abra a janela Output** no Visual Studio
2. **Selecione "Debug"** no dropdown
3. **Procure por:**
   - `[PRODUCT SERVICE] InnerException:`
   - `[WIZARD ERROR]`
   - `[WIZARD EXCEPTION] InnerException:`

4. **Copie a mensagem completa do InnerException**

5. **Verifique:**
   - Se CategoryId está sendo preenchido corretamente
   - Se SupplierId está sendo preenchido corretamente
   - Se a categoria existe no banco de dados
   - Se o fornecedor existe no banco de dados

---

## 📝 Checklist de Validação

Antes de salvar o produto, o wizard agora valida:

- ✅ CategoryId > 0
- ✅ SupplierId > 0
- ✅ Nome preenchido
- ✅ Preço de venda > 0

**Se alguma validação falhar, mostra mensagem antes de tentar salvar no banco.**

---

## 🗄️ Verificar no Banco de Dados

Execute estas queries no SQL Server Management Studio para verificar:

### 1. Verificar se categoria existe:
```sql
SELECT * FROM Categories WHERE CategoryId = <ID_DA_CATEGORIA>
```

### 2. Verificar se fornecedor existe:
```sql
SELECT * FROM People WHERE PersonId = <ID_DO_FORNECEDOR>
```

### 3. Verificar constraints da tabela Products:
```sql
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTableName,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumnName
FROM 
    sys.foreign_keys AS fk
    INNER JOIN sys.foreign_key_columns AS fkc 
        ON fk.object_id = fkc.constraint_object_id
WHERE 
    OBJECT_NAME(fk.parent_object_id) = 'Products'
```

---

**Data:** 2 de dezembro de 2025  
**Status:** ✅ Correções aplicadas, aguardando novo teste
