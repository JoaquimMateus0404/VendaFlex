using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.ViewModels.Base;

namespace VendaFlex.ViewModels.Dashboard
{
    /// <summary>
    /// ViewModel para o Dashboard com dados dinâmicos
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly IExpirationService _expirationService;
        private readonly IInvoiceProductService _invoiceProductService;
        private readonly IPersonService _personService;
        private readonly ISessionService _sessionService;
        private readonly ICurrentUserContext _currentUserContext;

        #region Properties

        private ObservableCollection<DashboardMetricDto> _metrics = new();
        public ObservableCollection<DashboardMetricDto> Metrics
        {
            get => _metrics;
            set => Set(ref _metrics, value);
        }

        private ObservableCollection<TopProductDto> _topProducts = new();
        public ObservableCollection<TopProductDto> TopProducts
        {
            get => _topProducts;
            set => Set(ref _topProducts, value);
        }

        private ObservableCollection<RecentInvoiceDto> _recentInvoices = new();
        public ObservableCollection<RecentInvoiceDto> RecentInvoices
        {
            get => _recentInvoices;
            set => Set(ref _recentInvoices, value);
        }

        private ObservableCollection<DashboardNotificationDto> _notifications = new();
        public ObservableCollection<DashboardNotificationDto> Notifications
        {
            get => _notifications;
            set
            {
                if (Set(ref _notifications, value))
                {
                    UnreadNotificationsCount = _notifications?.Count(n => !n.IsRead) ?? 0;
                }
            }
        }

        private int _unreadNotificationsCount;
        public int UnreadNotificationsCount
        {
            get => _unreadNotificationsCount;
            set => Set(ref _unreadNotificationsCount, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private string _currentUserName = "Admin User";
        public string CurrentUserName
        {
            get => _currentUserName;
            set => Set(ref _currentUserName, value);
        }

        private string _currentUserEmail = "admin@vendaflex.ao";
        public string CurrentUserEmail
        {
            get => _currentUserEmail;
            set => Set(ref _currentUserEmail, value);
        }

        private string _currentUserInitials = "AD";
        public string CurrentUserInitials
        {
            get => _currentUserInitials;
            set => Set(ref _currentUserInitials, value);
        }

        private int? _currentUserId;
        public int? CurrentUserId
        {
            get => _currentUserId;
            set => Set(ref _currentUserId, value);
        }

        #endregion

        public DashboardViewModel(
            IInvoiceService invoiceService,
            IProductService productService,
            IStockService stockService,
            IExpirationService expirationService,
            IInvoiceProductService invoiceProductService,
            IPersonService personService,
            ICurrentUserContext currentUserContext,
            ISessionService sessionService)
        {
            _invoiceService = invoiceService;
            _productService = productService;
            _stockService = stockService;
            _expirationService = expirationService;
            _invoiceProductService = invoiceProductService;
            _personService = personService;
            _currentUserContext = currentUserContext;
            _sessionService = sessionService;
        }

        /// <summary>
        /// Carrega todos os dados do dashboard
        /// </summary>
        public async Task LoadDashboardDataAsync()
        {
            System.Diagnostics.Debug.WriteLine("=== DashboardViewModel: LoadDashboardDataAsync INICIADO ===");
            IsLoading = true;
            try
            {
                System.Diagnostics.Debug.WriteLine("DashboardViewModel: Carregando Metrics...");
                await LoadMetricsAsync();
                System.Diagnostics.Debug.WriteLine($"DashboardViewModel: Metrics carregadas. Count = {Metrics.Count}");
                
                System.Diagnostics.Debug.WriteLine("DashboardViewModel: Carregando TopProducts...");
                await LoadTopProductsAsync();
                System.Diagnostics.Debug.WriteLine($"DashboardViewModel: TopProducts carregados. Count = {TopProducts.Count}");
                
                System.Diagnostics.Debug.WriteLine("DashboardViewModel: Carregando RecentInvoices...");
                await LoadRecentInvoicesAsync();
                System.Diagnostics.Debug.WriteLine($"DashboardViewModel: RecentInvoices carregadas. Count = {RecentInvoices.Count}");
                
                System.Diagnostics.Debug.WriteLine("DashboardViewModel: Carregando Notifications...");
                await RefreshNotificationsAsync();
                System.Diagnostics.Debug.WriteLine($"DashboardViewModel: Notifications carregadas. Count = {Notifications.Count}");

                CurrentUserName = _sessionService.CurrentUser!.Username;
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"❌ ERRO ao carregar dados do dashboard: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                Console.WriteLine($"Erro ao carregar dados do dashboard: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("=== DashboardViewModel: LoadDashboardDataAsync CONCLUÍDO ===");
            }
        }

        /// <summary>
        /// Atualiza lista de notificações e contador de não lidas
        /// </summary>
        public async Task RefreshNotificationsAsync()
        {
            await LoadNotificationsAsync();
            UnreadNotificationsCount = Notifications?.Count(n => !n.IsRead) ?? 0;
        }

        /// <summary>
        /// Marca todas as notificações como lidas (somente UI por enquanto)
        /// </summary>
        public void MarkAllNotificationsAsRead()
        {
            if (Notifications == null || Notifications.Count == 0) return;
            foreach (var n in Notifications)
            {
                n.IsRead = true;
            }
            // disparar notificação de mudança na coleção e contador
            Notifications = new ObservableCollection<DashboardNotificationDto>(Notifications);
            UnreadNotificationsCount = 0;
        }

        /// <summary>
        /// Carrega métricas principais do dashboard
        /// </summary>
        private async Task LoadMetricsAsync()
        {
            try
            {
                var now = DateTime.Now;
                var currentMonthStart = new DateTime(now.Year, now.Month, 1);
                var lastMonthStart = currentMonthStart.AddMonths(-1);
                var lastMonthEnd = currentMonthStart.AddDays(-1);

                // Buscar todas as faturas
                var allInvoicesResult = await _invoiceService.GetAllAsync();
                if (!allInvoicesResult.Success)
                {
                   // LoadSampleMetrics();
                    return;
                }

                var allInvoices = allInvoicesResult.Data?.ToList() ?? new List<InvoiceDto>();

                // Receita Total (mês atual)
                var currentMonthRevenue = allInvoices
                    .Where(i => i.Date >= currentMonthStart)
                    .Sum(i => i.Total);

                var lastMonthRevenue = allInvoices
                    .Where(i => i.Date >= lastMonthStart && i.Date <= lastMonthEnd)
                    .Sum(i => i.Total);

                var revenueChange = lastMonthRevenue > 0 
                    ? ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 
                    : 0;

                // Faturas Emitidas
                var currentMonthInvoices = allInvoices
                    .Count(i => i.Date >= currentMonthStart);

                var lastMonthInvoices = allInvoices
                    .Count(i => i.Date >= lastMonthStart && i.Date <= lastMonthEnd);

                var invoicesChange = lastMonthInvoices > 0 
                    ? ((double)(currentMonthInvoices - lastMonthInvoices) / lastMonthInvoices) * 100 
                    : 0;

                // Produtos em Estoque
                var allProductsResult = await _productService.GetAllAsync();
                var totalProducts = allProductsResult.Success ? (allProductsResult.Data?.Count() ?? 0) : 0;

                var lowStockResult = await _stockService.GetLowStockAsync();
                var lowStockProducts = lowStockResult.Success ? (lowStockResult.Data?.Count() ?? 0) : 0;

                // Pagamentos Pendentes (Status Confirmed = 2 significa confirmado mas não totalmente pago)
                var pendingInvoices = allInvoices
                    .Where(i => i.Status == (int)InvoiceStatus.Confirmed)
                    .ToList();

                var pendingAmount = pendingInvoices.Sum(i => i.Total - i.PaidAmount);
                var pendingCount = pendingInvoices.Count;

                var metrics = new ObservableCollection<DashboardMetricDto>
                {
                    new DashboardMetricDto
                    {
                        Title = "Receita Total",
                        Value = $"Kz {currentMonthRevenue:N2}",
                        IconKind = "CashMultiple",
                        IconBackgroundColor = "#FEE2E2",
                        IconColor = "#EF4444",
                        ChangeText = $"{(revenueChange >= 0 ? "↑" : "↓")} {Math.Abs(revenueChange):F1}%",
                        ChangeColor = revenueChange >= 0 ? "#10B981" : "#EF4444",
                        DescriptionText = "vs. mês passado"
                    },
                    new DashboardMetricDto
                    {
                        Title = "Faturas Emitidas",
                        Value = currentMonthInvoices.ToString(),
                        IconKind = "Receipt",
                        IconBackgroundColor = "#DBEAFE",
                        IconColor = "#3B82F6",
                        ChangeText = $"{(invoicesChange >= 0 ? "↑" : "↓")} {Math.Abs(invoicesChange):F1}%",
                        ChangeColor = invoicesChange >= 0 ? "#10B981" : "#EF4444",
                        DescriptionText = "vs. mês passado"
                    },
                    new DashboardMetricDto
                    {
                        Title = "Produtos",
                        Value = totalProducts.ToString(),
                        IconKind = "Package",
                        IconBackgroundColor = "#E0E7FF",
                        IconColor = "#6366F1",
                        ChangeText = $"{lowStockProducts} baixo estoque",
                        ChangeColor = lowStockProducts > 0 ? "#F59E0B" : "#10B981",
                        DescriptionText = ""
                    },
                    new DashboardMetricDto
                    {
                        Title = "Pendentes",
                        Value = $"Kz {pendingAmount:N2}",
                        IconKind = "CreditCardClock",
                        IconBackgroundColor = "#FEF3C7",
                        IconColor = "#F59E0B",
                        ChangeText = $"{pendingCount} faturas",
                        ChangeColor = "#9CA3AF",
                        DescriptionText = ""
                    }
                };

                Metrics = metrics;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar métricas: {ex.Message}");
               // LoadSampleMetrics();
            }
        }

        /// <summary>
        /// Carrega produtos mais vendidos
        /// </summary>
        private async Task LoadTopProductsAsync()
        {
            try
            {
                // NOTA: Esta implementação simplificada usa dados de exemplo
                // Para evitar problemas de concorrência do DbContext ao buscar produtos de cada fatura
                // TODO: Criar um método otimizado no serviço que faça uma única query agregada
                
                // Por enquanto, usar dados de exemplo
               // LoadSampleTopProducts();
                
                // IMPLEMENTAÇÃO FUTURA COM QUERY OTIMIZADA:

                var topProductsResult = await _invoiceProductService.GetTopSellingProductsAsync(5);
                if (topProductsResult.Success && topProductsResult.Data != null)
                {
                    TopProducts = new ObservableCollection<TopProductDto>(topProductsResult.Data);
                }
                
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar produtos top: {ex.Message}");
                //LoadSampleTopProducts();
            }
        }

        /// <summary>
        /// Carrega faturas recentes
        /// </summary>
        private async Task LoadRecentInvoicesAsync()
        {
            try
            {
                var allInvoicesResult = await _invoiceService.GetAllAsync();
                if (!allInvoicesResult.Success || allInvoicesResult.Data == null)
                {
                    //LoadSampleInvoices();
                    return;
                }

                var invoices = allInvoicesResult.Data
                    .OrderByDescending(i => i.Date)
                    .Take(10)
                    .ToList();

                var recentInvoicesList = new List<RecentInvoiceDto>();

                foreach (var invoice in invoices)
                {
                    var customerName = "Cliente Desconhecido";
                    if (invoice.PersonId > 0)
                    {
                        var personResult = await _personService.GetByIdAsync(invoice.PersonId);
                        if (personResult.Success && personResult.Data != null)
                        {
                            customerName = personResult.Data.Name;
                        }
                    }

                    // Converter status numérico para string
                    var statusText = ((InvoiceStatus)invoice.Status) switch
                    {
                        InvoiceStatus.Draft => "Rascunho",
                        InvoiceStatus.Confirmed => "Pendente",
                        InvoiceStatus.Paid => "Pago",
                        InvoiceStatus.Cancelled => "Cancelado",
                        InvoiceStatus.Refunded => "Reembolsado",
                        _ => "Desconhecido"
                    };

                    recentInvoicesList.Add(new RecentInvoiceDto
                    {
                        InvoiceNumber = invoice.InvoiceNumber,
                        CustomerName = customerName,
                        IssueDate = invoice.Date,
                        TotalAmount = invoice.Total,
                        Status = statusText
                    });
                }

                RecentInvoices = new ObservableCollection<RecentInvoiceDto>(recentInvoicesList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar faturas recentes: {ex.Message}");
                //LoadSampleInvoices();
            }
        }

        /// <summary>
        /// Carrega notificações
        /// </summary>
        private async Task LoadNotificationsAsync()
        {
            try
            {
                var notifications = new ObservableCollection<DashboardNotificationDto>();

                // Notificação de estoque baixo
                var lowStockResult = await _stockService.GetLowStockAsync();
                var lowStockCount = lowStockResult.Success && lowStockResult.Data != null 
                    ? lowStockResult.Data.Count() 
                    : 0;

                if (lowStockCount > 0)
                {
                    notifications.Add(new DashboardNotificationDto
                    {
                        Title = "Estoque Baixo",
                        Message = $"{lowStockCount} produto{(lowStockCount > 1 ? "s" : "")} com estoque abaixo do mínimo",
                        IconKind = "AlertCircle",
                        IconBackgroundColor = "#FEE2E2",
                        IconColor = "#EF4444",
                        Timestamp = DateTime.Now.AddHours(-2),
                        IsRead = false
                    });
                }

                // Notificação de produtos próximos ao vencimento
                var nearExpirationResult = await _expirationService.GetNearExpirationAsync();
                var expiringCount = nearExpirationResult.Success && nearExpirationResult.Data != null 
                    ? nearExpirationResult.Data.Count() 
                    : 0;

                if (expiringCount > 0)
                {
                    notifications.Add(new DashboardNotificationDto
                    {
                        Title = "Produtos Próximos ao Vencimento",
                        Message = $"{expiringCount} produto{(expiringCount > 1 ? "s" : "")} vencem em 7 dias",
                        IconKind = "ClockAlert",
                        IconBackgroundColor = "#FEF3C7",
                        IconColor = "#F59E0B",
                        Timestamp = DateTime.Now.AddDays(-1),
                        IsRead = false
                    });
                }

                // Notificação de nova fatura (última criada hoje)
                var allInvoicesResult = await _invoiceService.GetAllAsync();
                if (allInvoicesResult.Success && allInvoicesResult.Data != null)
                {
                    var todayInvoice = allInvoicesResult.Data
                        .Where(i => i.Date >= DateTime.Today)
                        .OrderByDescending(i => i.Date)
                        .FirstOrDefault();

                    if (todayInvoice != null)
                    {
                        notifications.Add(new DashboardNotificationDto
                        {
                            Title = "Nova Fatura",
                            Message = $"Fatura {todayInvoice.InvoiceNumber} foi criada",
                            IconKind = "Receipt",
                            IconBackgroundColor = "#DBEAFE",
                            IconColor = "#3B82F6",
                            Timestamp = todayInvoice.Date,
                            IsRead = false
                        });
                    }
                }

                Notifications = notifications;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar notificações: {ex.Message}");
                //LoadSampleNotifications();
            }
        }

        #region Sample Data Methods (fallback)

       /* private void LoadSampleMetrics()
        {
            Metrics = new ObservableCollection<DashboardMetricDto>
            {
                new DashboardMetricDto
                {
                    Title = "Receita Total",
                    Value = "Kz 4.523.000,00",
                    IconKind = "CashMultiple",
                    IconBackgroundColor = "#FEE2E2",
                    IconColor = "#EF4444",
                    ChangeText = "↑ 12.5%",
                    ChangeColor = "#10B981",
                    DescriptionText = "vs. mês passado"
                },
                new DashboardMetricDto
                {
                    Title = "Faturas Emitidas",
                    Value = "342",
                    IconKind = "Receipt",
                    IconBackgroundColor = "#DBEAFE",
                    IconColor = "#3B82F6",
                    ChangeText = "↑ 8.2%",
                    ChangeColor = "#10B981",
                    DescriptionText = "vs. mês passado"
                },
                new DashboardMetricDto
                {
                    Title = "Produtos",
                    Value = "1.248",
                    IconKind = "Package",
                    IconBackgroundColor = "#E0E7FF",
                    IconColor = "#6366F1",
                    ChangeText = "18 baixo estoque",
                    ChangeColor = "#F59E0B",
                    DescriptionText = ""
                },
                new DashboardMetricDto
                {
                    Title = "Pendentes",
                    Value = "Kz 856.400,00",
                    IconKind = "CreditCardClock",
                    IconBackgroundColor = "#FEF3C7",
                    IconColor = "#F59E0B",
                    ChangeText = "23 faturas",
                    ChangeColor = "#9CA3AF",
                    DescriptionText = ""
                }
            };
        }

        private void LoadSampleTopProducts()
        {
            TopProducts = new ObservableCollection<TopProductDto>
            {
                new TopProductDto { ProductName = "Cimento 50kg", QuantitySold = 450, Revenue = 1250000, ProgressPercentage = 100 },
                new TopProductDto { ProductName = "Tijolo Furado", QuantitySold = 3200, Revenue = 980000, ProgressPercentage = 78 },
                new TopProductDto { ProductName = "Areia Fina", QuantitySold = 120, Revenue = 750000, ProgressPercentage = 60 },
                new TopProductDto { ProductName = "Tinta Branca 20L", QuantitySold = 85, Revenue = 620000, ProgressPercentage = 50 },
                new TopProductDto { ProductName = "Ferro 10mm", QuantitySold = 220, Revenue = 580000, ProgressPercentage = 46 }
            };
        }

        private void LoadSampleInvoices()
        {
            RecentInvoices = new ObservableCollection<RecentInvoiceDto>
            {
                new RecentInvoiceDto
                {
                    InvoiceNumber = "INV-2025-042",
                    CustomerName = "João Silva",
                    IssueDate = DateTime.Now.AddHours(-2),
                    TotalAmount = 245000,
                    Status = "Pago"
                },
                new RecentInvoiceDto
                {
                    InvoiceNumber = "INV-2025-041",
                    CustomerName = "Maria Costa",
                    IssueDate = DateTime.Now.AddHours(-5),
                    TotalAmount = 156000,
                    Status = "Pendente"
                },
                new RecentInvoiceDto
                {
                    InvoiceNumber = "INV-2025-040",
                    CustomerName = "Pedro Santos",
                    IssueDate = DateTime.Now.AddDays(-1),
                    TotalAmount = 432000,
                    Status = "Pago"
                }
            };
        }

        private void LoadSampleNotifications()
        {
            Notifications = new ObservableCollection<DashboardNotificationDto>
            {
                new DashboardNotificationDto
                {
                    Title = "Estoque Baixo",
                    Message = "5 produtos com estoque abaixo do mínimo",
                    IconKind = "AlertCircle",
                    IconBackgroundColor = "#FEE2E2",
                    IconColor = "#EF4444",
                    Timestamp = DateTime.Now.AddHours(-2),
                    IsRead = false
                },
                new DashboardNotificationDto
                {
                    Title = "Nova Fatura",
                    Message = "Fatura #INV-2025-042 foi criada",
                    IconKind = "Receipt",
                    IconBackgroundColor = "#DBEAFE",
                    IconColor = "#3B82F6",
                    Timestamp = DateTime.Now.AddHours(-5),
                    IsRead = false
                },
                new DashboardNotificationDto
                {
                    Title = "Produtos Próximos ao Vencimento",
                    Message = "3 produtos vencem em 7 dias",
                    IconKind = "ClockAlert",
                    IconBackgroundColor = "#FEF3C7",
                    IconColor = "#F59E0B",
                    Timestamp = DateTime.Now.AddDays(-1),
                    IsRead = false
                }
            };
        }*/

        #endregion
    }
}
