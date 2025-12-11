using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;

namespace VendaFlex.ViewModels.Settings
{
    /// <summary>
    /// ViewModel para a página de Ajuda e Suporte do sistema VendaFlex.
    /// Fornece documentação, tutoriais, FAQ e informações de contato.
    /// </summary>
    public class HelpViewModel : BaseViewModel
    {
        #region Properties

        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (Set(ref _searchQuery, value))
                {
                    FilterContent();
                }
            }
        }

        private ObservableCollection<HelpCategory> _categories = new();
        public ObservableCollection<HelpCategory> Categories
        {
            get => _categories;
            set => Set(ref _categories, value);
        }

        private ObservableCollection<HelpCategory> _filteredCategories = new();
        public ObservableCollection<HelpCategory> FilteredCategories
        {
            get => _filteredCategories;
            set => Set(ref _filteredCategories, value);
        }

        private ObservableCollection<FaqItem> _faqItems = new();
        public ObservableCollection<FaqItem> FaqItems
        {
            get => _faqItems;
            set => Set(ref _faqItems, value);
        }

        private ObservableCollection<FaqItem> _filteredFaqItems = new();
        public ObservableCollection<FaqItem> FilteredFaqItems
        {
            get => _filteredFaqItems;
            set => Set(ref _filteredFaqItems, value);
        }

        private ObservableCollection<VideoTutorial> _tutorials = new();
        public ObservableCollection<VideoTutorial> Tutorials
        {
            get => _tutorials;
            set => Set(ref _tutorials, value);
        }

        private string _systemVersion = "1.0.0";
        public string SystemVersion
        {
            get => _systemVersion;
            set => Set(ref _systemVersion, value);
        }

        private string _supportEmail = "suporte@vendaflex.com";
        public string SupportEmail
        {
            get => _supportEmail;
            set => Set(ref _supportEmail, value);
        }

        private string _supportPhone = "+244 923 456 789";
        public string SupportPhone
        {
            get => _supportPhone;
            set => Set(ref _supportPhone, value);
        }

        private string _supportHours = "Segunda a Sexta: 8h - 18h";
        public string SupportHours
        {
            get => _supportHours;
            set => Set(ref _supportHours, value);
        }

        #endregion

        #region Commands

        public ICommand OpenEmailCommand { get; }
        public ICommand OpenPhoneCommand { get; }
        public ICommand OpenDocumentationCommand { get; }
        public ICommand ClearSearchCommand { get; }

        #endregion

        #region Constructor

        public HelpViewModel()
        {
            OpenEmailCommand = new RelayCommand(_ => OpenEmail());
            OpenPhoneCommand = new RelayCommand(_ => OpenPhone());
            OpenDocumentationCommand = new RelayCommand(param => OpenDocumentation(param as string));
            ClearSearchCommand = new RelayCommand(_ => ClearSearch());

            InitializeHelpContent();
        }

        #endregion

        #region Methods

        private void InitializeHelpContent()
        {
            // Categorias de Ajuda
            Categories = new ObservableCollection<HelpCategory>
            {
                new HelpCategory
                {
                    Id = 1,
                    Title = "Primeiros Passos",
                    Description = "Aprenda a configurar e começar a usar o VendaFlex",
                    Icon = "RocketLaunch",
                    IconColor = "#3B82F6",
                    IconBackgroundColor = "#DBEAFE",
                    Articles = new ObservableCollection<HelpArticle>
                    {
                        new HelpArticle { Title = "Configuração inicial do sistema", Duration = "5 min" },
                        new HelpArticle { Title = "Cadastro de empresa e usuários", Duration = "3 min" },
                        new HelpArticle { Title = "Configuração de impostos e taxas", Duration = "4 min" },
                        new HelpArticle { Title = "Importação de dados iniciais", Duration = "7 min" }
                    }
                },
                new HelpCategory
                {
                    Id = 2,
                    Title = "Gestão de Faturas",
                    Description = "Emissão, gestão e impressão de faturas",
                    Icon = "Receipt",
                    IconColor = "#10B981",
                    IconBackgroundColor = "#D1FAE5",
                    Articles = new ObservableCollection<HelpArticle>
                    {
                        new HelpArticle { Title = "Como emitir uma fatura", Duration = "4 min" },
                        new HelpArticle { Title = "Anular e retificar faturas", Duration = "5 min" },
                        new HelpArticle { Title = "Notas de crédito e débito", Duration = "6 min" },
                        new HelpArticle { Title = "Impressão e envio por email", Duration = "3 min" },
                        new HelpArticle { Title = "Consulta de histórico", Duration = "2 min" }
                    }
                },
                new HelpCategory
                {
                    Id = 3,
                    Title = "Produtos e Estoque",
                    Description = "Gestão completa de produtos e inventário",
                    Icon = "Package",
                    IconColor = "#F59E0B",
                    IconBackgroundColor = "#FEF3C7",
                    Articles = new ObservableCollection<HelpArticle>
                    {
                        new HelpArticle { Title = "Cadastro de produtos", Duration = "4 min" },
                        new HelpArticle { Title = "Controle de estoque", Duration = "5 min" },
                        new HelpArticle { Title = "Movimentações de entrada e saída", Duration = "6 min" },
                        new HelpArticle { Title = "Auditoria de estoque", Duration = "7 min" },
                        new HelpArticle { Title = "Alertas de stock mínimo", Duration = "3 min" }
                    }
                },
                new HelpCategory
                {
                    Id = 4,
                    Title = "Clientes e Fornecedores",
                    Description = "Gestão de relacionamento com parceiros",
                    Icon = "AccountMultiple",
                    IconColor = "#8B5CF6",
                    IconBackgroundColor = "#EDE9FE",
                    Articles = new ObservableCollection<HelpArticle>
                    {
                        new HelpArticle { Title = "Cadastro de clientes", Duration = "3 min" },
                        new HelpArticle { Title = "Cadastro de fornecedores", Duration = "3 min" },
                        new HelpArticle { Title = "Histórico de transações", Duration = "4 min" },
                        new HelpArticle { Title = "Gestão de contactos", Duration = "2 min" }
                    }
                },
                new HelpCategory
                {
                    Id = 5,
                    Title = "Relatórios e Análises",
                    Description = "Extração de insights e relatórios gerenciais",
                    Icon = "ChartLine",
                    IconColor = "#EF4444",
                    IconBackgroundColor = "#FEE2E2",
                    Articles = new ObservableCollection<HelpArticle>
                    {
                        new HelpArticle { Title = "Dashboard executivo", Duration = "3 min" },
                        new HelpArticle { Title = "Relatórios de vendas", Duration = "5 min" },
                        new HelpArticle { Title = "Relatórios de estoque", Duration = "4 min" },
                        new HelpArticle { Title = "Análise financeira", Duration = "6 min" },
                        new HelpArticle { Title = "Exportação de dados", Duration = "3 min" }
                    }
                },
                new HelpCategory
                {
                    Id = 6,
                    Title = "Gestão de Despesas",
                    Description = "Controle de custos e despesas operacionais",
                    Icon = "CreditCardMinus",
                    IconColor = "#EC4899",
                    IconBackgroundColor = "#FCE7F3",
                    Articles = new ObservableCollection<HelpArticle>
                    {
                        new HelpArticle { Title = "Registro de despesas", Duration = "4 min" },
                        new HelpArticle { Title = "Categorias de despesas", Duration = "3 min" },
                        new HelpArticle { Title = "Aprovação de despesas", Duration = "5 min" },
                        new HelpArticle { Title = "Relatórios de custos", Duration = "4 min" }
                    }
                },
                new HelpCategory
                {
                    Id = 7,
                    Title = "Configurações",
                    Description = "Personalização e configuração do sistema",
                    Icon = "Cog",
                    IconColor = "#6B7280",
                    IconBackgroundColor = "#F3F4F6",
                    Articles = new ObservableCollection<HelpArticle>
                    {
                        new HelpArticle { Title = "Configurações da empresa", Duration = "5 min" },
                        new HelpArticle { Title = "Gestão de usuários e permissões", Duration = "6 min" },
                        new HelpArticle { Title = "Personalização de documentos", Duration = "4 min" },
                        new HelpArticle { Title = "Integração com AGT", Duration = "8 min" }
                    }
                }
            };

            // FAQ Items
            FaqItems = new ObservableCollection<FaqItem>
            {
                new FaqItem
                {
                    Question = "Como posso emitir minha primeira fatura?",
                    Answer = "Para emitir uma fatura, vá ao menu 'Faturas' e clique em 'Nova Fatura'. Selecione o cliente, adicione os produtos desejados, revise os valores e clique em 'Emitir'. O sistema irá gerar automaticamente o documento com a numeração sequencial."
                },
                new FaqItem
                {
                    Question = "É possível anular uma fatura já emitida?",
                    Answer = "Sim, você pode anular faturas através do menu 'Gestão de Faturas'. Selecione a fatura desejada, clique em 'Anular' e informe o motivo. O sistema criará automaticamente uma nota de crédito correspondente."
                },
                new FaqItem
                {
                    Question = "Como faço para importar dados de outro sistema?",
                    Answer = "O VendaFlex oferece assistentes de importação para produtos, clientes e fornecedores. Acesse 'Configurações' > 'Importação de Dados' e siga o assistente passo a passo. Aceita arquivos Excel (.xlsx) e CSV."
                },
                new FaqItem
                {
                    Question = "Como configurar os impostos e taxas?",
                    Answer = "Vá em 'Configurações' > 'Configurações da Empresa' > aba 'Impostos'. Configure o IVA, Selo, Imposto de Consumo e outras taxas aplicáveis. As taxas serão aplicadas automaticamente nas faturas."
                },
                new FaqItem
                {
                    Question = "O sistema funciona offline?",
                    Answer = "Sim, o VendaFlex funciona totalmente offline. Todas as operações são realizadas localmente. A sincronização com a AGT ocorre apenas quando necessário enviar documentos fiscais."
                },
                new FaqItem
                {
                    Question = "Como adiciono novos usuários ao sistema?",
                    Answer = "Acesse 'Usuários' no menu lateral, clique em 'Adicionar Usuário', preencha os dados e defina o nível de acesso (Administrador, Gerente ou Operador). Cada nível tem permissões específicas."
                },
                new FaqItem
                {
                    Question = "Como faço backup dos meus dados?",
                    Answer = "O sistema realiza backups automáticos diariamente. Para backup manual, vá em 'Configurações' > 'Backup e Restauração' e clique em 'Criar Backup Agora'. Recomendamos manter backups em locais seguros."
                },
                new FaqItem
                {
                    Question = "Como personalizar o logotipo nas faturas?",
                    Answer = "Em 'Configurações' > 'Configurações da Empresa' > aba 'Informações Gerais', você pode fazer upload do logotipo da empresa. Ele será exibido automaticamente em todas as faturas e documentos."
                },
                new FaqItem
                {
                    Question = "O que fazer se o estoque ficar negativo?",
                    Answer = "O sistema permite configurar se vendas com estoque negativo são permitidas. Vá em 'Configurações' > 'Estoque' e defina as regras. Recomendamos sempre manter o controle de estoque atualizado."
                },
                new FaqItem
                {
                    Question = "Como emitir notas de crédito?",
                    Answer = "As notas de crédito são geradas automaticamente ao anular faturas. Você também pode emitir manualmente em 'Gestão de Faturas' > 'Nova Nota de Crédito', vinculando à fatura original."
                },
                new FaqItem
                {
                    Question = "Posso usar o sistema em múltiplos computadores?",
                    Answer = "Sim, você pode instalar o VendaFlex em vários computadores. Para trabalhar com a mesma base de dados, configure o caminho do banco de dados em 'Configurações' > 'Banco de Dados'."
                },
                new FaqItem
                {
                    Question = "Como gerar relatórios personalizados?",
                    Answer = "Vá ao menu 'Relatórios', selecione o tipo desejado (Vendas, Estoque, Financeiro, etc.), defina os filtros e período, e clique em 'Gerar'. Você pode exportar em PDF ou Excel."
                }
            };

            // Video Tutorials
            Tutorials = new ObservableCollection<VideoTutorial>
            {
                new VideoTutorial
                {
                    Title = "Introdução ao VendaFlex",
                    Description = "Visão geral das funcionalidades principais",
                    Duration = "8:30",
                    Thumbnail = "RocketLaunch",
                    Category = "BÁSICO"
                },
                new VideoTutorial
                {
                    Title = "Emissão de Faturas Passo a Passo",
                    Description = "Tutorial completo sobre emissão de documentos",
                    Duration = "12:15",
                    Thumbnail = "Receipt",
                    Category = "FATURAÇÃO"
                },
                new VideoTutorial
                {
                    Title = "Gestão de Estoque",
                    Description = "Como controlar e auditar seu inventário",
                    Duration = "10:45",
                    Thumbnail = "Warehouse",
                    Category = "ESTOQUE"
                },
                new VideoTutorial
                {
                    Title = "Configuração Inicial",
                    Description = "Configure sua empresa do zero",
                    Duration = "15:20",
                    Thumbnail = "Cog",
                    Category = "CONFIGURAÇÃO"
                }
            };

            FilteredCategories = new ObservableCollection<HelpCategory>(Categories);
            FilteredFaqItems = new ObservableCollection<FaqItem>(FaqItems);
        }

        private void FilterContent()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                FilteredCategories = new ObservableCollection<HelpCategory>(Categories);
                FilteredFaqItems = new ObservableCollection<FaqItem>(FaqItems);
                return;
            }

            var query = SearchQuery.ToLower();

            // Filter Categories
            var filteredCats = Categories.Where(c =>
                c.Title.ToLower().Contains(query) ||
                c.Description.ToLower().Contains(query) ||
                c.Articles.Any(a => a.Title.ToLower().Contains(query))
            ).ToList();

            FilteredCategories = new ObservableCollection<HelpCategory>(filteredCats);

            // Filter FAQ
            var filteredFaq = FaqItems.Where(f =>
                f.Question.ToLower().Contains(query) ||
                f.Answer.ToLower().Contains(query)
            ).ToList();

            FilteredFaqItems = new ObservableCollection<FaqItem>(filteredFaq);
        }

        private void ClearSearch()
        {
            SearchQuery = string.Empty;
        }

        private void OpenEmail()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"mailto:{SupportEmail}",
                    UseShellExecute = true
                });
            }
            catch
            {
                // Log error if needed
            }
        }

        private void OpenPhone()
        {
            // Could open Skype or other calling app
            // For now, just copy to clipboard
            System.Windows.Clipboard.SetText(SupportPhone);
        }

        private void OpenDocumentation(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
                // Log error if needed
            }
        }

        #endregion
    }

    #region Helper Classes

    public class HelpCategory
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "Folder";
        public string IconColor { get; set; } = "#3B82F6";
        public string IconBackgroundColor { get; set; } = "#DBEAFE";
        public ObservableCollection<HelpArticle> Articles { get; set; } = new();
    }

    public class HelpArticle
    {
        public string Title { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
    }

    public class FaqItem
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set => _isExpanded = value;
        }
    }

    public class VideoTutorial
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    #endregion
}

