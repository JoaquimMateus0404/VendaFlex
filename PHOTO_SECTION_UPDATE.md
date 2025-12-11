# Atualização da Seção de Foto de Perfil

## 📋 Alterações Realizadas

Foi atualizada a seção de foto de perfil no modal de cadastro de pessoas para seguir o mesmo padrão visual da seção de logo da empresa.

## 🎨 Melhorias Visuais

### Antes:
- Foto circular centralizada no topo
- Botão único "SELECIONAR FOTO"
- Layout vertical simples

### Depois:
- ✅ Layout horizontal profissional em Card Material Design
- ✅ Preview da foto em círculo (120x120px) à esquerda
- ✅ Informações e ações à direita
- ✅ Placeholder elegante quando não há foto
- ✅ Dois botões: "Selecionar Foto" e "Remover"
- ✅ Botão "Remover" aparece apenas quando há foto
- ✅ Cores e espaçamentos consistentes com o design system

## 🔧 Funcionalidades Adicionadas

### 1. **RemovePhotoCommand**
```csharp
private void RemovePhoto()
{
    // Confirmação antes de remover
    // Tentativa de deletar arquivo físico
    // Limpar URL da foto
}
```

### 2. **Visibilidade Condicional**
- Botão "Remover" só aparece quando `ProfileImageUrl` tem valor
- Placeholder "Sem Foto" aparece quando não há imagem
- Imagem aparece quando URL está preenchida

## 📐 Estrutura do Layout

```
┌─────────────────────────────────────────────────────┐
│  Card com Padding e CornerRadius                    │
│  ┌────────────┐  ┌─────────────────────────────┐   │
│  │            │  │  Foto de Perfil             │   │
│  │   Foto     │  │  Descrição...               │   │
│  │  Preview   │  │                             │   │
│  │  120x120   │  │  [Selecionar] [Remover]     │   │
│  │            │  │                             │   │
│  └────────────┘  └─────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

## 🎯 Características do Design

### Preview da Foto:
- **Tamanho**: 120x120 pixels
- **Forma**: Circular (CornerRadius="60")
- **Background**: #F1F5F9 (cinza claro)
- **Borda**: #E2E8F0, 2px
- **ClipToBounds**: True (imagem recortada em círculo)

### Placeholder:
- **Ícone**: AccountCircle (48x48px)
- **Cor**: #94A3B8 (cinza médio)
- **Texto**: "Sem Foto" (11px)

### Texto Informativo:
- **Título**: "Foto de Perfil" (16px, SemiBold, #1E293B)
- **Descrição**: 13px, #64748B
- **Wrapping**: Automático

### Botões:
- **Estilo**: MaterialDesignOutlinedButton
- **Altura**: 40px
- **Padding**: 20px horizontal
- **Ícones**: Upload (18x18) e Delete (18x18)
- **Espaçamento**: 12px entre botões

## 🔐 Funcionalidade de Remoção

### Fluxo:
1. Usuário clica em "Remover"
2. Mensagem de confirmação (MessageBox)
3. Se confirmado:
   - Tenta deletar arquivo físico (ignora erros)
   - Limpa `ProfileImageUrl`
   - UI atualiza automaticamente
4. Botão "Remover" desaparece

### Segurança:
- ✅ Confirmação antes de remover
- ✅ Try-catch para evitar crashes
- ✅ Ignora erros ao deletar arquivo (caso já tenha sido removido)

## 📊 Comparação de Código

### XAML - Antes: ~40 linhas
### XAML - Depois: ~130 linhas
**Motivo**: Layout mais rico e estruturado

### ViewModel - Antes: 1 comando
### ViewModel - Depois: 2 comandos
**Adicionado**: `RemovePhotoCommand`

## ✅ Benefícios

1. **Consistência Visual**: Mesmo padrão da tela de configuração
2. **Melhor UX**: Informações claras sobre o propósito da foto
3. **Mais Controle**: Usuário pode remover foto facilmente
4. **Profissional**: Layout elegante e moderno
5. **Responsivo**: Adapta-se ao conteúdo
6. **Acessível**: Ícones e textos claros

## 🎨 Paleta de Cores Utilizada

| Elemento | Cor | Uso |
|----------|-----|-----|
| Background Preview | #F1F5F9 | Fundo do círculo |
| Border Preview | #E2E8F0 | Borda do círculo |
| Placeholder Icon | #94A3B8 | Ícone e texto "Sem Foto" |
| Título | #1E293B | "Foto de Perfil" |
| Descrição | #64748B | Texto explicativo |

## 📝 Notas Técnicas

### Binding:
- `ProfileImageUrl` - URL da imagem ou string vazia
- `RemovePhotoCommand` - Comando para remover foto
- `UploadPhotoCommand` - Comando existente mantido

### Conversores:
- `NullToVis` - Controla visibilidade baseado em null/empty
- `BoolToVis` - Conversão booleana para visibilidade

### Material Design:
- Card com UniformCornerRadius="12"
- PackIcons: AccountCircle, Upload, Delete
- Outlined Buttons

## 🚀 Resultado Final

A seção de foto agora tem uma aparência profissional e consistente com o resto da aplicação, oferecendo uma experiência de usuário superior com controles intuitivos e feedback visual claro.

---

**Arquivos Modificados:**
1. `PersonFormDialog.xaml` - Layout da seção de foto
2. `PersonFormDialogViewModel.cs` - Adicionado RemovePhotoCommand e método RemovePhoto()

**Status:** ✅ Concluído e testado

