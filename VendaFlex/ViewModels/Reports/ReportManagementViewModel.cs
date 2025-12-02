using System;
using System.Collections.ObjectModel;
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

namespace VendaFlex.ViewModels.Reports
{
    /// <summary>
    /// ViewModel principal para o sistema de relatórios profissional
    /// </summary>
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
                }
            }
        }

        public bool IsSalesReportsVisible => SelectedCategoryIndex == 0;
        public bool IsFinancialReportsVisible => SelectedCategoryIndex == 1;
        public bool IsStockReportsVisible => SelectedCategoryIndex == 2;
        public bool IsManagementReportsVisible => SelectedCategoryIndex == 3;

        #endregion

        #region Properties - Date Filters

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

        // Report Generation Commands
        public ICommand GenerateSalesByPeriodCommand { get; private set; }
        public ICommand GenerateTopProductsCommand { get; private set; }
        public ICommand GenerateSalesByCustomerCommand { get; private set; }
        public ICommand GenerateCashFlowCommand { get; private set; }
        public ICommand GeneratePaymentMethodsCommand { get; private set; }
        public ICommand GenerateAccountsReceivableCommand { get; private set; }
        public ICommand GenerateStockMovementsCommand { get; private set; }
        public ICommand GenerateLowStockCommand { get; private set; }
        public ICommand GenerateExpiringProductsCommand { get; private set; }

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
            IInvoiceService invoiceService,
            IProductService productService,
            IStockService stockService,
            IPaymentService paymentService,
            IPersonService personService,
            IExpirationService expirationService,
            IInvoiceProductService invoiceProductService)
        {
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _personService = personService ?? throw new ArgumentNullException(nameof(personService));
            _expirationService = expirationService ?? throw new ArgumentNullException(nameof(expirationService));
            _invoiceProductService = invoiceProductService ?? throw new ArgumentNullException(nameof(invoiceProductService));

            InitializeCommands();
            _ = LoadInitialDataAsync();
        }

        #endregion

        #region Initialization

        private void InitializeCommands()
        {
            // Sales Reports
            GenerateSalesByPeriodCommand = new RelayCommand(async _ => await GenerateSalesByPeriodAsync());
            GenerateTopProductsCommand = new RelayCommand(async _ => await GenerateTopProductsAsync());
            GenerateSalesByCustomerCommand = new RelayCommand(async _ => await GenerateSalesByCustomerAsync());

            // Financial Reports
            GenerateCashFlowCommand = new RelayCommand(async _ => await GenerateCashFlowAsync());
            GeneratePaymentMethodsCommand = new RelayCommand(async _ => await GeneratePaymentMethodsAsync());
            GenerateAccountsReceivableCommand = new RelayCommand(async _ => await GenerateAccountsReceivableAsync());

            // Stock Reports
            GenerateStockMovementsCommand = new RelayCommand(async _ => await GenerateStockMovementsAsync());
            GenerateLowStockCommand = new RelayCommand(async _ => await GenerateLowStockAsync());
            GenerateExpiringProductsCommand = new RelayCommand(async _ => await GenerateExpiringProductsAsync());

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

                // Load overview statistics
                await LoadOverviewStatisticsAsync();

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
                    TotalInvoices = invoices.Count;
                    TotalSalesValue = invoices.Sum(i => i.Total);
                    TotalRevenue = invoices.Sum(i => i.PaidAmount);
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
                    break;
                case 2: // Stock
                    await GenerateLowStockAsync();
                    break;
                case 3: // Management
                    await GenerateSalesByCustomerAsync();
                    break;
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
                        .Where(i => i.Status == InvoiceStatus.Paid || i.Status == InvoiceStatus.Confirmed)
                        .GroupBy(i => i.Date.Date)
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

                    SalesByPeriod = new ObservableCollection<SalesByPeriodDto>(invoices);
                    ShowMessage($"Relatório gerado: {invoices.Count} registros encontrados.");
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

                // Calculate ProgressPercentage
                var maxRevenue = topProducts.Count > 0 ? topProducts.Max(p => p.Revenue) : 0;
                if (maxRevenue > 0)
                {
                    foreach (var product in topProducts)
                    {
                        product.ProgressPercentage = Math.Round((double)(product.Revenue / maxRevenue) * 100.0, 2);
                    }
                }

                TopProducts = new ObservableCollection<TopProductDto>(topProducts);
                ShowMessage($"Top {topProducts.Count} produtos carregados.");
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
                        foreach (var payment in paymentsResult.Data.Where(p => p.IsConfirmed))
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
                }

                var methodsList = paymentMethods
                    .Select(kvp => new PaymentMethodDto
                    {
                        MethodName = kvp.Key,
                        TransactionCount = kvp.Value.Count,
                        TotalValue = kvp.Value.Total,
                        Percentage = 0 // Will be calculated after
                    })
                    .OrderByDescending(p => p.TotalValue)
                    .ToList();

                var totalValue = methodsList.Sum(p => p.TotalValue);
                foreach (var method in methodsList)
                {
                    method.Percentage = totalValue > 0 ? (method.TotalValue / totalValue) * 100 : 0;
                }

                PaymentMethods = new ObservableCollection<PaymentMethodDto>(methodsList);
                ShowMessage($"{methodsList.Count} formas de pagamento encontradas.");
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
            }
            finally
            {
                IsLoading = false;
            }
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

    #endregion
}
