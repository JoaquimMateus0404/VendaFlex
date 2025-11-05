# CompanyConfig - ViewModel e View

## 📋 Resumo da Implementação

Foi criado um sistema completo de gerenciamento das configurações da empresa com design profissional e moderno.

## 🎯 Componentes Criados

### 1. **CompanyConfigViewModel.cs**
Localização: `VendaFlex/ViewModels/Settings/CompanyConfigViewModel.cs`

**Funcionalidades:**
- ✅ Gerenciamento completo de todas as propriedades da empresa
- ✅ Validação em tempo real (nome, email, taxa de imposto)
- ✅ Comandos para todas as operações (Salvar, Cancelar, Upload Logo, etc.)
- ✅ Controle de estado (loading, saving, hasChanges)
- ✅ Preview de numeração de faturas
- ✅ Suporte para ativação/desativação
- ✅ Upload e remoção de logo

**Propriedades Organizadas por Categoria:**

#### Informações Gerais
- CompanyName (obrigatório, validado)
- IndustryType (combo editável)
- TaxRegime (combo editável)
- TaxId
- LogoUrl (com preview)
- BusinessHours

#### Contato
- PhoneNumber
- Email (validado)
- Website

#### Endereço
- Address
- City
- PostalCode
- Country (combo editável)

#### Configurações Fiscais
- Currency (combo com moedas)
- CurrencySymbol
- DefaultTaxRate (validado 0-100)

#### Configurações de Fatura
- InvoicePrefix
- NextInvoiceNumber (com preview)
- InvoiceFooterText
- InvoiceFormat (A4 ou Térmica)
- IncludeCustomerData
- AllowAnonymousInvoice

**Comandos Disponíveis:**
- `LoadCommand` - Carrega configurações
- `SaveCommand` - Salva alterações (habilitado quando há mudanças válidas)
- `CancelCommand` - Cancela alterações (habilitado quando há mudanças)
- `UploadLogoCommand` - Abre diálogo para selecionar logo
- `RemoveLogoCommand` - Remove logo (habilitado quando há logo)
- `ActivateCommand` - Ativa configuração (habilitado quando inativa)
- `DeactivateCommand` - Desativa configuração (habilitado quando ativa)
- `TestInvoiceNumberCommand` - Testa geração de número de fatura

### 2. **CompanyConfigView.xaml**
Localização: `VendaFlex/UI/Views/Settings/CompanyConfigView.xaml`

**Design Profissional com:**
- ✅ Interface moderna usando Material Design
- ✅ Layout responsivo com ScrollViewer
- ✅ Sistema de abas para organizar conteúdo
- ✅ Cards com sombras e bordas arredondadas
- ✅ Animações suaves
- ✅ Feedback visual de validação
- ✅ Status badge (Ativa/Inativa)
- ✅ Loading overlay
- ✅ Footer fixo com ações

**5 Abas Organizadas:**

#### Tab 1: Geral (ícone: Domain)
- Seção de Logo (preview 120x120, botões upload/remover)
- Informações da Empresa (nome, tipo, NIF, regime fiscal, horário)
- Informações de Contato (telefone, email, website)

#### Tab 2: Endereço (ícone: MapMarker)
- Formulário completo de endereço
- Placeholder para mapa (recurso futuro)

#### Tab 3: Fiscal (ícone: CurrencyUsd)
- Seleção de moeda com preview
- Taxa de imposto padrão
- Painel informativo sobre impostos
- Preview de formatação de moeda

#### Tab 4: Faturas (ícone: Receipt)
- Configuração de numeração (prefixo + próximo número)
- Preview em tempo real do formato
- Botão de teste de numeração
- Seleção de formato de impressão (A4/Térmica)
- Opções de fatura (incluir dados do cliente, permitir anônimas)
- Rodapé personalizado

#### Tab 5: Preferências (ícone: Cog)
- Status da configuração com ícone grande
- Botões de ativação/desativação
- Informações do sistema (versão, última atualização)

**Elementos Visuais Destacados:**
- Cards brancos com sombras sutis
- Títulos com hierarquia clara
- Labels descritivas
- Tooltips e textos de ajuda
- Badges de status coloridos
- Botões com ícones
- Validação inline (mensagens de erro em vermelho)

### 3. **CompanyConfigView.xaml.cs**
Localização: `VendaFlex/UI/Views/Settings/CompanyConfigView.xaml.cs`

**Funcionalidades:**
- Injeção de dependência da ViewModel via ServiceProvider
- Carregamento automático dos dados no evento Loaded
- Tratamento de erros com MessageBox

### 4. **BooleanConverters.cs**
Localização: `VendaFlex/Infrastructure/Converters/BooleanConverters.cs`

**Converters Criados:**
- `BoolToVisibilityConverter` - bool → Visibility
- `InverseBoolToVisibilityConverter` - !bool → Visibility
- `NullToVisibilityConverter` - null check → Visibility
- `BoolToStringConverter` - bool → string (parametrizado)
- `BoolToColorConverter` - bool → Brush (parametrizado)
- `BoolToIconConverter` - bool → PackIconKind (parametrizado)

### 5. **Integração com Navegação**

#### INavigationService
Adicionado método: `NavigateToCompanyConfig()`

#### NavigationService
Implementado navegação para CompanyConfigView com:
- Janela em tela cheia (maximizada)
- Modo Stack (permite voltar)
- Título: "VendaFlex - Configurações da Empresa"

#### DashboardView
Adicionado evento `Click` no botão "Configurações" da sidebar que navega para a nova view.

### 6. **DependencyInjection.cs**
Registrados:
- `CompanyConfigView` (Transient)
- `CompanyConfigViewModel` (Transient)

## 🎨 Características de Design

### Paleta de Cores
- **Primária:** #3B82F6 (Azul)
- **Sucesso:** #10B981 (Verde)
- **Erro:** #EF4444 (Vermelho)
- **Fundo:** #F5F7FA (Cinza claro)
- **Cards:** #FFFFFF (Branco)
- **Texto Primário:** #1E293B (Cinza escuro)
- **Texto Secundário:** #64748B (Cinza médio)
- **Bordas:** #E2E8F0 (Cinza claro)

### Tipografia
- **Título Principal:** 32px, Bold
- **Título de Seção:** 18px, SemiBold
- **Labels:** 13px, Medium
- **Inputs:** 14px
- **Descrições:** 12-13px

### Espaçamento
- Margin entre cards: 20px
- Padding dos cards: 24px
- Margin da página: 32px
- Gap entre colunas: 16px

### Componentes Visuais
- **Border Radius:** 12px (cards), 8px (inputs)
- **Sombras:** BlurRadius 20, Opacity 0.08
- **Animações:** Fade in ao carregar
- **Loading:** Overlay escuro com spinner circular

## 🔄 Fluxo de Uso

1. **Acesso:** Dashboard → Botão "Configurações" na sidebar
2. **Carregamento:** Dados são carregados automaticamente do banco
3. **Edição:** Usuário edita campos em qualquer aba
4. **Validação:** Validação em tempo real mostra erros
5. **Salvar:** Botão só é habilitado quando há mudanças válidas
6. **Feedback:** Mensagem de sucesso/erro após salvar
7. **Cancelar:** Descarta mudanças e recarrega dados originais

## 📦 Estrutura de Arquivos

```
VendaFlex/
├── ViewModels/
│   └── Settings/
│       └── CompanyConfigViewModel.cs       ✅ NOVO
├── UI/
│   └── Views/
│       └── Settings/
│           ├── CompanyConfigView.xaml      ✅ NOVO
│           └── CompanyConfigView.xaml.cs   ✅ NOVO
├── Infrastructure/
│   ├── Converters/
│   │   └── BooleanConverters.cs            ✅ NOVO
│   ├── Navigation/
│   │   ├── INavigationService.cs           ✅ MODIFICADO
│   │   └── NavigationService.cs            ✅ MODIFICADO
│   └── DependencyInjection.cs              ✅ MODIFICADO
└── UI/
    └── Views/
        └── Dashboard/
            ├── DashboardView.xaml          ✅ MODIFICADO
            └── DashboardView.xaml.cs       ✅ MODIFICADO
```

## ✅ Checklist de Implementação

- [x] CompanyConfigViewModel com todas as propriedades
- [x] Validação de campos obrigatórios
- [x] Comandos para todas as operações
- [x] CompanyConfigView com design profissional
- [x] Sistema de abas para organizar conteúdo
- [x] Upload e preview de logo
- [x] Preview de numeração de faturas
- [x] Validação inline com mensagens de erro
- [x] Status badge (Ativa/Inativa)
- [x] Loading overlay
- [x] Converters para binding de UI
- [x] Integração com navegação
- [x] Botão no Dashboard
- [x] Registro no DI

## 🚀 Próximos Passos (Opcional)

- [ ] Implementar upload real de imagens (atualmente só seleciona arquivo local)
- [ ] Adicionar integração com Google Maps na aba de Endereço
- [ ] Implementar histórico de alterações
- [ ] Adicionar exportação/importação de configurações
- [ ] Implementar backup automático
- [ ] Adicionar mais moedas e idiomas
- [ ] Criar templates de fatura personalizáveis

## 📝 Notas Técnicas

- ViewModel usa padrão MVVM com INotifyPropertyChanged via BaseViewModel
- Comandos implementados com RelayCommand
- Validação integrada com FluentValidation via CompanyConfigDtoValidator
- Serviço ICompanyConfigService já implementado e injetado
- Converters permitem binding flexível de dados booleanos para UI
- InitializeComponent() é gerado automaticamente no build - erros são normais no editor

---

**Desenvolvido para VendaFlex** 🚀
Versão: 1.0.0
Data: Novembro 2025
