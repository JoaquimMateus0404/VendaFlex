using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.IO;
using System.Text;
using Microsoft.Win32;
using VendaFlex.Core.DTOs;
using VendaFlex.Core.Interfaces;
using VendaFlex.Data.Entities;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;
using MaterialDesignThemes.Wpf;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;


namespace VendaFlex.ViewModels.Reports
{
    public class ReportManagementViewModel : BaseViewModel
    {
        #region Services

        private readonly ICompanyConfigService _companyConfigService;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly IUserService _userService;
        private readonly IInvoiceService _invoiceService;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;
        private readonly IPaymentService _paymentService;
        private readonly IPaymentTypeService _paymentTypeService;
        private readonly IPersonService _personService;
        private readonly IExpirationService _expirationService;
        private readonly IInvoiceProductService _invoiceProductService;
        private readonly IPdfGeneratorService _pdfGeneratorService;

        #endregion

        #region Properties - Loading & Messages

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        private SnackbarMessageQueue _messageQueue = new(TimeSpan.FromSeconds(3));
        public SnackbarMessageQueue MessageQueue
        {
            get => _messageQueue;
            set => Set(ref _messageQueue, value);
        }

        #endregion

        #region Properties - Report Categories

        private int _selectedCategoryIndex;
        public int SelectedCategoryIndex
        {
            get => _selectedCategoryIndex;
            set
            {
                if (Set(ref _selectedCategoryIndex, value))
                {
                    OnPropertyChanged(nameof(IsSalesReportsVisible));
                    OnPropertyChanged(nameof(IsFinancialReportsVisible));
                    OnPropertyChanged(nameof(IsStockReportsVisible));
                    OnPropertyChanged(nameof(IsManagementReportsVisible));
                    
                    // Load reports when category changes
                    _ = GenerateDefaultReportsAsync();
                }
            }
        }

        public bool IsSalesReportsVisible => SelectedCategoryIndex == 0;
        public bool IsFinancialReportsVisible => SelectedCategoryIndex == 1;
        public bool IsStockReportsVisible => SelectedCategoryIndex == 2;
        public bool IsManagementReportsVisible => SelectedCategoryIndex == 3;

        #endregion

        #region Properties - Advanced Filters

        private DateTime _startDate = DateTime.Today.AddMonths(-1);
        public DateTime StartDate
        {
            get => _startDate;
            set => Set(ref _startDate, value);
        }

        private DateTime _endDate = DateTime.Today;
        public DateTime EndDate
        {
            get => _endDate;
            set => Set(ref _endDate, value);
        }

        private bool _isAdvancedFiltersVisible;
        public bool IsAdvancedFiltersVisible
        {
            get => _isAdvancedFiltersVisible;
            set => Set(ref _isAdvancedFiltersVisible, value);
        }

        private string? _selectedInvoiceStatus;
        public string? SelectedInvoiceStatus
        {
            get => _selectedInvoiceStatus;
            set => Set(ref _selectedInvoiceStatus, value);
        }

        private int? _selectedUserId;
        public int? SelectedUserId
        {
            get => _selectedUserId;
            set => Set(ref _selectedUserId, value);
        }

        private int? _selectedPaymentTypeId;
        public int? SelectedPaymentTypeId
        {
            get => _selectedPaymentTypeId;
            set => Set(ref _selectedPaymentTypeId, value);
        }

        private string _selectedPeriodGrouping = "Dia";
        public string SelectedPeriodGrouping
        {
            get => _selectedPeriodGrouping;
            set => Set(ref _selectedPeriodGrouping, value);
        }

        public ObservableCollection<string> InvoiceStatusOptions { get; } = new()
        {
            "Todos",
            "Pago",
            "Confirmado",
            "Pendente",
            "Cancelado"
        };

        public ObservableCollection<string> PeriodGroupingOptions { get; } = new()
        {
            "Dia",
            "Semana",
            "Mês",
            "Trimestre",
            "Ano"
        };

        private ObservableCollection<User> _users = new();
        public ObservableCollection<User> Users
        {
            get => _users;
            set => Set(ref _users, value);
        }

        #endregion

        #region Properties - Statistics

        private decimal _totalSalesValue;
        public decimal TotalSalesValue
        {
            get => _totalSalesValue;
            set => Set(ref _totalSalesValue, value);
        }

        private int _totalInvoices;
        public int TotalInvoices
        {
            get => _totalInvoices;
            set => Set(ref _totalInvoices, value);
        }

        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => Set(ref _totalRevenue, value);
        }

        private decimal _totalStockValue;
        public decimal TotalStockValue
        {
            get => _totalStockValue;
            set => Set(ref _totalStockValue, value);
        }

        private decimal _totalProfitMargin;
        public decimal TotalProfitMargin
        {
            get => _totalProfitMargin;
            set => Set(ref _totalProfitMargin, value);
        }

        private int _totalPendingInvoices;
        public int TotalPendingInvoices
        {
            get => _totalPendingInvoices;
            set => Set(ref _totalPendingInvoices, value);
        }

        private decimal _totalPendingAmount;
        public decimal TotalPendingAmount
        {
            get => _totalPendingAmount;
            set => Set(ref _totalPendingAmount, value);
        }

        #endregion

        #region Properties - Charts

        private PlotModel _salesByMonthChart;
        public PlotModel SalesByMonthChart
        {
            get => _salesByMonthChart;
            set => Set(ref _salesByMonthChart, value);
        }

        private PlotModel _salesTrendChart;
        public PlotModel SalesTrendChart
        {
            get => _salesTrendChart;
            set => Set(ref _salesTrendChart, value);
        }

        private PlotModel _invoiceStatusChart;
        public PlotModel InvoiceStatusChart
        {
            get => _invoiceStatusChart;
            set => Set(ref _invoiceStatusChart, value);
        }

        private PlotModel _topProductsChart;
        public PlotModel TopProductsChart
        {
            get => _topProductsChart;
            set => Set(ref _topProductsChart, value);
        }

        private PlotModel _paymentMethodsChart;
        public PlotModel PaymentMethodsChart
        {
            get => _paymentMethodsChart;
            set => Set(ref _paymentMethodsChart, value);
        }

        #endregion

        #region Collections - Sales Reports

        private ObservableCollection<SalesByPeriodDto> _salesByPeriod = new();
        public ObservableCollection<SalesByPeriodDto> SalesByPeriod
        {
            get => _salesByPeriod;
            set => Set(ref _salesByPeriod, value);
        }

        private ObservableCollection<TopProductDto> _topProducts = new();
        public ObservableCollection<TopProductDto> TopProducts
        {
            get => _topProducts;
            set => Set(ref _topProducts, value);
        }

        private ObservableCollection<SalesByCustomerDto> _salesByCustomer = new();
        public ObservableCollection<SalesByCustomerDto> SalesByCustomer
        {
            get => _salesByCustomer;
            set => Set(ref _salesByCustomer, value);
        }

        private ObservableCollection<ProfitMarginDto> _profitMargins = new();
        public ObservableCollection<ProfitMarginDto> ProfitMargins
        {
            get => _profitMargins;
            set => Set(ref _profitMargins, value);
        }

        #endregion

        #region Collections - Financial Reports

        private ObservableCollection<CashFlowDto> _cashFlow = new();
        public ObservableCollection<CashFlowDto> CashFlow
        {
            get => _cashFlow;
            set => Set(ref _cashFlow, value);
        }

        private ObservableCollection<PaymentMethodDto> _paymentMethods = new();
        public ObservableCollection<PaymentMethodDto> PaymentMethods
        {
            get => _paymentMethods;
            set => Set(ref _paymentMethods, value);
        }

        private ObservableCollection<AccountsReceivableDto> _accountsReceivable = new();
        public ObservableCollection<AccountsReceivableDto> AccountsReceivable
        {
            get => _accountsReceivable;
            set => Set(ref _accountsReceivable, value);
        }

        private ObservableCollection<InvoicesByStatusDto> _invoicesByStatus = new();
        public ObservableCollection<InvoicesByStatusDto> InvoicesByStatus
        {
            get => _invoicesByStatus;
            set => Set(ref _invoicesByStatus, value);
        }

        #endregion

        #region Collections - Stock Reports

        private ObservableCollection<StockMovementReportDto> _stockMovements = new();
        public ObservableCollection<StockMovementReportDto> StockMovements
        {
            get => _stockMovements;
            set => Set(ref _stockMovements, value);
        }

        private ObservableCollection<LowStockDto> _lowStockProducts = new();
        public ObservableCollection<LowStockDto> LowStockProducts
        {
            get => _lowStockProducts;
            set => Set(ref _lowStockProducts, value);
        }

        private ObservableCollection<ExpirationReportDto> _expiringProducts = new();
        public ObservableCollection<ExpirationReportDto> ExpiringProducts
        {
            get => _expiringProducts;
            set => Set(ref _expiringProducts, value);
        }

        #endregion

        #region Commands

        public ICommand ToggleAdvancedFiltersCommand { get; private set; }
        public ICommand ApplyFiltersCommand { get; private set; }
        public ICommand ClearFiltersCommand { get; private set; }
        public ICommand RefreshDashboardCommand { get; private set; }

        // Report Generation Commands
        public ICommand GenerateSalesByPeriodCommand { get; private set; }
        public ICommand GenerateTopProductsCommand { get; private set; }
        public ICommand GenerateSalesByCustomerCommand { get; private set; }
        public ICommand GenerateProfitMarginCommand { get; private set; }
        public ICommand GenerateInvoicesByStatusCommand { get; private set; }
        public ICommand GenerateCashFlowCommand { get; private set; }
        public ICommand GeneratePaymentMethodsCommand { get; private set; }
        public ICommand GenerateAccountsReceivableCommand { get; private set; }
        public ICommand GenerateStockMovementsCommand { get; private set; }
        public ICommand GenerateLowStockCommand { get; private set; }
        public ICommand GenerateExpiringProductsCommand { get; private set; }
        
        // Management Reports
        public ICommand GenerateFinancialStatementCommand { get; private set; }
        public ICommand GenerateExecutiveDashboardCommand { get; private set; }
        public ICommand GenerateUserPerformanceCommand { get; private set; }
        public ICommand GenerateComprehensiveReportCommand { get; private set; }
        public ICommand GenerateSalesTrendCommand { get; private set; }


        // Export Commands
        public ICommand ExportSalesByPeriodCommand { get; private set; }
        public ICommand ExportTopProductsCommand { get; private set; }
        public ICommand ExportSalesByCustomerCommand { get; private set; }
        public ICommand ExportCashFlowCommand { get; private set; }
        public ICommand ExportPaymentMethodsCommand { get; private set; }
        public ICommand ExportAccountsReceivableCommand { get; private set; }
        public ICommand ExportStockMovementsCommand { get; private set; }
        public ICommand ExportLowStockCommand { get; private set; }
        public ICommand ExportExpiringProductsCommand { get; private set; }

        // Quick Actions
        public ICommand QuickPeriodTodayCommand { get; private set; }
        public ICommand QuickPeriodWeekCommand { get; private set; }
        public ICommand QuickPeriodMonthCommand { get; private set; }
        public ICommand QuickPeriodYearCommand { get; private set; }

        #endregion

        #region Constructor

        public ReportManagementViewModel(
            ICompanyConfigService companyConfigService,
            IInvoiceService invoiceService,
            IProductService productService,
            IStockService stockService,
            IPaymentService paymentService,
            IPaymentTypeService paymentTypeService,
            IPersonService personService,
            IExpirationService expirationService,
            IInvoiceProductService invoiceProductService,
            IUserService userService,
            IPdfGeneratorService pdfGeneratorService)
        {
            _companyConfigService = companyConfigService ?? throw new ArgumentNullException(nameof(companyConfigService));
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _personService = personService ?? throw new ArgumentNullException(nameof(personService));
            _expirationService = expirationService ?? throw new ArgumentNullException(nameof(expirationService));
            _invoiceProductService = invoiceProductService ?? throw new ArgumentNullException(nameof(invoiceProductService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
            _pdfGeneratorService = pdfGeneratorService ?? throw new ArgumentNullException(nameof(pdfGeneratorService));

            InitializeCommands();
            _ = LoadInitialDataAsync();
        }

        #endregion

        #region Initialization

        private void InitializeCommands()
        {
            // Filter Commands
            ToggleAdvancedFiltersCommand = new RelayCommand(_ => IsAdvancedFiltersVisible = !IsAdvancedFiltersVisible);
            ApplyFiltersCommand = new RelayCommand(async _ => await ApplyFiltersAsync());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            RefreshDashboardCommand = new RelayCommand(async _ => await RefreshDashboardAsync());

            // Sales Reports - Geram PDFs quando chamados da aba Gerencial
            GenerateSalesByPeriodCommand = new RelayCommand(async _ => await GenerateSalesByPeriodPdfAsync());
            GenerateTopProductsCommand = new RelayCommand(async _ => await GenerateTopProductsPdfAsync());
            GenerateSalesByCustomerCommand = new RelayCommand(async _ => await GenerateSalesByCustomerPdfAsync());
            GenerateProfitMarginCommand = new RelayCommand(async _ => await GenerateProfitMarginPdfAsync());
            GenerateInvoicesByStatusCommand = new RelayCommand(async _ => await GenerateInvoicesByStatusPdfAsync());

            // Financial Reports - Geram PDFs quando chamados da aba Gerencial
            GenerateCashFlowCommand = new RelayCommand(async _ => await GenerateCashFlowPdfAsync());
            GeneratePaymentMethodsCommand = new RelayCommand(async _ => await GeneratePaymentMethodsPdfAsync());
            GenerateAccountsReceivableCommand = new RelayCommand(async _ => await GenerateAccountsReceivableAsync());

            // Stock Reports
            GenerateStockMovementsCommand = new RelayCommand(async _ => await GenerateStockMovementsAsync());
            GenerateLowStockCommand = new RelayCommand(async _ => await GenerateLowStockAsync());
            GenerateExpiringProductsCommand = new RelayCommand(async _ => await GenerateExpiringProductsAsync());
            
            // Management Reports
            GenerateFinancialStatementCommand = new RelayCommand(async _ => await GenerateFinancialStatementAsync());
            GenerateExecutiveDashboardCommand = new RelayCommand(async _ => await GenerateExecutiveDashboardAsync());
            GenerateUserPerformanceCommand = new RelayCommand(async _ => await GenerateUserPerformanceAsync());
            GenerateComprehensiveReportCommand = new RelayCommand(async _ => await GenerateComprehensiveReportAsync());

            // Export Commands
            ExportSalesByPeriodCommand = new RelayCommand(async _ => await ExportDataAsync(SalesByPeriod, "Vendas_Por_Periodo"));
            ExportTopProductsCommand = new RelayCommand(async _ => await ExportDataAsync(TopProducts, "Top_Produtos"));
            ExportSalesByCustomerCommand = new RelayCommand(async _ => await ExportDataAsync(SalesByCustomer, "Vendas_Por_Cliente"));
            ExportCashFlowCommand = new RelayCommand(async _ => await ExportDataAsync(CashFlow, "Fluxo_Caixa"));
            ExportPaymentMethodsCommand = new RelayCommand(async _ => await ExportDataAsync(PaymentMethods, "Formas_Pagamento"));
            ExportAccountsReceivableCommand = new RelayCommand(async _ => await ExportDataAsync(AccountsReceivable, "Contas_Receber"));
            ExportStockMovementsCommand = new RelayCommand(async _ => await ExportDataAsync(StockMovements, "Movimentacao_Estoque"));
            ExportLowStockCommand = new RelayCommand(async _ => await ExportDataAsync(LowStockProducts, "Estoque_Baixo"));
            ExportExpiringProductsCommand = new RelayCommand(async _ => await ExportDataAsync(ExpiringProducts, "Produtos_Vencendo"));

            // Quick Period Actions
            QuickPeriodTodayCommand = new RelayCommand(_ => SetQuickPeriod(PeriodType.Today));
            QuickPeriodWeekCommand = new RelayCommand(_ => SetQuickPeriod(PeriodType.Week));
            QuickPeriodMonthCommand = new RelayCommand(_ => SetQuickPeriod(PeriodType.Month));
            QuickPeriodYearCommand = new RelayCommand(_ => SetQuickPeriod(PeriodType.Year));
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                IsLoading = true;

                // Load users for filter
                var usersResult = await _userService.GetAllAsync();
                if (usersResult.Success && usersResult.Data != null)
                {
                    Users = new ObservableCollection<User>(usersResult.Data.Select(u => new User
                    {
                        UserId = u.UserId,
                        PersonId = u.PersonId,
                        Username = u.Username,
                        PasswordHash = u.PasswordHash,
                        Status = u.Status,
                        LastLoginAt = u.LastLoginAt,
                        FailedLoginAttempts = u.FailedLoginAttempts,
                        LockedUntil = u.LockedUntil,
                        LastLoginIp = u.LastLoginIp
                    }));

                }

                // Load overview statistics
                await LoadOverviewStatisticsAsync();

                // Generate charts
                await GenerateChartsAsync();

                // Generate default reports for current category
                await GenerateDefaultReportsAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao carregar dados: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadOverviewStatisticsAsync()
        {
            try
            {
                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (invoicesResult.Success && invoicesResult.Data != null)
                {
                    var invoices = invoicesResult.Data.ToList();
                    
                    // Apply filters
                    if (!string.IsNullOrEmpty(SelectedInvoiceStatus) && SelectedInvoiceStatus != "Todos")
                    {
                        invoices = invoices.Where(i => i.Status.ToString() == SelectedInvoiceStatus).ToList();
                    }
                    
                    if (SelectedUserId.HasValue)
                    {
                        invoices = invoices.Where(i => i.UserId == SelectedUserId.Value).ToList();
                    }

                    TotalInvoices = invoices.Count;
                    TotalSalesValue = invoices.Sum(i => i.Total);
                    TotalRevenue = invoices.Sum(i => i.PaidAmount);
                    TotalPendingInvoices = invoices.Count(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Confirmed);
                    TotalPendingAmount = invoices.Where(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Confirmed)
                        .Sum(i => i.Total - i.PaidAmount);
                    
                    // Calculate profit margin automatically
                    await CalculateProfitMarginAsync(invoices.Where(i => i.Status == InvoiceStatus.Paid).ToList());
                }

                var stocksResult = await _stockService.GetAllAsync();
                if (stocksResult.Success && stocksResult.Data != null)
                {
                    var products = (await _productService.GetAllAsync()).Data?.ToDictionary(p => p.ProductId) ?? new();
                    TotalStockValue = stocksResult.Data
                        .Where(s => products.ContainsKey(s.ProductId))
                        .Sum(s => products[s.ProductId].SalePrice * s.Quantity);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar estatísticas: {ex.Message}");
            }
        }
        
        private async Task CalculateProfitMarginAsync(List<InvoiceDto> paidInvoices)
        {
            try
            {
                if (!paidInvoices.Any())
                {
                    TotalProfitMargin = 0;
                    return;
                }

                decimal totalGrossProfit = 0;
                decimal totalRevenue = 0;

                foreach (var invoice in paidInvoices)
                {
                    var itemsResult = await _invoiceProductService.GetByInvoiceIdAsync(invoice.InvoiceId);
                    if (itemsResult.Success && itemsResult.Data != null)
                    {
                        decimal totalCost = 0;
                        foreach (var item in itemsResult.Data)
                        {
                            var productResult = await _productService.GetByIdAsync(item.ProductId);
                            if (productResult.Success && productResult.Data != null)
                            {
                                totalCost += productResult.Data.CostPrice * item.Quantity;
                            }
                        }

                        totalGrossProfit += invoice.Total - totalCost;
                        totalRevenue += invoice.Total;
                    }
                }

                TotalProfitMargin = totalRevenue > 0 ? (totalGrossProfit / totalRevenue) * 100 : 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao calcular margem de lucro: {ex.Message}");
                TotalProfitMargin = 0;
            }
        }

        private async Task GenerateDefaultReportsAsync()
        {
            switch (SelectedCategoryIndex)
            {
                case 0: // Sales
                    await GenerateSalesByPeriodAsync();
                    await GenerateTopProductsAsync();
                    break;
                case 1: // Financial
                    await GenerateCashFlowAsync();
                    await GeneratePaymentMethodsAsync();
                    break;
                case 2: // Stock
                    await GenerateLowStockAsync();
                    break;
                case 3: // Management - Load all data needed for charts
                    await GenerateChartsAsync(); // Carrega os gráficos de Tendências e Status
                    await GenerateTopProductsAsync();
                    await GeneratePaymentMethodsAsync();
                    break;
            }
        }

        #endregion

        #region Filter Actions

        private async Task ApplyFiltersAsync()
        {
            await LoadOverviewStatisticsAsync();
            await GenerateChartsAsync();
            await GenerateDefaultReportsAsync();
            ShowMessage("Filtros aplicados com sucesso!");
        }

        private void ClearFilters()
        {
            SelectedInvoiceStatus = "Todos";
            SelectedUserId = null;
            SelectedPaymentTypeId = null;
            SelectedPeriodGrouping = "Dia";
            StartDate = DateTime.Today.AddMonths(-1);
            EndDate = DateTime.Today;
            _ = ApplyFiltersAsync();
        }

        private async Task RefreshDashboardAsync()
        {
            await LoadOverviewStatisticsAsync();
            await GenerateChartsAsync();
            await GenerateDefaultReportsAsync();
            ShowMessage("Dashboard atualizado!");
        }

        #endregion

        #region Charts Generation

        private async Task GenerateChartsAsync()
        {
            await GenerateSalesByMonthChartAsync();
            await GenerateSalesTrendChartAsync();
            await GenerateInvoiceStatusChartAsync();
        }

        private async Task GenerateSalesByMonthChartAsync()
        {
            try
            {
                var plotModel = new PlotModel
                {
                    Title = "Vendas por Mês",
                    Background = OxyColors.White,
                    TitleFontSize = 18
                };

                var categoryAxis = new CategoryAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "Mês",
                    Angle = -45
                };

                var valueAxis = new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Title = "Valor (Kz)",
                    StringFormat = "N0",
                    MinimumPadding = 0.1,
                    MaximumPadding = 0.1
                };

                plotModel.Axes.Add(categoryAxis);
                plotModel.Axes.Add(valueAxis);

                var lineSeries = new LineSeries
                {
                    Color = OxyColor.FromRgb(33, 150, 243),
                    StrokeThickness = 3,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 6,
                    MarkerFill = OxyColor.FromRgb(33, 150, 243),
                    MarkerStroke = OxyColors.White,
                    MarkerStrokeThickness = 2
                };

                // Get real data from last 12 months
                var endDate = DateTime.Today;
                var startDate = endDate.AddMonths(-11).AddDays(-endDate.Day + 1); // First day of 12 months ago

                var invoicesResult = await _invoiceService.GetByDateRangeAsync(startDate, endDate);
                
                if (invoicesResult.Success && invoicesResult.Data != null)
                {
                    var salesByMonth = invoicesResult.Data
                        .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed)
                        .GroupBy(i => new { i.Date.Year, i.Date.Month })
                        .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                        .Select(g => new
                        {
                            Year = g.Key.Year,
                            Month = g.Key.Month,
                            Total = g.Sum(i => i.Total)
                        })
                        .ToList();

                    // Fill missing months with zero
                    var allMonths = new List<(int Year, int Month, decimal Total)>();
                    for (int i = 0; i < 12; i++)
                    {
                        var date = startDate.AddMonths(i);
                        var monthData = salesByMonth.FirstOrDefault(s => s.Year == date.Year && s.Month == date.Month);
                        allMonths.Add((date.Year, date.Month, monthData?.Total ?? 0));
                        categoryAxis.Labels.Add(date.ToString("MMM/yy"));
                    }

                    int categoryIndex = 0;
                    foreach (var month in allMonths)
                    {
                        lineSeries.Points.Add(new DataPoint(categoryIndex, (double)month.Total));
                        categoryIndex++;
                    }
                }
                else
                {
                    // If no data, show empty chart
                    for (int i = 0; i < 12; i++)
                    {
                        var date = startDate.AddMonths(i);
                        categoryAxis.Labels.Add(date.ToString("MMM/yy"));
                        lineSeries.Points.Add(new DataPoint(i, 0));
                    }
                }

                plotModel.Series.Add(lineSeries);
                SalesByMonthChart = plotModel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao gerar gráfico de vendas mensais: {ex.Message}");
                ShowMessage($"Erro ao gerar gráfico: {ex.Message}", true);
            }
        }

        private async Task GenerateSalesTrendChartAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[SalesTrend] Iniciando geração do gráfico de tendências");
                
                var plotModel = new PlotModel 
                { 
                    Title = "Tendência de Vendas (Últimos 30 Dias)",
                    Background = OxyColors.White,
                    TitleFontSize = 18
                };

                var dateAxis = new DateTimeAxis 
                { 
                    Position = AxisPosition.Bottom,
                    StringFormat = "dd/MM",
                    Title = "Data",
                    IntervalType = DateTimeIntervalType.Days,
                    MinorIntervalType = DateTimeIntervalType.Days
                };
                
                var valueAxis = new LinearAxis 
                { 
                    Position = AxisPosition.Left,
                    Title = "Valor Total (Kz)",
                    StringFormat = "N0",
                    MinimumPadding = 0.1,
                    MaximumPadding = 0.1
                };

                plotModel.Axes.Add(dateAxis);
                plotModel.Axes.Add(valueAxis);

                var lineSeries = new LineSeries
                {
                    Title = "Vendas Diárias",
                    Color = OxyColor.FromRgb(76, 175, 80),
                    StrokeThickness = 3,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 5,
                    MarkerFill = OxyColor.FromRgb(76, 175, 80),
                    MarkerStroke = OxyColors.White,
                    MarkerStrokeThickness = 2
                };

                // Get real sales data from last 30 days
                var endDate = DateTime.Today;
                var startDate = endDate.AddDays(-30);
                
                System.Diagnostics.Debug.WriteLine($"[SalesTrend] Buscando vendas de {startDate:yyyy-MM-dd} a {endDate:yyyy-MM-dd}");
                
                var invoicesResult = await _invoiceService.GetByDateRangeAsync(startDate, endDate);
                
                if (invoicesResult.Success && invoicesResult.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesTrend] Encontradas {invoicesResult.Data.Count()} faturas");
                    
                    var paidInvoices = invoicesResult.Data
                        .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed)
                        .ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"[SalesTrend] {paidInvoices.Count} faturas pagas/confirmadas");
                    
                    foreach (var inv in paidInvoices)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SalesTrend] - Fatura {inv.InvoiceNumber}, Data: {inv.Date:yyyy-MM-dd HH:mm:ss}, Date.Date: {inv.Date.Date:yyyy-MM-dd}, Total: {inv.Total}");
                    }
                    
                    var salesByDay = paidInvoices
                        .GroupBy(i => i.Date.Date)
                        .Select(g => new { Date = g.Key, Total = g.Sum(i => i.Total) })
                        .OrderBy(x => x.Date)
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"[SalesTrend] {salesByDay.Count} dias com vendas:");
                    foreach (var day in salesByDay)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SalesTrend]   {day.Date:yyyy-MM-dd} = {day.Total:N2} Kz");
                    }

                    // Fill missing days with zero
                    for (int i = 0; i <= 30; i++)
                    {
                        var date = startDate.AddDays(i);
                        var dayData = salesByDay.FirstOrDefault(s => s.Date == date);
                        var value = dayData?.Total ?? 0;
                        
                        if (value > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"[SalesTrend] Adicionando ponto: {date:yyyy-MM-dd} = {value:N2} Kz");
                        }
                        
                        lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(date), (double)value));
                    }
                }
                else
                {
                    // If no data, show empty chart
                    for (int i = 0; i <= 30; i++)
                    {
                        var date = startDate.AddDays(i);
                        lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(date), 0));
                    }
                }

                plotModel.Series.Add(lineSeries);
                SalesTrendChart = plotModel;
                
                System.Diagnostics.Debug.WriteLine($"[SalesTrend] Gráfico gerado com {lineSeries.Points.Count} pontos");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesTrend] Erro ao gerar gráfico de tendências: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SalesTrend] Stack trace: {ex.StackTrace}");
            }
        }

        private async Task GenerateInvoiceStatusChartAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[InvoiceStatus] Iniciando geração do gráfico de status");
                
                var plotModel = new PlotModel 
                { 
                    Title = "Distribuição de Faturas",
                    Background = OxyColors.White,
                    TitleFontSize = 18
                };

                var pieSeries = new PieSeries
                {
                    StrokeThickness = 2,
                    InsideLabelPosition = 0.8,
                    AngleSpan = 360,
                    StartAngle = 0
                };

                // Get real data from database
                System.Diagnostics.Debug.WriteLine($"[InvoiceStatus] Buscando faturas de {StartDate:yyyy-MM-dd} a {EndDate:yyyy-MM-dd}");
                
                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                
                System.Diagnostics.Debug.WriteLine($"[InvoiceStatus] Resultado da busca: Success={invoicesResult.Success}, Data={invoicesResult.Data != null}");
                
                if (invoicesResult.Success && invoicesResult.Data != null)
                {
                    var invoices = invoicesResult.Data.ToList();
                    var total = invoices.Count;
                    
                    System.Diagnostics.Debug.WriteLine($"[InvoiceStatus] {total} faturas encontradas");
                    
                    if (total > 0)
                    {
                        var statusGroups = invoices
                            .GroupBy(i => i.Status)
                            .Select(g => new { Status = g.Key, Count = g.Count(), Percentage = (g.Count() * 100.0 / total) })
                            .OrderByDescending(x => x.Count)
                            .ToList();
                        
                        System.Diagnostics.Debug.WriteLine($"[InvoiceStatus] {statusGroups.Count} grupos de status");

                        foreach (var group in statusGroups)
                        {
                            var statusName = group.Status switch
                            {
                                InvoiceStatus.Paid => "Pagas",
                                InvoiceStatus.Confirmed => "Confirmadas",
                                InvoiceStatus.Pending => "Pendentes",
                                InvoiceStatus.Cancelled => "Canceladas",
                                InvoiceStatus.Refunded => "Reembolsadas",
                                InvoiceStatus.Draft => "Rascunho",
                                _ => group.Status.ToString()
                            };

                            var color = group.Status switch
                            {
                                InvoiceStatus.Paid => OxyColor.FromRgb(76, 175, 80),      // Green
                                InvoiceStatus.Confirmed => OxyColor.FromRgb(33, 150, 243),  // Blue
                                InvoiceStatus.Pending => OxyColor.FromRgb(255, 152, 0),    // Orange
                                InvoiceStatus.Cancelled => OxyColor.FromRgb(244, 67, 54),  // Red
                                InvoiceStatus.Refunded => OxyColor.FromRgb(156, 39, 176),  // Purple
                                InvoiceStatus.Draft => OxyColor.FromRgb(158, 158, 158),    // Gray
                                _ => OxyColor.FromRgb(0, 188, 212)                         // Cyan
                            };

                            pieSeries.Slices.Add(new PieSlice(statusName, group.Percentage) { Fill = color });
                        }
                    }
                    else
                    {
                        // No data - show empty state
                        pieSeries.Slices.Add(new PieSlice("Sem dados", 100) { Fill = OxyColor.FromRgb(200, 200, 200) });
                    }
                }
                else
                {
                    // Error or no data - show empty state
                    pieSeries.Slices.Add(new PieSlice("Sem dados", 100) { Fill = OxyColor.FromRgb(200, 200, 200) });
                }

                plotModel.Series.Add(pieSeries);
                InvoiceStatusChart = plotModel;
                
                System.Diagnostics.Debug.WriteLine("[InvoiceStatus] Gráfico de status gerado com sucesso");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[InvoiceStatus] Erro ao gerar gráfico de status: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[InvoiceStatus] Stack trace: {ex.StackTrace}");
            }
        }

        private void GenerateTopProductsChartData()
        {
            try
            {
                if (TopProducts == null || TopProducts.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Nenhum produto disponível para gerar o gráfico");
                    return;
                }

                var plotModel = new PlotModel 
                { 
                    Title = "Top 10 Produtos",
                    Background = OxyColors.White,
                    TitleFontSize = 18
                };

                var categoryAxis = new CategoryAxis 
                { 
                    Position = AxisPosition.Left,
                    MinorStep = 1
                };
                
                var valueAxis = new LinearAxis 
                { 
                    Position = AxisPosition.Bottom,
                    Title = "Quantidade Vendida",
                    MinimumPadding = 0.1,
                    MaximumPadding = 0.1
                };

                plotModel.Axes.Add(categoryAxis);
                plotModel.Axes.Add(valueAxis);

                var barSeries = new BarSeries
                {
                    FillColor = OxyColor.FromRgb(156, 39, 176),
                    StrokeColor = OxyColors.White,
                    StrokeThickness = 2
                };

                foreach (var product in TopProducts.Take(10))
                {
                    var productName = product.ProductName?.Length > 30 
                        ? product.ProductName.Substring(0, 27) + "..." 
                        : product.ProductName;
                    categoryAxis.Labels.Add(productName);
                    barSeries.Items.Add(new BarItem { Value = product.QuantitySold });
                }

                plotModel.Series.Add(barSeries);
                TopProductsChart = plotModel;
                
                System.Diagnostics.Debug.WriteLine($"Gráfico Top Produtos gerado com {TopProducts.Count} produtos");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao gerar gráfico Top Produtos: {ex.Message}");
                ShowMessage($"Erro ao gerar gráfico de produtos: {ex.Message}", true);
            }
        }

        private void GeneratePaymentMethodsChartData()
        {
            try
            {
                if (PaymentMethods == null || PaymentMethods.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Nenhuma forma de pagamento disponível para gerar o gráfico");
                    return;
                }

                var plotModel = new PlotModel 
                { 
                    Title = "Formas de Pagamento",
                    Background = OxyColors.White,
                    TitleFontSize = 18
                };

                var pieSeries = new PieSeries
                {
                    StrokeThickness = 2,
                    InsideLabelPosition = 0.8,
                    AngleSpan = 360,
                    StartAngle = 0,
                    OutsideLabelFormat = "{1}: {2:0.0}%"
                };

                var colors = new[] 
                {
                    OxyColor.FromRgb(33, 150, 243),   // Blue
                    OxyColor.FromRgb(76, 175, 80),    // Green
                    OxyColor.FromRgb(255, 152, 0),    // Orange
                    OxyColor.FromRgb(156, 39, 176),   // Purple
                    OxyColor.FromRgb(244, 67, 54),    // Red
                    OxyColor.FromRgb(0, 188, 212),    // Cyan
                    OxyColor.FromRgb(255, 235, 59)    // Yellow
                };

                int colorIndex = 0;
                foreach (var method in PaymentMethods)
                {
                    pieSeries.Slices.Add(new PieSlice(method.MethodName, (double)method.Percentage) 
                    { 
                        Fill = colors[colorIndex % colors.Length],
                        IsExploded = false
                    });
                    colorIndex++;
                }

                plotModel.Series.Add(pieSeries);
                PaymentMethodsChart = plotModel;
                
                System.Diagnostics.Debug.WriteLine($"Gráfico de Formas de Pagamento gerado com {PaymentMethods.Count} métodos");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao gerar gráfico de formas de pagamento: {ex.Message}");
                ShowMessage($"Erro ao gerar gráfico de pagamentos: {ex.Message}", true);
            }
        }

        #endregion

        #region Sales Reports

        private async Task GenerateSalesByPeriodAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório de vendas por período...");

                var result = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (result.Success && result.Data != null)
                {
                    var invoices = result.Data
                        .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed);

                    // Apply filters
                    if (SelectedUserId.HasValue)
                    {
                        invoices = invoices.Where(i => i.UserId == SelectedUserId.Value);
                    }

                    // Group by selected period
                    var grouped = SelectedPeriodGrouping switch
                    {
                        "Dia" => invoices.GroupBy(i => i.Date.Date),
                        "Semana" => invoices.GroupBy(i => GetWeekStart(i.Date)),
                        "Mês" => invoices.GroupBy(i => new DateTime(i.Date.Year, i.Date.Month, 1)),
                        "Trimestre" => invoices.GroupBy(i => GetQuarterStart(i.Date)),
                        "Ano" => invoices.GroupBy(i => new DateTime(i.Date.Year, 1, 1)),
                        _ => invoices.GroupBy(i => i.Date.Date)
                    };

                    var salesData = grouped
                        .Select(g => new SalesByPeriodDto
                        {
                            Date = g.Key,
                            InvoiceCount = g.Count(),
                            TotalValue = g.Sum(i => i.Total),
                            PaidValue = g.Sum(i => i.PaidAmount),
                            PendingValue = g.Sum(i => i.Total - i.PaidAmount)
                        })
                        .OrderBy(s => s.Date)
                        .ToList();

                    SalesByPeriod = new ObservableCollection<SalesByPeriodDto>(salesData);
                    ShowMessage($"Relatório gerado: {salesData.Count} registros encontrados.");
                }
                else
                {
                    ShowMessage($"Erro ao gerar relatório: {result.Message}", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateTopProductsAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório de produtos mais vendidos...");

                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar faturas.", true);
                    System.Diagnostics.Debug.WriteLine("TopProducts: Nenhuma fatura encontrada");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"TopProducts: {invoicesResult.Data.Count()} faturas encontradas");

                var invoiceIds = invoicesResult.Data
                    .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed)
                    .Select(i => i.InvoiceId)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"TopProducts: {invoiceIds.Count} faturas pagas/confirmadas");

                var productSales = new Dictionary<int, (string Name, int Quantity, decimal TotalValue, decimal CostValue)>();
                int totalItemsProcessed = 0;

                foreach (var invoiceId in invoiceIds)
                {
                    var itemsResult = await _invoiceProductService.GetByInvoiceIdAsync(invoiceId);
                    
                    if (!itemsResult.Success)
                    {
                        System.Diagnostics.Debug.WriteLine($"TopProducts: Erro ao buscar itens da fatura {invoiceId}: {itemsResult.Message}");
                        continue;
                    }
                    
                    if (itemsResult.Data == null || !itemsResult.Data.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"TopProducts: Fatura {invoiceId} não tem produtos associados");
                        continue;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"TopProducts: Fatura {invoiceId} tem {itemsResult.Data.Count()} produtos");
                    
                    foreach (var item in itemsResult.Data)
                    {
                        totalItemsProcessed++;
                        
                        // Get product cost
                        var productResult = await _productService.GetByIdAsync(item.ProductId);
                        var costPrice = productResult.Success && productResult.Data != null ? productResult.Data.CostPrice : 0;

                        if (productSales.ContainsKey(item.ProductId))
                        {
                            var current = productSales[item.ProductId];
                            productSales[item.ProductId] = (
                                current.Name,
                                current.Quantity + item.Quantity,
                                current.TotalValue + (item.UnitPrice * item.Quantity),
                                current.CostValue + (costPrice * item.Quantity)
                            );
                        }
                        else
                        {
                            productSales[item.ProductId] = (
                                item.ProductName ?? "Desconhecido",
                                item.Quantity,
                                item.UnitPrice * item.Quantity,
                                costPrice * item.Quantity
                            );
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"TopProducts: {totalItemsProcessed} itens de fatura processados no total");

                System.Diagnostics.Debug.WriteLine($"TopProducts: {productSales.Count} produtos únicos processados");

                var topProducts = productSales
                    .Select(kvp => new TopProductDto
                    {
                        ProductName = kvp.Value.Name,
                        QuantitySold = kvp.Value.Quantity,
                        Revenue = kvp.Value.TotalValue
                       
                    })
                    .OrderByDescending(p => p.Revenue)
                    .Take(20)
                    .ToList();

                var maxRevenue = topProducts.Count > 0 ? topProducts.Max(p => p.Revenue) : 0;
                if (maxRevenue > 0)
                {
                    foreach (var product in topProducts)
                    {
                        product.ProgressPercentage = Math.Round((double)(product.Revenue / maxRevenue) * 100.0, 2);
                    }
                }

                TopProducts = new ObservableCollection<TopProductDto>(topProducts);
                
                System.Diagnostics.Debug.WriteLine($"TopProducts: {topProducts.Count} produtos no resultado final");
                
                if (topProducts.Count > 0)
                {
                    GenerateTopProductsChartData();
                    ShowMessage($"Top {topProducts.Count} produtos carregados.");
                }
                else
                {
                    ShowMessage("Nenhum produto vendido no período selecionado.", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateSalesByCustomerAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório de vendas por cliente...");

                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar faturas.", true);
                    return;
                }

                var customerSales = invoicesResult.Data
                    .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed)
                    .GroupBy(i => i.PersonId)
                    .Select(g => new { PersonId = g.Key, Invoices = g.ToList() })
                    .ToList();

                var salesByCustomer = new List<SalesByCustomerDto>();

                foreach (var group in customerSales)
                {
                    var personResult = await _personService.GetByIdAsync(group.PersonId);
                    var customerName = personResult.Success && personResult.Data != null
                        ? personResult.Data.Name
                        : "Cliente Desconhecido";

                    salesByCustomer.Add(new SalesByCustomerDto
                    {
                        CustomerName = customerName,
                        InvoiceCount = group.Invoices.Count,
                        TotalValue = group.Invoices.Sum(i => i.Total),
                        PaidValue = group.Invoices.Sum(i => i.PaidAmount),
                        PendingValue = group.Invoices.Sum(i => i.Total - i.PaidAmount),
                        LastPurchaseDate = group.Invoices.Max(i => i.Date)
                    });
                }

                SalesByCustomer = new ObservableCollection<SalesByCustomerDto>(
                    salesByCustomer.OrderByDescending(s => s.TotalValue));

                ShowMessage($"{salesByCustomer.Count} clientes encontrados.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateProfitMarginAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório de margem de lucro...");

                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar faturas.", true);
                    return;
                }

                var profitData = new List<ProfitMarginDto>();

                foreach (var invoice in invoicesResult.Data.Where(i => i.Status == InvoiceStatus.Paid))
                {
                    var itemsResult = await _invoiceProductService.GetByInvoiceIdAsync(invoice.InvoiceId);
                    if (itemsResult.Success && itemsResult.Data != null)
                    {
                        decimal totalCost = 0;
                        foreach (var item in itemsResult.Data)
                        {
                            var productResult = await _productService.GetByIdAsync(item.ProductId);
                            if (productResult.Success && productResult.Data != null)
                            {
                                totalCost += productResult.Data.CostPrice * item.Quantity;
                            }
                        }

                        var grossProfit = invoice.Total - totalCost;
                        var marginPercentage = invoice.Total > 0 ? (grossProfit / invoice.Total) * 100 : 0;

                        profitData.Add(new ProfitMarginDto
                        {
                            InvoiceNumber = invoice.InvoiceNumber,
                            InvoiceDate = invoice.Date,
                            TotalRevenue = invoice.Total,
                            TotalCost = totalCost,
                            GrossProfit = grossProfit,
                            MarginPercentage = marginPercentage
                        });
                    }
                }

                ProfitMargins = new ObservableCollection<ProfitMarginDto>(profitData.OrderByDescending(p => p.GrossProfit));
                TotalProfitMargin = profitData.Any() ? profitData.Average(p => p.MarginPercentage) : 0;
                ShowMessage($"{profitData.Count} faturas analisadas.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateInvoicesByStatusAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório de faturas por status...");

                var result = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (result.Success && result.Data != null)
                {
                    var statusGroups = result.Data
                        .GroupBy(i => i.Status)
                        .Select(g => new InvoicesByStatusDto
                        {
                            Status = g.Key.ToString(),
                            Count = g.Count(),
                            TotalValue = g.Sum(i => i.Total),
                            Percentage = 0
                        })
                        .ToList();

                    var totalCount = statusGroups.Sum(s => s.Count);
                    foreach (var status in statusGroups)
                    {
                        status.Percentage = totalCount > 0 ? (decimal)status.Count / totalCount * 100 : 0;
                    }

                    InvoicesByStatus = new ObservableCollection<InvoicesByStatusDto>(statusGroups);
                    ShowMessage($"Relatório gerado com {statusGroups.Count} status.");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Financial Reports

        private async Task GenerateCashFlowAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório de fluxo de caixa...");

                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar faturas.", true);
                    return;
                }

                var cashFlow = invoicesResult.Data
                    .GroupBy(i => i.Date.Date)
                    .Select(g => new CashFlowDto
                    {
                        Date = g.Key,
                        Inflow = g.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.PaidAmount),
                        Outflow = 0, // TODO: Implement expenses/costs
                        Balance = g.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.PaidAmount)
                    })
                    .OrderBy(cf => cf.Date)
                    .ToList();

                // Calculate accumulated balance
                decimal accumulated = 0;
                foreach (var day in cashFlow)
                {
                    accumulated += (day.Inflow - day.Outflow);
                    day.Balance = accumulated;
                }

                CashFlow = new ObservableCollection<CashFlowDto>(cashFlow);
                ShowMessage($"Fluxo de caixa gerado: {cashFlow.Count} dias.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GeneratePaymentMethodsAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório de formas de pagamento...");

                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar faturas.", true);
                    System.Diagnostics.Debug.WriteLine("PaymentMethods: Nenhuma fatura encontrada");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"PaymentMethods: {invoicesResult.Data.Count()} faturas encontradas");

                var invoiceIds = invoicesResult.Data
                    .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed)
                    .Select(i => i.InvoiceId)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"PaymentMethods: {invoiceIds.Count} faturas pagas/confirmadas");

                var paymentMethods = new Dictionary<string, (int Count, decimal Total)>();
                int totalPaymentsProcessed = 0;

                foreach (var invoiceId in invoiceIds)
                {
                    var paymentsResult = await _paymentService.GetByInvoiceIdAsync(invoiceId);
                    
                    if (!paymentsResult.Success)
                    {
                        System.Diagnostics.Debug.WriteLine($"PaymentMethods: Erro ao buscar pagamentos da fatura {invoiceId}: {paymentsResult.Message}");
                        continue;
                    }
                    
                    if (paymentsResult.Data == null || !paymentsResult.Data.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"PaymentMethods: Fatura {invoiceId} não tem pagamentos associados");
                        continue;
                    }
                    
                    var allPayments = paymentsResult.Data.ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"PaymentMethods: Fatura {invoiceId} tem {allPayments.Count} pagamentos");
                    
                    totalPaymentsProcessed += allPayments.Count;

                    foreach (var payment in allPayments)
                    {
                        var key = $"Tipo {payment.PaymentTypeId}"; // TODO: Get actual payment type name

                        if (paymentMethods.ContainsKey(key))
                        {
                            var current = paymentMethods[key];
                            paymentMethods[key] = (current.Count + 1, current.Total + payment.Amount);
                        }
                        else
                        {
                            paymentMethods[key] = (1, payment.Amount);
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"PaymentMethods: {totalPaymentsProcessed} pagamentos processados");
                
                // Get payment type names
                var paymentTypesResult = await _paymentTypeService.GetAllAsync();
                var paymentTypesDict = new Dictionary<int, string>();
                if (paymentTypesResult.Success && paymentTypesResult.Data != null)
                {
                    foreach (var pt in paymentTypesResult.Data)
                    {
                        paymentTypesDict[pt.PaymentTypeId] = pt.Name;
                    }
                }

                var methodsList = paymentMethods
                    .Select(kvp => 
                    {
                        // Extract payment type ID from key "Tipo {id}"
                        var typeIdStr = kvp.Key.Replace("Tipo ", "");
                        var methodName = kvp.Key;
                        
                        if (int.TryParse(typeIdStr, out int typeId) && paymentTypesDict.ContainsKey(typeId))
                        {
                            methodName = paymentTypesDict[typeId];
                        }
                        
                        return new PaymentMethodDto
                        {
                            MethodName = methodName,
                            TransactionCount = kvp.Value.Count,
                            TotalValue = kvp.Value.Total,
                            Percentage = 0 // Will be calculated after
                        };
                    })
                    .OrderByDescending(p => p.TotalValue)
                    .ToList();

                var totalValue = methodsList.Sum(p => p.TotalValue);
                foreach (var method in methodsList)
                {
                    method.Percentage = totalValue > 0 ? (method.TotalValue / totalValue) * 100 : 0;
                }

                PaymentMethods = new ObservableCollection<PaymentMethodDto>(methodsList);
                
                System.Diagnostics.Debug.WriteLine($"PaymentMethods: {methodsList.Count} métodos diferentes encontrados");
                
                // Generate chart after data is loaded
                if (methodsList.Count > 0)
                {
                    GeneratePaymentMethodsChartData();
                    ShowMessage($"{methodsList.Count} formas de pagamento encontradas.");
                }
                else
                {
                    ShowMessage("Nenhuma forma de pagamento encontrada no período selecionado.", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateAccountsReceivableAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório de contas a receber...");

                var invoicesResult = await _invoiceService.GetAllAsync();
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar faturas.", true);
                    return;
                }

                var pendingInvoices = invoicesResult.Data
                    .Where(i => i.Status == InvoiceStatus.Pending ||
                                i.Status == InvoiceStatus.Confirmed)
                    .ToList();

                var accountsReceivable = new List<AccountsReceivableDto>();

                foreach (var invoice in pendingInvoices)
                {
                    var personResult = await _personService.GetByIdAsync(invoice.PersonId);
                    var customerName = personResult.Success && personResult.Data != null
                        ? personResult.Data.Name
                        : "Cliente Desconhecido";

                    var daysOverdue = invoice.DueDate.HasValue && invoice.DueDate.Value < DateTime.Today
                        ? (DateTime.Today - invoice.DueDate.Value).Days
                        : 0;

                    accountsReceivable.Add(new AccountsReceivableDto
                    {
                        InvoiceNumber = invoice.InvoiceNumber,
                        CustomerName = customerName,
                        InvoiceDate = invoice.Date,
                        DueDate = invoice.DueDate,
                        TotalValue = invoice.Total,
                        PaidValue = invoice.PaidAmount,
                        PendingValue = invoice.Total - invoice.PaidAmount,
                        DaysOverdue = daysOverdue,
                        Status = daysOverdue > 0 ? "Vencida" : "Em Dia"
                    });
                }

                AccountsReceivable = new ObservableCollection<AccountsReceivableDto>(
                    accountsReceivable.OrderByDescending(a => a.DaysOverdue));

                ShowMessage($"{accountsReceivable.Count} faturas pendentes encontradas.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Stock Reports

        private async Task GenerateStockMovementsAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório de movimentação de estoque...");

                // TODO: Implement when IStockMovementService has date range filter
                ShowMessage("Relatório de movimentação em desenvolvimento.", true);
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateLowStockAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório de estoque baixo...");

                var stocksResult = await _stockService.GetLowStockAsync();
                if (!stocksResult.Success || stocksResult.Data == null)
                {
                    ShowMessage("Erro ao buscar estoque baixo.", true);
                    return;
                }

                var lowStock = new List<LowStockDto>();

                foreach (var stock in stocksResult.Data)
                {
                    var productResult = await _productService.GetByIdAsync(stock.ProductId);
                    if (productResult.Success && productResult.Data != null)
                    {
                        var product = productResult.Data;
                        lowStock.Add(new LowStockDto
                        {
                            ProductName = product.Name,
                            CurrentQuantity = stock.Quantity,
                            MinimumQuantity = stock.MinimumStock ?? 0,
                            ReorderPoint = stock.ReorderPoint ?? 0,
                            Difference = (stock.MinimumStock ?? 0) - stock.Quantity,
                            Status = stock.Quantity == 0 ? "Sem Estoque" : "Estoque Baixo"
                        });
                    }
                }

                LowStockProducts = new ObservableCollection<LowStockDto>(
                    lowStock.OrderBy(l => l.CurrentQuantity));

                ShowMessage($"{lowStock.Count} produtos com estoque baixo.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region Gerar relatorios PDF

        private async Task GenerateExpiringProductsAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório de produtos vencendo...");

                var expirationsResult = await _expirationService.GetExpiringAsync(30);
                if (!expirationsResult.Success || expirationsResult.Data == null)
                {
                    ShowMessage("Erro ao buscar produtos vencendo.", true);
                    return;
                }

                var expiringProducts = new List<ExpirationReportDto>();

                foreach (var expiration in expirationsResult.Data)
                {
                    var productResult = await _productService.GetByIdAsync(expiration.ProductId);
                    if (productResult.Success && productResult.Data != null)
                    {
                        var product = productResult.Data;
                        var daysToExpire = (expiration.ExpirationDate - DateTime.Today).Days;

                        expiringProducts.Add(new ExpirationReportDto
                        {
                            ProductName = product.Name,
                            Batch = expiration.BatchNumber ?? "N/A",
                            ExpirationDate = expiration.ExpirationDate,
                            DaysToExpire = daysToExpire,
                            Quantity = expiration.Quantity,
                            Status = daysToExpire <= 0 ? "Vencido" :
                                     daysToExpire <= 7 ? "Vence em 7 dias" :
                                     "Vence em 30 dias"
                        });
                    }
                }

                ExpiringProducts = new ObservableCollection<ExpirationReportDto>(
                    expiringProducts.OrderBy(e => e.ExpirationDate));

                ShowMessage($"{expiringProducts.Count} produtos vencendo nos próximos 30 dias.");
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
                Debug.WriteLine($"Erro ao gerar relatório: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateSalesByPeriodPdfAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório PDF de vendas por período...");

                var companyConfig = await GetCompanyConfigAsync();
                
                var result = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!result.Success || result.Data == null)
                {
                    ShowMessage($"Erro ao buscar dados: {result.Message}", true);
                    return;
                }

                var invoices = result.Data
                    .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed);

                if (SelectedUserId.HasValue)
                {
                    invoices = invoices.Where(i => i.UserId == SelectedUserId.Value);
                }

                var grouped = SelectedPeriodGrouping switch
                {
                    "Dia" => invoices.GroupBy(i => i.Date.Date),
                    "Semana" => invoices.GroupBy(i => GetWeekStart(i.Date)),
                    "Mês" => invoices.GroupBy(i => new DateTime(i.Date.Year, i.Date.Month, 1)),
                    "Trimestre" => invoices.GroupBy(i => GetQuarterStart(i.Date)),
                    "Ano" => invoices.GroupBy(i => new DateTime(i.Date.Year, 1, 1)),
                    _ => invoices.GroupBy(i => i.Date.Date)
                };

                var salesData = grouped
                    .Select(g => new SalesByPeriodDto
                    {
                        Date = g.Key,
                        InvoiceCount = g.Count(),
                        TotalValue = g.Sum(i => i.Total),
                        PaidValue = g.Sum(i => i.PaidAmount),
                        PendingValue = g.Sum(i => i.Total - i.PaidAmount)
                    })
                    .OrderBy(s => s.Date)
                    .ToList();

                if (!salesData.Any())
                {
                    ShowMessage("Nenhum dado encontrado para o período selecionado.", true);
                    return;
                }

                var fileName = $"Vendas_Por_Periodo_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine(myDocuments, fileName);

                await _pdfGeneratorService.GenerateSalesByPeriodReportAsync(
                    companyConfig,
                    salesData,
                    StartDate,
                    EndDate,
                    filePath);

                ShowMessage($"Relatório PDF gerado: {fileName}");
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
                Debug.WriteLine($"Erro ao gerar relatório: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateTopProductsPdfAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório PDF de top produtos...");

                var companyConfig = await GetCompanyConfigAsync();

                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar faturas.", true);
                    return;
                }

                var invoiceIds = invoicesResult.Data
                    .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed)
                    .Select(i => i.InvoiceId)
                    .ToList();

                var productSales = new Dictionary<int, (string Name, int Quantity, decimal TotalValue)>();

                foreach (var invoiceId in invoiceIds)
                {
                    var itemsResult = await _invoiceProductService.GetByInvoiceIdAsync(invoiceId);
                    if (itemsResult.Success && itemsResult.Data != null)
                    {
                        foreach (var item in itemsResult.Data)
                        {
                            if (productSales.ContainsKey(item.ProductId))
                            {
                                var current = productSales[item.ProductId];
                                productSales[item.ProductId] = (
                                    current.Name,
                                    current.Quantity + item.Quantity,
                                    current.TotalValue + (item.UnitPrice * item.Quantity)
                                );
                            }
                            else
                            {
                                productSales[item.ProductId] = (
                                    item.ProductName ?? "Desconhecido",
                                    item.Quantity,
                                    item.UnitPrice * item.Quantity
                                );
                            }
                        }
                    }
                }

                var topProducts = productSales
                    .Select(kvp => new TopProductDto
                    {
                        ProductName = kvp.Value.Name,
                        QuantitySold = kvp.Value.Quantity,
                        Revenue = kvp.Value.TotalValue
                    })
                    .OrderByDescending(p => p.Revenue)
                    .Take(20)
                    .ToList();

                if (!topProducts.Any())
                {
                    ShowMessage("Nenhum produto vendido no período selecionado.", true);
                    return;
                }

                var fileName = $"Top_Produtos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine(myDocuments, fileName);

                await _pdfGeneratorService.GenerateTopProductsReportAsync(
                    companyConfig,
                    topProducts,
                    StartDate,
                    EndDate,
                    filePath);

                ShowMessage($"Relatório PDF gerado: {fileName}");
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
                Debug.WriteLine($"Erro ao gerar relatório: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateSalesByCustomerPdfAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório PDF de vendas por cliente...");

                var companyConfig = await GetCompanyConfigAsync();

                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar faturas.", true);
                    return;
                }

                var customerSales = invoicesResult.Data
                    .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed)
                    .GroupBy(i => i.PersonId)
                    .Select(g => new { PersonId = g.Key, Invoices = g.ToList() })
                    .ToList();

                var salesByCustomer = new List<SalesByCustomerDto>();

                foreach (var group in customerSales)
                {
                    var personResult = await _personService.GetByIdAsync(group.PersonId);
                    var customerName = personResult.Success && personResult.Data != null
                        ? personResult.Data.Name
                        : "Cliente Desconhecido";

                    salesByCustomer.Add(new SalesByCustomerDto
                    {
                        CustomerName = customerName,
                        InvoiceCount = group.Invoices.Count,
                        TotalValue = group.Invoices.Sum(i => i.Total),
                        PaidValue = group.Invoices.Sum(i => i.PaidAmount),
                        PendingValue = group.Invoices.Sum(i => i.Total - i.PaidAmount),
                        LastPurchaseDate = group.Invoices.Max(i => i.Date)
                    });
                }

                if (!salesByCustomer.Any())
                {
                    ShowMessage("Nenhum cliente encontrado no período selecionado.", true);
                    return;
                }

                var fileName = $"Vendas_Por_Cliente_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine(myDocuments, fileName);

                await _pdfGeneratorService.GenerateSalesByCustomerReportAsync(
                    companyConfig,
                    salesByCustomer.OrderByDescending(s => s.TotalValue),
                    StartDate,
                    EndDate,
                    filePath);

                ShowMessage($"Relatório PDF gerado: {fileName}");
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
                Debug.WriteLine($"Erro ao gerar relatório: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateProfitMarginPdfAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório PDF de margem de lucro...");

                var companyConfig = await GetCompanyConfigAsync();

                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar faturas.", true);
                    return;
                }

                var profitData = new List<ProfitMarginDto>();

                foreach (var invoice in invoicesResult.Data.Where(i => i.Status == InvoiceStatus.Paid))
                {
                    var itemsResult = await _invoiceProductService.GetByInvoiceIdAsync(invoice.InvoiceId);
                    if (itemsResult.Success && itemsResult.Data != null)
                    {
                        decimal totalCost = 0;
                        foreach (var item in itemsResult.Data)
                        {
                            var productResult = await _productService.GetByIdAsync(item.ProductId);
                            if (productResult.Success && productResult.Data != null)
                            {
                                totalCost += productResult.Data.CostPrice * item.Quantity;
                            }
                        }

                        var grossProfit = invoice.Total - totalCost;
                        var marginPercentage = invoice.Total > 0 ? (grossProfit / invoice.Total) * 100 : 0;

                        profitData.Add(new ProfitMarginDto
                        {
                            InvoiceNumber = invoice.InvoiceNumber,
                            InvoiceDate = invoice.Date,
                            TotalRevenue = invoice.Total,
                            TotalCost = totalCost,
                            GrossProfit = grossProfit,
                            MarginPercentage = marginPercentage
                        });
                    }
                }

                if (!profitData.Any())
                {
                    ShowMessage("Nenhum dado de margem encontrado no período selecionado.", true);
                    return;
                }

                var fileName = $"Margem_Lucro_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine(myDocuments, fileName);

                await _pdfGeneratorService.GenerateProfitMarginReportAsync(
                    companyConfig,
                    profitData.OrderByDescending(p => p.GrossProfit),
                    StartDate,
                    EndDate,
                    filePath);

                ShowMessage($"Relatório PDF gerado: {fileName}");
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
                Debug.WriteLine($"Erro ao gerar relatório: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateInvoicesByStatusPdfAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório PDF de faturas por status...");

                var companyConfig = await GetCompanyConfigAsync();

                var result = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!result.Success || result.Data == null)
                {
                    ShowMessage("Erro ao buscar faturas.", true);
                    return;
                }

                var statusGroups = result.Data
                    .GroupBy(i => i.Status)
                    .Select(g => new InvoicesByStatusDto
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count(),
                        TotalValue = g.Sum(i => i.Total),
                        Percentage = 0
                    })
                    .ToList();

                var totalCount = statusGroups.Sum(s => s.Count);
                foreach (var status in statusGroups)
                {
                    status.Percentage = totalCount > 0 ? (decimal)status.Count / totalCount * 100 : 0;
                }

                if (!statusGroups.Any())
                {
                    ShowMessage("Nenhum dado encontrado no período selecionado.", true);
                    return;
                }

                var fileName = $"Faturas_Por_Status_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine(myDocuments, fileName);

                await _pdfGeneratorService.GenerateInvoicesByStatusReportAsync(
                    companyConfig,
                    statusGroups,
                    StartDate,
                    EndDate,
                    filePath);

                ShowMessage($"Relatório PDF gerado: {fileName}");
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
                Debug.WriteLine($"Erro ao gerar relatório: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateCashFlowPdfAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório PDF de fluxo de caixa...");

                var companyConfig = await GetCompanyConfigAsync();

                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar faturas.", true);
                    return;
                }

                var cashFlow = invoicesResult.Data
                    .GroupBy(i => i.Date.Date)
                    .Select(g => new CashFlowDto
                    {
                        Date = g.Key,
                        Inflow = g.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.PaidAmount),
                        Outflow = 0,
                        Balance = g.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.PaidAmount)
                    })
                    .OrderBy(cf => cf.Date)
                    .ToList();

                decimal accumulated = 0;
                foreach (var day in cashFlow)
                {
                    accumulated += (day.Inflow - day.Outflow);
                    day.Balance = accumulated;
                }

                if (!cashFlow.Any())
                {
                    ShowMessage("Nenhum dado de fluxo encontrado no período selecionado.", true);
                    return;
                }

                var fileName = $"Fluxo_Caixa_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine(myDocuments, fileName);

                await _pdfGeneratorService.GenerateCashFlowReportAsync(
                    companyConfig,
                    cashFlow,
                    StartDate,
                    EndDate,
                    filePath);

                ShowMessage($"Relatório PDF gerado: {fileName}");
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
                Debug.WriteLine($"Erro ao gerar relatório: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GeneratePaymentMethodsPdfAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório PDF de formas de pagamento...");

                var companyConfig = await GetCompanyConfigAsync();

                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar faturas.", true);
                    return;
                }

                var invoiceIds = invoicesResult.Data
                    .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed)
                    .Select(i => i.InvoiceId)
                    .ToList();

                var paymentMethods = new Dictionary<string, (int Count, decimal Total)>();

                foreach (var invoiceId in invoiceIds)
                {
                    var paymentsResult = await _paymentService.GetByInvoiceIdAsync(invoiceId);
                    if (paymentsResult.Success && paymentsResult.Data != null)
                    {
                        foreach (var payment in paymentsResult.Data)
                        {
                            var key = $"Tipo {payment.PaymentTypeId}";

                            if (paymentMethods.ContainsKey(key))
                            {
                                var current = paymentMethods[key];
                                paymentMethods[key] = (current.Count + 1, current.Total + payment.Amount);
                            }
                            else
                            {
                                paymentMethods[key] = (1, payment.Amount);
                            }
                        }
                    }
                }

                var paymentTypesResult = await _paymentTypeService.GetAllAsync();
                var paymentTypesDict = new Dictionary<int, string>();
                if (paymentTypesResult.Success && paymentTypesResult.Data != null)
                {
                    foreach (var pt in paymentTypesResult.Data)
                    {
                        paymentTypesDict[pt.PaymentTypeId] = pt.Name;
                    }
                }

                var methodsList = paymentMethods
                    .Select(kvp => 
                    {
                        var typeIdStr = kvp.Key.Replace("Tipo ", "");
                        var methodName = kvp.Key;
                        
                        if (int.TryParse(typeIdStr, out int typeId) && paymentTypesDict.ContainsKey(typeId))
                        {
                            methodName = paymentTypesDict[typeId];
                        }
                        
                        return new PaymentMethodDto
                        {
                            MethodName = methodName,
                            TransactionCount = kvp.Value.Count,
                            TotalValue = kvp.Value.Total,
                            Percentage = 0
                        };
                    })
                    .OrderByDescending(p => p.TotalValue)
                    .ToList();

                var totalValue = methodsList.Sum(p => p.TotalValue);
                foreach (var method in methodsList)
                {
                    method.Percentage = totalValue > 0 ? (method.TotalValue / totalValue) * 100 : 0;
                }

                if (!methodsList.Any())
                {
                    ShowMessage("Nenhuma forma de pagamento encontrada no período selecionado.", true);
                    return;
                }

                var fileName = $"Formas_Pagamento_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine(myDocuments, fileName);

                await _pdfGeneratorService.GeneratePaymentMethodsReportAsync(
                    companyConfig,
                    methodsList,
                    StartDate,
                    EndDate,
                    filePath);

                ShowMessage($"Relatório PDF gerado: {fileName}");
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
                Debug.WriteLine($"Erro ao gerar relatório: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateFinancialStatementAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando demonstrativo financeiro...");

                var companyConfig = await GetCompanyConfigAsync();

                // Coletar dados de vendas
                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar dados de vendas.", true);
                    return;
                }

                var invoices = invoicesResult.Data
                    .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed)
                    .ToList();

                // Carregar itens de todas as faturas para calcular custo
                decimal totalCost = 0;
                foreach (var invoice in invoices)
                {
                    var items = await GetInvoiceItemsAsync(invoice.InvoiceId);
                    foreach (var item in items)
                    {
                        var productResult = await _productService.GetByIdAsync(item.ProductId);
                        if (productResult.Success && productResult.Data != null)
                        {
                            totalCost += item.Quantity * productResult.Data.CostPrice;
                        }
                    }
                }

                var totalRevenue = invoices.Sum(i => i.Total);
                var totalProfit = totalRevenue - totalCost;
                var profitMargin = totalRevenue > 0 ? (totalProfit / totalRevenue) * 100 : 0;

                // Formas de pagamento
                var allPayments = new List<PaymentDto>();
                foreach (var invoice in invoices)
                {
                    var payments = await GetInvoicePaymentsAsync(invoice.InvoiceId);
                    allPayments.AddRange(payments);
                }

                // Agrupar por tipo de pagamento
                var paymentGroups = allPayments.GroupBy(p => p.PaymentTypeId);
                var paymentMethods = new List<PaymentMethodDto>();
                
                foreach (var group in paymentGroups)
                {
                    var paymentTypeResult = await _paymentTypeService.GetByIdAsync(group.Key);
                    var methodName = paymentTypeResult.Success && paymentTypeResult.Data != null 
                        ? paymentTypeResult.Data.Name 
                        : "Desconhecido";
                    
                    var totalAmount = group.Sum(p => p.Amount);
                    paymentMethods.Add(new PaymentMethodDto
                    {
                        MethodName = methodName,
                        TransactionCount = group.Count(),
                        TotalValue = totalAmount,
                        Percentage = totalRevenue > 0 ? (totalAmount / totalRevenue) * 100 : 0
                    });
                }
                paymentMethods = paymentMethods.OrderByDescending(p => p.TotalValue).ToList();

                // Contas a receber
                var accountsReceivable = new List<AccountsReceivableDto>();
                foreach (var invoice in invoices.Where(i => i.Total > i.PaidAmount))
                {
                    var customer = await GetInvoiceCustomerAsync(invoice.PersonId);
                    accountsReceivable.Add(new AccountsReceivableDto
                    {
                        InvoiceNumber = invoice.InvoiceNumber,
                        CustomerName = customer?.Name ?? "N/A",
                        DueDate = invoice.DueDate ?? DateTime.Now.AddDays(30),
                        TotalValue = invoice.Total,
                        PaidValue = invoice.PaidAmount,
                        PendingValue = invoice.Total - invoice.PaidAmount,
                        DaysOverdue = invoice.DueDate.HasValue && invoice.DueDate < DateTime.Today 
                            ? (DateTime.Today - invoice.DueDate.Value).Days : 0,
                        Status = invoice.Status.ToString()
                    });
                }

                // Gerar PDF
                var fileName = $"Demonstrativo_Financeiro_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                // Validar serviço
                if (_pdfGeneratorService == null)
                {
                    ShowMessage("Erro: Serviço de geração de PDF não está disponível.", true);
                    return;
                }

                // Validar parâmetros
                if (companyConfig == null)
                {
                    ShowMessage("Erro: Configuração da empresa não encontrada.", true);
                    return;
                }

                await _pdfGeneratorService.GenerateFinancialStatementReportAsync(
                    companyConfig,
                    totalRevenue,
                    totalCost,
                    totalProfit,
                    profitMargin,
                    paymentMethods ?? new List<PaymentMethodDto>(),
                    accountsReceivable ?? new List<AccountsReceivableDto>(),
                    StartDate,
                    EndDate,
                    filePath);

                ShowMessage($"Demonstrativo financeiro gerado: {fileName}");
                
                // Abrir o PDF
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar demonstrativo: {ex.Message}", true);
                Debug.WriteLine($"Erro ao gerar demonstrativo: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateExecutiveDashboardAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando dashboard executivo...");

                var companyConfig = await GetCompanyConfigAsync();

                // Coletar dados
                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar dados.", true);
                    return;
                }

                var invoices = invoicesResult.Data
                    .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed)
                    .ToList();

                // Top produtos - Carregar todos os itens primeiro
                var allItems = new List<InvoiceProductDto>();
                foreach (var invoice in invoices)
                {
                    var items = await GetInvoiceItemsAsync(invoice.InvoiceId);
                    allItems.AddRange(items);
                }

                var topProducts = allItems
                    .GroupBy(item => item.ProductId)
                    .Select(g => new TopProductDto
                    {
                        ProductName = g.First().ProductName ?? "Produto Desconhecido",
                        QuantitySold = g.Sum(item => item.Quantity),
                        Revenue = g.Sum(item => item.SubTotal)
                    })
                    .OrderByDescending(p => p.Revenue)
                    .Take(5)
                    .ToList();

                // Vendas por status
                var invoicesByStatus = invoicesResult.Data
                    .GroupBy(i => i.Status)
                    .Select(g => new InvoicesByStatusDto
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count(),
                        TotalValue = g.Sum(i => i.Total)
                    })
                    .ToList();

                // Formas de pagamento
                var allPayments = new List<PaymentDto>();
                foreach (var invoice in invoices)
                {
                    var payments = await GetInvoicePaymentsAsync(invoice.InvoiceId);
                    allPayments.AddRange(payments);
                }

                var totalRevenue = invoices.Sum(i => i.Total);
                
                // Agrupar por tipo de pagamento
                var paymentGroups = allPayments.GroupBy(p => p.PaymentTypeId);
                var paymentMethods = new List<PaymentMethodDto>();
                
                foreach (var group in paymentGroups)
                {
                    var paymentTypeResult = await _paymentTypeService.GetByIdAsync(group.Key);
                    var methodName = paymentTypeResult.Success && paymentTypeResult.Data != null 
                        ? paymentTypeResult.Data.Name 
                        : "Desconhecido";
                    
                    var totalAmount = group.Sum(p => p.Amount);
                    paymentMethods.Add(new PaymentMethodDto
                    {
                        MethodName = methodName,
                        TransactionCount = group.Count(),
                        TotalValue = totalAmount,
                        Percentage = totalRevenue > 0 ? (totalAmount / totalRevenue) * 100 : 0
                    });
                }
                paymentMethods = paymentMethods.OrderByDescending(p => p.TotalValue).Take(5).ToList();

                // Calcular valores para o dashboard
                decimal totalCost = 0;
                foreach (var item in allItems)
                {
                    var productResult = await _productService.GetByIdAsync(item.ProductId);
                    if (productResult.Success && productResult.Data != null)
                    {
                        totalCost += item.Quantity * productResult.Data.CostPrice;
                    }
                }

                var totalProfit = totalRevenue - totalCost;
                var profitMargin = totalRevenue > 0 ? (totalProfit / totalRevenue) * 100 : 0;
                var pendingAmount = invoices.Sum(i => i.Total - i.PaidAmount);

                // Obter valor do estoque
                decimal stockValue = 0;
                var stockResult = await _stockService.GetAllAsync();
                if (stockResult.Success && stockResult.Data != null)
                {
                    foreach (var stock in stockResult.Data)
                    {
                        var productResult = await _productService.GetByIdAsync(stock.ProductId);
                        if (productResult.Success && productResult.Data != null)
                        {
                            stockValue += stock.Quantity * productResult.Data.SalePrice;
                        }
                    }
                }

                // Gerar PDF
                var fileName = $"Dashboard_Executivo_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                // Validar serviço
                if (_pdfGeneratorService == null)
                {
                    ShowMessage("Erro: Serviço de geração de PDF não está disponível.", true);
                    return;
                }

                // Validar parâmetros
                if (companyConfig == null)
                {
                    ShowMessage("Erro: Configuração da empresa não encontrada.", true);
                    return;
                }

                await _pdfGeneratorService.GenerateExecutiveDashboardReportAsync(
                    companyConfig,
                    totalRevenue,
                    totalRevenue,
                    profitMargin,
                    invoices.Count,
                    pendingAmount,
                    stockValue,
                    topProducts ?? new List<TopProductDto>(),
                    paymentMethods ?? new List<PaymentMethodDto>(),
                    StartDate,
                    EndDate,
                    filePath);

                ShowMessage($"Dashboard executivo gerado: {fileName}");
                
                // Abrir o PDF
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar dashboard: {ex.Message}", true);
                Debug.WriteLine($"Erro ao gerar dashboard: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateUserPerformanceAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório de desempenho de usuários...");

                var companyConfig = await GetCompanyConfigAsync();

                // Coletar dados de vendas
                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar dados de vendas.", true);
                    return;
                }

                var invoices = invoicesResult.Data
                    .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed)
                    .ToList();

                // Agrupar por usuário - carregar dados dos usuários
                var userGroups = invoices.GroupBy(i => i.UserId);
                var userPerformance = new List<SalesByUserDto>();

                foreach (var group in userGroups)
                {
                    var user = await GetInvoiceUserAsync(group.Key);
                    userPerformance.Add(new SalesByUserDto
                    {
                        UserName = user?.Username ?? "Desconhecido",
                        InvoiceCount = group.Count(),
                        TotalValue = group.Sum(i => i.Total),
                        PaidValue = group.Sum(i => i.PaidAmount),
                        PendingValue = group.Sum(i => i.Total - i.PaidAmount)
                    });
                }

                userPerformance = userPerformance.OrderByDescending(u => u.TotalValue).ToList();

                // Gerar PDF
                var fileName = $"Desempenho_Usuarios_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                // Validar serviço
                if (_pdfGeneratorService == null)
                {
                    ShowMessage("Erro: Serviço de geração de PDF não está disponível.", true);
                    return;
                }

                // Validar parâmetros
                if (companyConfig == null)
                {
                    ShowMessage("Erro: Configuração da empresa não encontrada.", true);
                    return;
                }

                await _pdfGeneratorService.GenerateUserPerformanceReportAsync(
                    companyConfig,
                    userPerformance ?? new List<SalesByUserDto>(),
                    StartDate,
                    EndDate,
                    filePath);

                ShowMessage($"Relatório de desempenho gerado: {fileName}");
                
                // Abrir o PDF
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório: {ex.Message}", true);
                Debug.WriteLine($"Erro ao gerar relatório: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateComprehensiveReportAsync()
        {
            try
            {
                IsLoading = true;
                ShowMessage("Gerando relatório abrangente... Isso pode levar alguns minutos.");

                var companyConfig = await GetCompanyConfigAsync();

                // Coletar TODOS os dados necessários
                var invoicesResult = await _invoiceService.GetByDateRangeAsync(StartDate, EndDate);
                if (!invoicesResult.Success || invoicesResult.Data == null)
                {
                    ShowMessage("Erro ao buscar dados.", true);
                    return;
                }

                var invoices = invoicesResult.Data.ToList();
                var paidInvoices = invoices.Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed).ToList();

                // Carregar todos os itens das faturas pagas
                var allItems = new List<InvoiceProductDto>();
                var allPayments = new List<PaymentDto>();
                
                foreach (var invoice in paidInvoices)
                {
                    var items = await GetInvoiceItemsAsync(invoice.InvoiceId);
                    allItems.AddRange(items);
                    
                    var payments = await GetInvoicePaymentsAsync(invoice.InvoiceId);
                    allPayments.AddRange(payments);
                }

                // 1. Top Produtos
                var topProducts = allItems
                    .GroupBy(item => item.ProductId)
                    .Select(g => new TopProductDto
                    {
                        ProductName = g.First().ProductName ?? "Produto Desconhecido",
                        QuantitySold = g.Sum(item => item.Quantity),
                        Revenue = g.Sum(item => item.SubTotal)
                    })
                    .OrderByDescending(p => p.Revenue)
                    .Take(10)
                    .ToList();

                // 2. Vendas por Status
                var invoicesByStatus = invoices
                    .GroupBy(i => i.Status)
                    .Select(g => new InvoicesByStatusDto
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count(),
                        TotalValue = g.Sum(i => i.Total)
                    })
                    .ToList();

                // 3. Margem de Lucro - calcular custos
                var profitMargins = new List<ProfitMarginDto>();
                foreach (var invoice in paidInvoices.Take(10))
                {
                    var items = await GetInvoiceItemsAsync(invoice.InvoiceId);
                    decimal invoiceCost = 0;
                    
                    foreach (var item in items)
                    {
                        var productResult = await _productService.GetByIdAsync(item.ProductId);
                        if (productResult.Success && productResult.Data != null)
                        {
                            invoiceCost += item.Quantity * productResult.Data.CostPrice;
                        }
                    }

                    var invoiceGrossProfit = invoice.Total - invoiceCost;
                    profitMargins.Add(new ProfitMarginDto
                    {
                        InvoiceNumber = invoice.InvoiceNumber,
                        InvoiceDate = invoice.Date,
                        TotalRevenue = invoice.Total,
                        TotalCost = invoiceCost,
                        GrossProfit = invoiceGrossProfit,
                        MarginPercentage = invoice.Total > 0 ? (invoiceGrossProfit / invoice.Total) * 100 : 0
                    });
                }
                profitMargins = profitMargins.OrderByDescending(p => p.GrossProfit).ToList();

                // 4. Formas de Pagamento
                var totalRevenue = paidInvoices.Sum(i => i.Total);
                
                // Agrupar por tipo de pagamento
                var paymentGroupsComp = allPayments.GroupBy(p => p.PaymentTypeId);
                var paymentMethods = new List<PaymentMethodDto>();
                
                foreach (var group in paymentGroupsComp)
                {
                    var paymentTypeResult = await _paymentTypeService.GetByIdAsync(group.Key);
                    var methodName = paymentTypeResult.Success && paymentTypeResult.Data != null 
                        ? paymentTypeResult.Data.Name 
                        : "Desconhecido";
                    
                    var totalAmount = group.Sum(p => p.Amount);
                    paymentMethods.Add(new PaymentMethodDto
                    {
                        MethodName = methodName,
                        TransactionCount = group.Count(),
                        TotalValue = totalAmount,
                        Percentage = totalRevenue > 0 ? (totalAmount / totalRevenue) * 100 : 0
                    });
                }
                paymentMethods = paymentMethods.OrderByDescending(p => p.TotalValue).ToList();

                // 5. Contas a Receber
                var accountsReceivable = new List<AccountsReceivableDto>();
                foreach (var invoice in invoices.Where(i => i.Total > i.PaidAmount))
                {
                    var customer = await GetInvoiceCustomerAsync(invoice.PersonId);
                    accountsReceivable.Add(new AccountsReceivableDto
                    {
                        InvoiceNumber = invoice.InvoiceNumber,
                        CustomerName = customer?.Name ?? "N/A",
                        DueDate = invoice.DueDate ?? DateTime.Now.AddDays(30),
                        TotalValue = invoice.Total,
                        PaidValue = invoice.PaidAmount,
                        PendingValue = invoice.Total - invoice.PaidAmount,
                        DaysOverdue = invoice.DueDate.HasValue && invoice.DueDate < DateTime.Today 
                            ? (DateTime.Today - invoice.DueDate.Value).Days : 0,
                        Status = invoice.Status.ToString()
                    });
                }
                accountsReceivable = accountsReceivable.OrderByDescending(a => a.DaysOverdue).ToList();

                // 6. Desempenho de Usuários
                var userGroups = paidInvoices.GroupBy(i => i.UserId);
                var userPerformance = new List<SalesByUserDto>();
                
                foreach (var group in userGroups)
                {
                    var user = await GetInvoiceUserAsync(group.Key);
                    userPerformance.Add(new SalesByUserDto
                    {
                        UserName = user?.Username ?? "Desconhecido",
                        InvoiceCount = group.Count(),
                        TotalValue = group.Sum(i => i.Total),
                        PaidValue = group.Sum(i => i.PaidAmount),
                        PendingValue = group.Sum(i => i.Total - i.PaidAmount)
                    });
                }
                userPerformance = userPerformance.OrderByDescending(u => u.TotalValue).ToList();

                // Calcular totais
                decimal totalCostAll = profitMargins.Sum(p => p.TotalCost);
                decimal grossProfit = totalRevenue - totalCostAll;

                // Criar o DTO abrangente
                var comprehensiveData = new ComprehensiveReportDto
                {
                    StartDate = StartDate,
                    EndDate = EndDate,
                    TotalRevenue = totalRevenue,
                    TotalSales = totalRevenue,
                    GrossProfit = grossProfit,
                    ProfitMargin = totalRevenue > 0 ? (grossProfit / totalRevenue) * 100 : 0,
                    TotalInvoices = invoices.Count,
                    TopProducts = topProducts,
                    InvoicesByStatus = invoicesByStatus,
                    PaymentMethods = paymentMethods,
                    AccountsReceivable = accountsReceivable,
                    UserPerformance = userPerformance
                };

                // Gerar PDF
                var fileName = $"Relatorio_Abrangente_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                // Validar serviço
                if (_pdfGeneratorService == null)
                {
                    ShowMessage("Erro: Serviço de geração de PDF não está disponível.", true);
                    return;
                }

                // Validar parâmetros
                if (companyConfig == null || comprehensiveData == null)
                {
                    ShowMessage("Erro: Dados do relatório incompletos.", true);
                    return;
                }

                await _pdfGeneratorService.GenerateComprehensiveReportAsync(
                    companyConfig,
                    comprehensiveData,
                    filePath);

                ShowMessage($"Relatório abrangente gerado: {fileName}");
                
                // Abrir o PDF
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar relatório abrangente: {ex.Message}", true);
                Debug.WriteLine($"Erro ao gerar relatório abrangente: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        private async Task<CompanyConfigDto> GetCompanyConfigAsync()
        {
            try
            {

                var companyResult = await _companyConfigService.GetAsync();
                if ( companyResult.Success && companyResult.Data != null)
                {
                    return companyResult.Data;
                }

                return CreateDefaultCompanyConfig();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao buscar configuração da empresa: {ex.Message}");
                return CreateDefaultCompanyConfig();
            }
        }

        private CompanyConfigDto CreateDefaultCompanyConfig()
        {
            return new CompanyConfigDto 
            { 
                CompanyName = "VendaFlex",
                Email = "contato@vendaflex.com",
                PhoneNumber = "",
                Address = "",
                City = "",
                Country = "Angola",
                Currency = "AOA",
                CurrencySymbol = "Kz"
            };
        }

        /// <summary>
        /// Carrega os itens de uma fatura
        /// </summary>
        private async Task<List<InvoiceProductDto>> GetInvoiceItemsAsync(int invoiceId)
        {
            try
            {
                var result = await _invoiceProductService.GetByInvoiceIdAsync(invoiceId);
                if (result.Success && result.Data != null)
                {
                    var items = result.Data.ToList();
                    // Carregar nomes dos produtos
                    foreach (var item in items)
                    {
                        var productResult = await _productService.GetByIdAsync(item.ProductId);
                        if (productResult.Success && productResult.Data != null)
                        {
                            item.ProductName = productResult.Data.Name;
                        }
                    }
                    return items;
                }
            }
            catch
            {
                // Ignora erros e retorna lista vazia
            }
            return new List<InvoiceProductDto>();
        }

        /// <summary>
        /// Carrega os pagamentos de uma fatura
        /// </summary>
        private async Task<List<PaymentDto>> GetInvoicePaymentsAsync(int invoiceId)
        {
            try
            {
                var result = await _paymentService.GetByInvoiceIdAsync(invoiceId);
                if (result.Success && result.Data != null)
                {
                    return result.Data.ToList();
                }
            }
            catch
            {
                // Ignora erros e retorna lista vazia
            }
            return new List<PaymentDto>();
        }

        /// <summary>
        /// Carrega o cliente de uma fatura
        /// </summary>
        private async Task<PersonDto?> GetInvoiceCustomerAsync(int personId)
        {
            try
            {
                var result = await _personService.GetByIdAsync(personId);
                if (result.Success && result.Data != null)
                {
                    return result.Data;
                }
            }
            catch
            {
                // Ignora erros e retorna null
            }
            return null;
        }

        /// <summary>
        /// Carrega o usuário de uma fatura
        /// </summary>
        private async Task<UserDto?> GetInvoiceUserAsync(int userId)
        {
            try
            {
                var result = await _userService.GetByIdAsync(userId);
                if (result.Success && result.Data != null)
                {
                    return result.Data;
                }
            }
            catch
            {
                // Ignora erros e retorna null
            }
            return null;
        }

        /// <summary>
        /// Carrega dados completos de uma fatura incluindo itens, pagamentos, cliente e usuário
        /// </summary>
        private async Task<(InvoiceDto invoice, List<InvoiceProductDto> items, List<PaymentDto> payments, PersonDto? customer, UserDto? user)> 
            LoadCompleteInvoiceDataAsync(InvoiceDto invoice)
        {
            var itemsTask = GetInvoiceItemsAsync(invoice.InvoiceId);
            var paymentsTask = GetInvoicePaymentsAsync(invoice.InvoiceId);
            var customerTask = GetInvoiceCustomerAsync(invoice.PersonId);
            var userTask = GetInvoiceUserAsync(invoice.UserId);

            await Task.WhenAll(itemsTask, paymentsTask, customerTask, userTask);

            return (invoice, await itemsTask, await paymentsTask, await customerTask, await userTask);
        }

        #endregion

        #region Export

        private async Task ExportDataAsync<T>(ObservableCollection<T> data, string reportName)
        {
            try
            {
                if (data == null || !data.Any())
                {
                    ShowMessage("Não há dados para exportar!", true);
                    return;
                }

                var dialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"{reportName}_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    ShowMessage("Exportando dados...");

                    var extension = Path.GetExtension(dialog.FileName).ToLowerInvariant();

                    if (extension == ".csv")
                        await ExportToCsvAsync(data, dialog.FileName);
                    else if (extension == ".xlsx")
                        await ExportToExcelAsync(data, dialog.FileName);

                    ShowMessage("Exportação concluída com sucesso!");

                    // Open file
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = dialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao exportar: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportToCsvAsync<T>(ObservableCollection<T> data, string filePath)
        {
            await Task.Run(() =>
            {
                var csv = new StringBuilder();
                var properties = typeof(T).GetProperties();

                // Header
                csv.AppendLine(string.Join(";", properties.Select(p => p.Name)));

                // Data
                foreach (var item in data)
                {
                    var values = properties.Select(p =>
                    {
                        var value = p.GetValue(item);
                        return value?.ToString() ?? string.Empty;
                    });
                    csv.AppendLine(string.Join(";", values));
                }

                File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
            });
        }

        private async Task ExportToExcelAsync<T>(ObservableCollection<T> data, string filePath)
        {
            // Placeholder - implement with EPPlus or ClosedXML
            await ExportToCsvAsync(data, filePath.Replace(".xlsx", ".csv"));
        }

        #endregion

        #region Quick Period Actions

        private enum PeriodType
        {
            Today,
            Week,
            Month,
            Year
        }

        private void SetQuickPeriod(PeriodType period)
        {
            var today = DateTime.Today;

            switch (period)
            {
                case PeriodType.Today:
                    StartDate = today;
                    EndDate = today;
                    break;

                case PeriodType.Week:
                    var dayOfWeek = (int)today.DayOfWeek;
                    var startOfWeek = today.AddDays(-dayOfWeek);
                    StartDate = startOfWeek;
                    EndDate = today;
                    break;

                case PeriodType.Month:
                    StartDate = new DateTime(today.Year, today.Month, 1);
                    EndDate = today;
                    break;

                case PeriodType.Year:
                    StartDate = new DateTime(today.Year, 1, 1);
                    EndDate = today;
                    break;
            }

            _ = GenerateDefaultReportsAsync();
        }

        #endregion

        #region Helper Methods

        private DateTime GetWeekStart(DateTime date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            return date.AddDays(-dayOfWeek).Date;
        }

        private DateTime GetQuarterStart(DateTime date)
        {
            var quarter = (date.Month - 1) / 3;
            return new DateTime(date.Year, quarter * 3 + 1, 1);
        }

        private void ShowMessage(string message, bool isError = false)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                MessageQueue?.Enqueue(message);
            });
        }

        #endregion
    }

    #region DTOs

    public class SalesByPeriodDto
    {
        public DateTime Date { get; set; }
        public int InvoiceCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal PaidValue { get; set; }
        public decimal PendingValue { get; set; }
        public string DateFormatted => Date.ToString("dd/MM/yyyy");
    }

    public class SalesByUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public int InvoiceCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal PaidValue { get; set; }
        public decimal PendingValue { get; set; }
    }

    public class SalesByCustomerDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public int InvoiceCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal PaidValue { get; set; }
        public decimal PendingValue { get; set; }
        public DateTime LastPurchaseDate { get; set; }
    }

    public class CashFlowDto
    {
        public DateTime Date { get; set; }
        public decimal Inflow { get; set; }
        public decimal Outflow { get; set; }
        public decimal Balance { get; set; }
        public string DateFormatted => Date.ToString("dd/MM/yyyy");
    }

    public class PaymentMethodDto
    {
        public string MethodName { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class AccountsReceivableDto
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal TotalValue { get; set; }
        public decimal PaidValue { get; set; }
        public decimal PendingValue { get; set; }
        public int DaysOverdue { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class StockMovementReportDto
    {
        public DateTime Date { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string MovementType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }

    public class LowStockDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public int MinimumQuantity { get; set; }
        public int ReorderPoint { get; set; }
        public int Difference { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ExpirationReportDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string Batch { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public int DaysToExpire { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ExpirationDateFormatted => ExpirationDate.ToString("dd/MM/yyyy");
    }

    public class ProfitMarginDto
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal MarginPercentage { get; set; }
        public string InvoiceDateFormatted => InvoiceDate.ToString("dd/MM/yyyy");
    }

    public class InvoicesByStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ComprehensiveReportDto
    {
        // Período do relatório
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportType { get; set; } = string.Empty; // "Mensal" ou "Anual"
        
        // Resumo Executivo
        public int TotalInvoices { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal GrowthPercentage { get; set; }
        
        // Análise de Vendas
        public IEnumerable<SalesByPeriodDto> SalesTrend { get; set; } = new List<SalesByPeriodDto>();
        public IEnumerable<TopProductDto> TopProducts { get; set; } = new List<TopProductDto>();
        public decimal AverageTicket { get; set; }
        
        // Análise Financeira
        public IEnumerable<PaymentMethodDto> PaymentMethods { get; set; } = new List<PaymentMethodDto>();
        public IEnumerable<AccountsReceivableDto> AccountsReceivable { get; set; } = new List<AccountsReceivableDto>();
        public decimal TotalPendingAmount { get; set; }
        public decimal DefaultRate { get; set; }
        public IEnumerable<CashFlowDto> CashFlow { get; set; } = new List<CashFlowDto>();
        
        // Análise de Clientes
        public IEnumerable<SalesByCustomerDto> TopCustomers { get; set; } = new List<SalesByCustomerDto>();
        public int NewCustomers { get; set; }
        public decimal RetentionRate { get; set; }
        public decimal AverageTicketPerCustomer { get; set; }
        
        // Análise de Estoque
        public decimal TotalStockValue { get; set; }
        public IEnumerable<TopProductDto> TopSellingProducts { get; set; } = new List<TopProductDto>();
        public IEnumerable<LowStockDto> LowStockProducts { get; set; } = new List<LowStockDto>();
        public IEnumerable<ExpirationReportDto> ExpiringProducts { get; set; } = new List<ExpirationReportDto>();
        
        // Análise Operacional
        public IEnumerable<InvoicesByStatusDto> InvoicesByStatus { get; set; } = new List<InvoicesByStatusDto>();
        public decimal CancellationRate { get; set; }
        public IEnumerable<SalesByUserDto> UserPerformance { get; set; } = new List<SalesByUserDto>();
        
        // Formatação
        public string StartDateFormatted => StartDate.ToString("dd/MM/yyyy");
        public string EndDateFormatted => EndDate.ToString("dd/MM/yyyy");
    }

    #endregion
}
