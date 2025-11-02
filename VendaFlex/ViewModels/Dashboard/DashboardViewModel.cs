using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.ViewModels.Base;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

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

        private string _currentUserName = "";
        public string CurrentUserName
        {
            get => _currentUserName;
            set => Set(ref _currentUserName, value);
        }

        private string _currentUserEmail = "";
        public string CurrentUserEmail
        {
            get => _currentUserEmail;
            set => Set(ref _currentUserEmail, value);
        }

        private string _currentUserInitials = "";
        public string CurrentUserInitials
        {
            get => _currentUserInitials;
            set => Set(ref _currentUserInitials, value);
        }

        private string? _currentUserProfileImageUrl;
        public string? CurrentUserProfileImageUrl
        {
            get => _currentUserProfileImageUrl;
            set => Set(ref _currentUserProfileImageUrl, value);
        }

        private PlotModel? _salesPlotModel;
        public PlotModel? SalesPlotModel
        {
            get => _salesPlotModel;
            set => Set(ref _salesPlotModel, value);
        }

        private string _salesChartCaption = string.Empty;
        public string SalesChartCaption
        {
            get => _salesChartCaption;
            set => Set(ref _salesChartCaption, value);
        }

        private ObservableCollection<SalesPointDto> _salesPoints = new();
        public ObservableCollection<SalesPointDto> SalesPoints
        {
            get => _salesPoints;
            set => Set(ref _salesPoints, value);
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
                await LoadCurrentUserInfoAsync();

                await LoadMetricsAsync();
                await LoadTopProductsAsync();
                await LoadRecentInvoicesAsync();
                await RefreshNotificationsAsync();
                await LoadSalesChartAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERRO ao carregar dados do dashboard: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine("=== DashboardViewModel: LoadDashboardDataAsync CONCLUÍDO ===");
            }
        }

        private async Task LoadCurrentUserInfoAsync()
        {
            var user = _sessionService.CurrentUser;
            if (user == null)
            {
                CurrentUserName = "Usuário";
                CurrentUserEmail = string.Empty;
                CurrentUserInitials = "?";
                CurrentUserProfileImageUrl = null;
                return;
            }

            // Tentar obter dados da Person
            try
            {
                if (user.PersonId > 0)
                {
                    var personResult = await _personService.GetByIdAsync(user.PersonId);
                    if (personResult.Success && personResult.Data != null)
                    {
                        var name = personResult.Data.Name;
                        var email = personResult.Data.Email;
                        var photo = personResult.Data.ProfileImageUrl;

                        CurrentUserName = string.IsNullOrWhiteSpace(name) ? user.Username : name;
                        CurrentUserEmail = email ?? string.Empty;
                        CurrentUserProfileImageUrl = string.IsNullOrWhiteSpace(photo) ? null : photo;
                        CurrentUserInitials = ComputeInitials(CurrentUserName);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Falha ao obter Person do usuário: {ex.Message}");
            }

            // Fallback para username
            CurrentUserName = user.Username;
            CurrentUserEmail = string.Empty;
            CurrentUserProfileImageUrl = null;
            CurrentUserInitials = ComputeInitials(CurrentUserName);
        }

        private static string ComputeInitials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(1, parts[0].Length)).ToUpperInvariant();
            var first = parts[0][0];
            var last = parts[^1][0];
            return ($"{first}{last}").ToUpperInvariant();
        }

        /// <summary>
        /// Carrega dados do gráfico de receita (últimos 30 dias)
        /// </summary>
        private async Task LoadSalesChartAsync()
        {
            try
            {
                var start = DateTime.Today.AddDays(-29);
                var end = DateTime.Today;
                var days = (end - start).Days + 1; // 30 dias

                var map = Enumerable.Range(0, days)
                    .ToDictionary(i => start.AddDays(i).Date, _ => 0m);

                var invoicesResult = await _invoiceService.GetAllAsync();
                if (invoicesResult.Success && invoicesResult.Data != null)
                {
                    foreach (var inv in invoicesResult.Data.Where(i => i.Date.Date >= start && i.Date.Date <= end))
                    {
                        var d = inv.Date.Date;
                        map[d] += inv.Total;
                    }
                }

                SalesPoints = new ObservableCollection<SalesPointDto>(
                    map.Select(kvp => new SalesPointDto
                    {
                        Date = kvp.Key,
                        Label = kvp.Key.ToString("dd/MM"),
                        Value = kvp.Value,
                        BarHeight = 0
                    }));

                // Legenda/Descrição
                SalesChartCaption = $"Receita diária de {start:dd/MM} a {end:dd/MM}. Eixo X: Data • Eixo Y: Valor (Kz).";

                var model = new PlotModel { Title = string.Empty };

                var xAxis = new DateTimeAxis
                {
                    Position = AxisPosition.Bottom,
                    StringFormat = "dd/MM",
                    IntervalType = DateTimeIntervalType.Days,
                    MinorIntervalType = DateTimeIntervalType.Days
                };

                var yAxis = new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Minimum = 0,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot
                };
                yAxis.LabelFormatter = v => $"Kz {v:N0}";

                var line = new LineSeries
                {
                    Title = "Receita",
                    StrokeThickness = 2,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 3
                };

                foreach (var p in SalesPoints)
                {
                    line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(p.Date), (double)p.Value));
                }

                // Exibir legenda abaixo (removido para compatibilidade da versão em uso)
                // model.LegendPlacement = LegendPlacement.Outside;
                // model.LegendPosition = LegendPosition.BottomCenter;
                // model.LegendOrientation = LegendOrientation.Horizontal;

                model.Axes.Add(xAxis);
                model.Axes.Add(yAxis);
                model.Series.Add(line);

                model.InvalidatePlot(true);
                SalesPlotModel = model;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OxyPlot error: {ex.Message}\n{ex.StackTrace}");
                SalesPlotModel = new PlotModel { Title = string.Empty };
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
                    .Where(i => i.Status == InvoiceStatus.Confirmed)
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
                var topProductsResult = await _invoiceProductService.GetTopSellingProductsAsync(5);
                if (topProductsResult.Success && topProductsResult.Data != null)
                {
                    TopProducts = new ObservableCollection<TopProductDto>(topProductsResult.Data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar produtos top: {ex.Message}");
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
            }
        }

        #region Sample Data Methods (fallback)

        #endregion
    }
}
