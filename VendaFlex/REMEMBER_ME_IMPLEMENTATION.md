# Implementa��o de "Lembrar-me" - VendaFlex

## Resumo

Foi implementado um sistema seguro de "Lembrar-me" que armazena o nome de usu�rio usando o **Windows Credential Manager**, garantindo criptografia nativa do sistema operacional.

---

## Arquivos Criados

### 1. `VendaFlex\Infrastructure\Interfaces\ICredentialManager.cs`

Interface para gerenciamento seguro de credenciais.

**M�todos:**
- `SaveRememberedUsername(string)`: Salva o username de forma segura
- `GetRememberedUsername()`: Recupera o username salvo
- `ClearRememberedUsername()`: Remove o username salvo
- `HasRememberedUsername()`: Verifica se existe username salvo

### 2. `VendaFlex\Infrastructure\Services\WindowsCredentialManager.cs`

Implementa��o completa usando a API nativa do Windows (Advapi32.dll).

**Caracter�sticas:**
- ? Usa `CredWrite`, `CredRead`, `CredDelete` da API do Windows
- ? Armazena credenciais com `CRED_PERSIST.LOCAL_MACHINE`
- ? Gerenciamento adequado de mem�ria n�o gerenciada
- ? Logging completo de todas as opera��es
- ? Tratamento de erros robusto

**Estruturas P/Invoke:**
```csharp
- CREDENTIAL struct (mapping para estrutura nativa)
- CRED_TYPE enum (tipo de credencial)
- CRED_PERSIST enum (persist�ncia)
```

**Seguran�a:**
- Credenciais armazenadas criptografadas pelo Windows
- Acesso restrito ao usu�rio atual
- N�o armazena senhas, apenas username

---

## Arquivos Modificados

### 3. `VendaFlex\ViewModels\Authentication\LoginViewModel.cs`

**Altera��es:**
1. Injetado `ICredentialManager` no construtor
2. Implementado `SaveCredentials(string)`:
   - Usa `_credentialManager.SaveRememberedUsername()`
   - Logging silencioso de erros
   - N�o interrompe fluxo de login em caso de erro

3. Implementado `ClearSavedCredentials()`:
   - Remove credenciais quando "Lembrar-me" est� desmarcado
   - Chamado no logout

4. Implementado `LoadSavedCredentials()`:
   - Chamado no construtor automaticamente
   - Preenche Username e marca RememberMe se encontrar credencial

5. Atualizado `ExecuteLoginAsync()`:
   - Salva credenciais ap�s login bem-sucedido (se RememberMe = true)
   - Limpa credenciais se RememberMe = false

### 4. `VendaFlex\Infrastructure\DependencyInjection.cs`

**Altera��o:**
- Registrado `WindowsCredentialManager` como Singleton:
```csharp
services.AddSingleton<ICredentialManager, WindowsCredentialManager>();
```

### 5. `VendaFlex\UI\Views\Authentication\LoginView.xaml.cs` (j� existente)

O m�todo `Page_Loaded` j� chama `viewModel.LoadSavedCredentials()`, garantindo que o username seja carregado ao abrir a tela.

---

## Fluxo de "Lembrar-me"

### Ao Abrir a Tela de Login:
```
1. LoginView.Page_Loaded � disparado
   ?
2. LoginViewModel.LoadSavedCredentials() � chamado (tamb�m no construtor)
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

### Usu�rio Final:
1. Na tela de login, marque "Lembrar-me"
2. Fa�a login normalmente
3. Na pr�xima vez que abrir o app, o username estar� preenchido

### Verificar no Windows:
1. Abra o **Gerenciador de Credenciais do Windows**
   - Painel de Controle ? Contas de Usu�rio ? Gerenciador de Credenciais
2. V� em **Credenciais do Windows**
3. Procure por: `VendaFlex_RememberedUser`

### Remover Credencial:
- Desmarque "Lembrar-me" e fa�a login (remove automaticamente)
- OU delete manualmente no Gerenciador de Credenciais do Windows

---

## Detalhes T�cnicos

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
- L� credencial por nome
- Retorna ponteiro para estrutura CREDENTIAL
- **Importante:** Usar `CredFree` para liberar mem�ria

**CredDelete:**
```csharp
[DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode)]
private static extern bool CredDelete(string target, CRED_TYPE type, int flags);
```
- Remove credencial
- ERROR_NOT_FOUND (1168) se n�o existir

**CredFree:**
```csharp
[DllImport("Advapi32.dll", SetLastError = true)]
private static extern bool CredFree([In] IntPtr cred);
```
- Libera mem�ria alocada por CredRead
- **Cr�tico:** Sempre chamar ap�s CredRead

### Gerenciamento de Mem�ria

**SaveRememberedUsername:**
```csharp
// Alocar mem�ria n�o gerenciada
byte[] credentialBytes = Encoding.Unicode.GetBytes(username);
credentialBlobPtr = Marshal.AllocHGlobal(credentialBytes.Length);
Marshal.Copy(credentialBytes, 0, credentialBlobPtr, credentialBytes.Length);

try
{
    // Usar mem�ria...
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

Todos os eventos s�o logados:

**Sucesso:**
```log
[INFO] Username 'admin' salvo com sucesso no Credential Manager
[INFO] Username recuperado com sucesso do Credential Manager
[INFO] Username removido com sucesso do Credential Manager
```

**Avisos:**
```log
[WARN] Tentativa de salvar username vazio ou nulo
[WARN] Falha ao ler credencial do Credential Manager. C�digo de erro: 1168
```

**Erros:**
```log
[ERROR] Falha ao salvar username no Credential Manager. C�digo de erro: 5
[ERROR] Erro ao salvar username 'admin' no Credential Manager
```

---

## Seguran�a

### ? Pontos Fortes:
- **Criptografia Nativa**: Windows gerencia criptografia automaticamente
- **Acesso Restrito**: Credenciais acess�veis apenas pelo usu�rio atual
- **N�o Armazena Senha**: Apenas username � salvo
- **Audit Trail**: Todos os acessos s�o logados
- **Mem�ria Segura**: Libera��o adequada de recursos

### ?? Considera��es:
- Username � armazenado em texto claro no Credential Manager (mas criptografado em disco)
- Usu�rios com acesso administrativo podem ver credenciais de outros usu�rios
- Em ambientes corporativos, pode ser controlado por GPO

### ?? O Que N�O � Armazenado:
- ? Senha (nunca armazenada)
- ? Token de sess�o
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
   1. Fechar aplica��o
   2. Reabrir
   3. Verificar que username est� preenchido
   4. Verificar que "Lembrar-me" est� marcado
   ```

3. **Remover Credencial:**
   ```
   1. Desmarcar "Lembrar-me"
   2. Fazer login
   3. Fechar e reabrir app
   4. Verificar que username est� vazio
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

| Plataforma | Suporte | Observa��es |
|------------|---------|-------------|
| Windows 10 | ? | Totalmente suportado |
| Windows 11 | ? | Totalmente suportado |
| Windows Server 2016+ | ? | Totalmente suportado |
| Linux | ? | API espec�fica do Windows |
| macOS | ? | API espec�fica do Windows |

**Nota:** Para cross-platform, seria necess�rio usar:
- Linux: `libsecret` ou `gnome-keyring`
- macOS: `Keychain Services`

---

## Melhorias Futuras

1. **Suporte Cross-Platform:**
   - Interface abstrata j� existe (`ICredentialManager`)
   - Implementar `LinuxCredentialManager` usando `libsecret`
   - Implementar `MacCredentialManager` usando `Keychain`

2. **Expira��o de Credencial:**
   - Adicionar timestamp no Comment
   - Limpar credenciais antigas (ex: ap�s 30 dias)

3. **M�ltiplos Usu�rios:**
   - Armazenar lista de �ltimos 5 usu�rios
   - ComboBox para sele��o r�pida

4. **Biometria:**
   - Integrar Windows Hello
   - Autentica��o por impress�o digital/face

---

## Troubleshooting

### Problema: Credencial n�o � salva

**Poss�veis Causas:**
1. Usu�rio sem permiss�o para acessar Credential Manager
2. Pol�tica de grupo bloqueando armazenamento
3. Antiv�rus bloqueando acesso ao Advapi32.dll

**Solu��o:**
- Verificar logs para c�digo de erro
- Executar como administrador (teste)
- Verificar GPO: `gpedit.msc` ? Windows Settings ? Security Settings ? Local Policies

### Problema: Credencial n�o � carregada

**Poss�veis Causas:**
1. Credencial foi removida manualmente
2. Mudan�a de usu�rio do Windows
3. Corrup��o do Credential Manager

**Solu��o:**
- Verificar logs
- Limpar e salvar novamente
- Resetar Credential Manager (�ltimo recurso)

### Problema: Erro "Access Denied"

**C�digo de Erro:** 5

**Solu��o:**
- Verificar permiss�es do usu�rio
- Executar app como administrador
- Verificar pol�ticas de seguran�a

---

## C�digos de Erro Comuns

| C�digo | Nome | Significado | Solu��o |
|--------|------|-------------|---------|
| 0 | SUCCESS | Opera��o bem-sucedida | - |
| 5 | ERROR_ACCESS_DENIED | Acesso negado | Verificar permiss�es |
| 1168 | ERROR_NOT_FOUND | Credencial n�o encontrada | Normal ao carregar pela primeira vez |
| 1312 | ERROR_NO_SUCH_LOGON_SESSION | Sess�o inv�lida | Reiniciar aplica��o |
| 1783 | ERROR_INVALID_ACCOUNT_NAME | Nome de conta inv�lido | Validar username |

---

## Refer�ncias

- [Windows Credential Management](https://docs.microsoft.com/en-us/windows/win32/secauthn/credential-management)
- [CredWrite Function](https://docs.microsoft.com/en-us/windows/win32/api/wincred/nf-wincred-credwritew)
- [CredRead Function](https://docs.microsoft.com/en-us/windows/win32/api/wincred/nf-wincred-credreadw)
- [Platform Invoke (P/Invoke)](https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke)

---

## Status

? **Implementa��o Completa**
? **Compila��o Bem-Sucedida**
? **Testes Manuais: Pendente**
? **Documenta��o: Completa**

---

## Checklist de Implementa��o

- [x] Interface `ICredentialManager` criada
- [x] Implementa��o `WindowsCredentialManager` criada
- [x] Integra��o com `LoginViewModel`
- [x] Registro no DI container
- [x] Gerenciamento de mem�ria adequado
- [x] Logging completo
- [x] Tratamento de erros
- [x] Compila��o sem erros
- [x] Documenta��o criada
- [ ] Testes manuais realizados
- [ ] Testes em ambiente de produ��o
