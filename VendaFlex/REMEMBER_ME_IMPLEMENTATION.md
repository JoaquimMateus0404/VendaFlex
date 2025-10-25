# Implementação de "Lembrar-me" - VendaFlex

## Resumo

Foi implementado um sistema seguro de "Lembrar-me" que armazena o nome de usuário usando o **Windows Credential Manager**, garantindo criptografia nativa do sistema operacional.

---

## Arquivos Criados

### 1. `VendaFlex\Infrastructure\Interfaces\ICredentialManager.cs`

Interface para gerenciamento seguro de credenciais.

**Métodos:**
- `SaveRememberedUsername(string)`: Salva o username de forma segura
- `GetRememberedUsername()`: Recupera o username salvo
- `ClearRememberedUsername()`: Remove o username salvo
- `HasRememberedUsername()`: Verifica se existe username salvo

### 2. `VendaFlex\Infrastructure\Services\WindowsCredentialManager.cs`

Implementação completa usando a API nativa do Windows (Advapi32.dll).

**Características:**
- ? Usa `CredWrite`, `CredRead`, `CredDelete` da API do Windows
- ? Armazena credenciais com `CRED_PERSIST.LOCAL_MACHINE`
- ? Gerenciamento adequado de memória não gerenciada
- ? Logging completo de todas as operações
- ? Tratamento de erros robusto

**Estruturas P/Invoke:**
```csharp
- CREDENTIAL struct (mapping para estrutura nativa)
- CRED_TYPE enum (tipo de credencial)
- CRED_PERSIST enum (persistência)
```

**Segurança:**
- Credenciais armazenadas criptografadas pelo Windows
- Acesso restrito ao usuário atual
- Não armazena senhas, apenas username

---

## Arquivos Modificados

### 3. `VendaFlex\ViewModels\Authentication\LoginViewModel.cs`

**Alterações:**
1. Injetado `ICredentialManager` no construtor
2. Implementado `SaveCredentials(string)`:
   - Usa `_credentialManager.SaveRememberedUsername()`
   - Logging silencioso de erros
   - Não interrompe fluxo de login em caso de erro

3. Implementado `ClearSavedCredentials()`:
   - Remove credenciais quando "Lembrar-me" está desmarcado
   - Chamado no logout

4. Implementado `LoadSavedCredentials()`:
   - Chamado no construtor automaticamente
   - Preenche Username e marca RememberMe se encontrar credencial

5. Atualizado `ExecuteLoginAsync()`:
   - Salva credenciais após login bem-sucedido (se RememberMe = true)
   - Limpa credenciais se RememberMe = false

### 4. `VendaFlex\Infrastructure\DependencyInjection.cs`

**Alteração:**
- Registrado `WindowsCredentialManager` como Singleton:
```csharp
services.AddSingleton<ICredentialManager, WindowsCredentialManager>();
```

### 5. `VendaFlex\UI\Views\Authentication\LoginView.xaml.cs` (já existente)

O método `Page_Loaded` já chama `viewModel.LoadSavedCredentials()`, garantindo que o username seja carregado ao abrir a tela.

---

## Fluxo de "Lembrar-me"

### Ao Abrir a Tela de Login:
```
1. LoginView.Page_Loaded é disparado
   ?
2. LoginViewModel.LoadSavedCredentials() é chamado (também no construtor)
   ?
3. CredentialManager.GetRememberedUsername()
   ?
4. Se encontrado:
   - Username = savedUsername
   - RememberMe = true
   - Campo username preenchido automaticamente
```

### Ao Fazer Login com "Lembrar-me" Marcado:
```
1. ExecuteLoginAsync() valida credenciais
   ?
2. Login bem-sucedido
   ?
3. SessionService.StartSession(user)
   ?
4. if (RememberMe) ? SaveCredentials(Username)
   ?
5. CredentialManager.SaveRememberedUsername(username)
   ?
6. Windows Credential Manager armazena criptografado
```

### Ao Fazer Login SEM "Lembrar-me":
```
1. ExecuteLoginAsync() valida credenciais
   ?
2. Login bem-sucedido
   ?
3. SessionService.StartSession(user)
   ?
4. if (!RememberMe) ? ClearSavedCredentials()
   ?
5. CredentialManager.ClearRememberedUsername()
   ?
6. Credencial removida do Windows
```

---

## Como Usar

### Usuário Final:
1. Na tela de login, marque "Lembrar-me"
2. Faça login normalmente
3. Na próxima vez que abrir o app, o username estará preenchido

### Verificar no Windows:
1. Abra o **Gerenciador de Credenciais do Windows**
   - Painel de Controle ? Contas de Usuário ? Gerenciador de Credenciais
2. Vá em **Credenciais do Windows**
3. Procure por: `VendaFlex_RememberedUser`

### Remover Credencial:
- Desmarque "Lembrar-me" e faça login (remove automaticamente)
- OU delete manualmente no Gerenciador de Credenciais do Windows

---

## Detalhes Técnicos

### Windows Credential Manager API

**CredWrite:**
```csharp
[DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredWriteW", CharSet = CharSet.Unicode)]
private static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] uint flags);
```
- Salva/atualiza credencial
- Unicode (wide char)
- Retorna true se bem-sucedido

**CredRead:**
```csharp
[DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredReadW", CharSet = CharSet.Unicode)]
private static extern bool CredRead(string target, CRED_TYPE type, int reservedFlag, out IntPtr credentialPtr);
```
- Lê credencial por nome
- Retorna ponteiro para estrutura CREDENTIAL
- **Importante:** Usar `CredFree` para liberar memória

**CredDelete:**
```csharp
[DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode)]
private static extern bool CredDelete(string target, CRED_TYPE type, int flags);
```
- Remove credencial
- ERROR_NOT_FOUND (1168) se não existir

**CredFree:**
```csharp
[DllImport("Advapi32.dll", SetLastError = true)]
private static extern bool CredFree([In] IntPtr cred);
```
- Libera memória alocada por CredRead
- **Crítico:** Sempre chamar após CredRead

### Gerenciamento de Memória

**SaveRememberedUsername:**
```csharp
// Alocar memória não gerenciada
byte[] credentialBytes = Encoding.Unicode.GetBytes(username);
credentialBlobPtr = Marshal.AllocHGlobal(credentialBytes.Length);
Marshal.Copy(credentialBytes, 0, credentialBlobPtr, credentialBytes.Length);

try
{
    // Usar memória...
}
finally
{
    // SEMPRE liberar
    if (credentialBlobPtr != IntPtr.Zero)
        Marshal.FreeHGlobal(credentialBlobPtr);
}
```

**GetRememberedUsername:**
```csharp
IntPtr credPtr;
bool result = CredRead(..., out credPtr);

try
{
    var credential = Marshal.PtrToStructure<CREDENTIAL>(credPtr);
    return credential.UserName;
}
finally
{
    // SEMPRE liberar
    CredFree(credPtr);
}
```

---

## Logging

Todos os eventos são logados:

**Sucesso:**
```log
[INFO] Username 'admin' salvo com sucesso no Credential Manager
[INFO] Username recuperado com sucesso do Credential Manager
[INFO] Username removido com sucesso do Credential Manager
```

**Avisos:**
```log
[WARN] Tentativa de salvar username vazio ou nulo
[WARN] Falha ao ler credencial do Credential Manager. Código de erro: 1168
```

**Erros:**
```log
[ERROR] Falha ao salvar username no Credential Manager. Código de erro: 5
[ERROR] Erro ao salvar username 'admin' no Credential Manager
```

---

## Segurança

### ? Pontos Fortes:
- **Criptografia Nativa**: Windows gerencia criptografia automaticamente
- **Acesso Restrito**: Credenciais acessíveis apenas pelo usuário atual
- **Não Armazena Senha**: Apenas username é salvo
- **Audit Trail**: Todos os acessos são logados
- **Memória Segura**: Liberação adequada de recursos

### ?? Considerações:
- Username é armazenado em texto claro no Credential Manager (mas criptografado em disco)
- Usuários com acesso administrativo podem ver credenciais de outros usuários
- Em ambientes corporativos, pode ser controlado por GPO

### ?? O Que NÃO É Armazenado:
- ? Senha (nunca armazenada)
- ? Token de sessão
- ? Dados pessoais
- ? Apenas Username

---

## Testes

### Teste Manual:

1. **Salvar Credencial:**
   ```
   1. Abrir login
   2. Digitar username
   3. Marcar "Lembrar-me"
   4. Fazer login
   5. Verificar log: "Username 'xxx' salvo com sucesso"
   ```

2. **Carregar Credencial:**
   ```
   1. Fechar aplicação
   2. Reabrir
   3. Verificar que username está preenchido
   4. Verificar que "Lembrar-me" está marcado
   ```

3. **Remover Credencial:**
   ```
   1. Desmarcar "Lembrar-me"
   2. Fazer login
   3. Fechar e reabrir app
   4. Verificar que username está vazio
   ```

### Teste no Credential Manager:

```powershell
# Verificar credencial
cmdkey /list | Select-String "VendaFlex"

# Remover manualmente
cmdkey /delete:VendaFlex_RememberedUser
```

---

## Compatibilidade

| Plataforma | Suporte | Observações |
|------------|---------|-------------|
| Windows 10 | ? | Totalmente suportado |
| Windows 11 | ? | Totalmente suportado |
| Windows Server 2016+ | ? | Totalmente suportado |
| Linux | ? | API específica do Windows |
| macOS | ? | API específica do Windows |

**Nota:** Para cross-platform, seria necessário usar:
- Linux: `libsecret` ou `gnome-keyring`
- macOS: `Keychain Services`

---

## Melhorias Futuras

1. **Suporte Cross-Platform:**
   - Interface abstrata já existe (`ICredentialManager`)
   - Implementar `LinuxCredentialManager` usando `libsecret`
   - Implementar `MacCredentialManager` usando `Keychain`

2. **Expiração de Credencial:**
   - Adicionar timestamp no Comment
   - Limpar credenciais antigas (ex: após 30 dias)

3. **Múltiplos Usuários:**
   - Armazenar lista de últimos 5 usuários
   - ComboBox para seleção rápida

4. **Biometria:**
   - Integrar Windows Hello
   - Autenticação por impressão digital/face

---

## Troubleshooting

### Problema: Credencial não é salva

**Possíveis Causas:**
1. Usuário sem permissão para acessar Credential Manager
2. Política de grupo bloqueando armazenamento
3. Antivírus bloqueando acesso ao Advapi32.dll

**Solução:**
- Verificar logs para código de erro
- Executar como administrador (teste)
- Verificar GPO: `gpedit.msc` ? Windows Settings ? Security Settings ? Local Policies

### Problema: Credencial não é carregada

**Possíveis Causas:**
1. Credencial foi removida manualmente
2. Mudança de usuário do Windows
3. Corrupção do Credential Manager

**Solução:**
- Verificar logs
- Limpar e salvar novamente
- Resetar Credential Manager (último recurso)

### Problema: Erro "Access Denied"

**Código de Erro:** 5

**Solução:**
- Verificar permissões do usuário
- Executar app como administrador
- Verificar políticas de segurança

---

## Códigos de Erro Comuns

| Código | Nome | Significado | Solução |
|--------|------|-------------|---------|
| 0 | SUCCESS | Operação bem-sucedida | - |
| 5 | ERROR_ACCESS_DENIED | Acesso negado | Verificar permissões |
| 1168 | ERROR_NOT_FOUND | Credencial não encontrada | Normal ao carregar pela primeira vez |
| 1312 | ERROR_NO_SUCH_LOGON_SESSION | Sessão inválida | Reiniciar aplicação |
| 1783 | ERROR_INVALID_ACCOUNT_NAME | Nome de conta inválido | Validar username |

---

## Referências

- [Windows Credential Management](https://docs.microsoft.com/en-us/windows/win32/secauthn/credential-management)
- [CredWrite Function](https://docs.microsoft.com/en-us/windows/win32/api/wincred/nf-wincred-credwritew)
- [CredRead Function](https://docs.microsoft.com/en-us/windows/win32/api/wincred/nf-wincred-credreadw)
- [Platform Invoke (P/Invoke)](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke)

---

## Status

? **Implementação Completa**
? **Compilação Bem-Sucedida**
? **Testes Manuais: Pendente**
? **Documentação: Completa**

---

## Checklist de Implementação

- [x] Interface `ICredentialManager` criada
- [x] Implementação `WindowsCredentialManager` criada
- [x] Integração com `LoginViewModel`
- [x] Registro no DI container
- [x] Gerenciamento de memória adequado
- [x] Logging completo
- [x] Tratamento de erros
- [x] Compilação sem erros
- [x] Documentação criada
- [ ] Testes manuais realizados
- [ ] Testes em ambiente de produção
