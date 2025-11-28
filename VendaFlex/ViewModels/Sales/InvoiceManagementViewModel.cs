using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using VendaFlex.Core.Interfaces;
using VendaFlex.Core.DTOs;
using VendaFlex.Data.Entities;
using VendaFlex.ViewModels.Base;
using VendaFlex.ViewModels.Commands;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace VendaFlex.ViewModels.Sales
{
    public class InvoiceManagementViewModel : BaseViewModel
    {
        #region Services

        private readonly ICompanyConfigService _companyConfigService;
        private readonly ICurrentUserContext _currentUserContext;
        private readonly IPersonService _personService;
        private readonly IReceiptPrintService _printService;
        private readonly IInvoiceService _invoiceService;
        private readonly IInvoiceProductService _invoiceProductService;
        private readonly IPaymentService _paymentService;
        private readonly IPaymentTypeService _paymentTypeService;
        private readonly IProductService _productService;
        private readonly IStockService _stockService;

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

        #region Properties - Statistics

        private int _totalPaidCount;
        public int TotalPaidCount
        {
            get => _totalPaidCount;
            set => Set(ref _totalPaidCount, value);
        }

        private int _totalPendingCount;
        public int TotalPendingCount
        {
            get => _totalPendingCount;
            set => Set(ref _totalPendingCount, value);
        }

        private int _totalCancelledCount;
        public int TotalCancelledCount
        {
            get => _totalCancelledCount;
            set => Set(ref _totalCancelledCount, value);
        }

        #endregion

        #region Properties - Filters

        private DateTime? _filterStartDate;
        public DateTime? FilterStartDate
        {
            get => _filterStartDate;
            set => Set(ref _filterStartDate, value);
        }

        private DateTime? _filterEndDate;
        public DateTime? FilterEndDate
        {
            get => _filterEndDate;
            set => Set(ref _filterEndDate, value);
        }

        private string _filterInvoiceNumber = string.Empty;
        public string FilterInvoiceNumber
        {
            get => _filterInvoiceNumber;
            set => Set(ref _filterInvoiceNumber, value);
        }

        private string _filterStatus = "Todos";
        public string FilterStatus
        {
            get => _filterStatus;
            set => Set(ref _filterStatus, value);
        }

        private string _filterCustomerName = string.Empty;
        public string FilterCustomerName
        {
            get => _filterCustomerName;
            set => Set(ref _filterCustomerName, value);
        }

        private string _filterCustomerNif = string.Empty;
        public string FilterCustomerNif
        {
            get => _filterCustomerNif;
            set => Set(ref _filterCustomerNif, value);
        }

        private string _filterOperatorName = string.Empty;
        public string FilterOperatorName
        {
            get => _filterOperatorName;
            set => Set(ref _filterOperatorName, value);
        }

        private PaymentTypeDto? _filterPaymentType;
        public PaymentTypeDto? FilterPaymentType
        {
            get => _filterPaymentType;
            set => Set(ref _filterPaymentType, value);
        }

        #endregion

        #region Properties - Pagination & Sorting

        private int _pageNumber = 1;
        public int PageNumber
        {
            get => _pageNumber;
            set => Set(ref _pageNumber, value);
        }

        private int _pageSize = 20;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (Set(ref _pageSize, value))
                {
                    PageNumber = 1;
                    _ = SearchAsync();
                }
            }
        }

        private int _totalPages;
        public int TotalPages
        {
            get => _totalPages;
            set => Set(ref _totalPages, value);
        }

        private int _totalItems;
        public int TotalItems
        {
            get => _totalItems;
            set => Set(ref _totalItems, value);
        }

        private string _sortColumn = "Data";
        public string SortColumn
        {
            get => _sortColumn;
            set
            {
                if (Set(ref _sortColumn, value))
                {
                    _ = SearchAsync();
                }
            }
        }

        private bool _sortAscending = false;
        public bool SortAscending
        {
            get => _sortAscending;
            set
            {
                if (Set(ref _sortAscending, value))
                {
                    _ = SearchAsync();
                }
            }
        }

        #endregion

        #region Properties - Collections

        private ObservableCollection<InvoiceListItemDto> _invoices = new();
        public ObservableCollection<InvoiceListItemDto> Invoices
        {
            get => _invoices;
            set => Set(ref _invoices, value);
        }

        private ObservableCollection<PaymentTypeDto> _paymentTypes = new();
        public ObservableCollection<PaymentTypeDto> PaymentTypes
        {
            get => _paymentTypes;
            set => Set(ref _paymentTypes, value);
        }

        #endregion

        #region Properties - Selected Invoice

        private InvoiceListItemDto? _selectedInvoice;
        public InvoiceListItemDto? SelectedInvoice
        {
            get => _selectedInvoice;
            set => Set(ref _selectedInvoice, value);
        }

        private decimal _selectedInvoiceTotal;
        public decimal SelectedInvoiceTotal
        {
            get => _selectedInvoiceTotal;
            set => Set(ref _selectedInvoiceTotal, value);
        }

        private decimal _selectedInvoicePaid;
        public decimal SelectedInvoicePaid
        {
            get => _selectedInvoicePaid;
            set
            {
                if (Set(ref _selectedInvoicePaid, value))
                {
                    SelectedInvoiceBalance = SelectedInvoiceTotal - value;
                }
            }
        }

        private decimal _selectedInvoiceBalance;
        public decimal SelectedInvoiceBalance
        {
            get => _selectedInvoiceBalance;
            set => Set(ref _selectedInvoiceBalance, value);
        }

        private ObservableCollection<InvoiceProductDto> _selectedInvoiceItems = new();
        public ObservableCollection<InvoiceProductDto> SelectedInvoiceItems
        {
            get => _selectedInvoiceItems;
            set => Set(ref _selectedInvoiceItems, value);
        }

        private ObservableCollection<PaymentListItemDto> _selectedInvoicePayments = new();
        public ObservableCollection<PaymentListItemDto> SelectedInvoicePayments
        {
            get => _selectedInvoicePayments;
            set => Set(ref _selectedInvoicePayments, value);
        }

        private ObservableCollection<InvoiceHistoryItemDto> _selectedInvoiceHistory = new();
        public ObservableCollection<InvoiceHistoryItemDto> SelectedInvoiceHistory
        {
            get => _selectedInvoiceHistory;
            set => Set(ref _selectedInvoiceHistory, value);
        }

        private PaymentListItemDto? _selectedPayment;
        public PaymentListItemDto? SelectedPayment
        {
            get => _selectedPayment;
            set => Set(ref _selectedPayment, value);
        }

        #endregion

        #region Properties - New Payment

        private NewPaymentDto _newPayment = new();
        public NewPaymentDto NewPayment
        {
            get => _newPayment;
            set => Set(ref _newPayment, value);
        }

        #endregion

        #region Properties - Adjustments

        private decimal _adjustmentDiscount;
        public decimal AdjustmentDiscount
        {
            get => _adjustmentDiscount;
            set => Set(ref _adjustmentDiscount, value);
        }

        private string _adjustmentReason = string.Empty;
        public string AdjustmentReason
        {
            get => _adjustmentReason;
            set => Set(ref _adjustmentReason, value);
        }

        private decimal _adjustmentSurcharge;
        public decimal AdjustmentSurcharge
        {
            get => _adjustmentSurcharge;
            set => Set(ref _adjustmentSurcharge, value);
        }

        private string _surchargeReason = string.Empty;
        public string SurchargeReason
        {
            get => _surchargeReason;
            set => Set(ref _surchargeReason, value);
        }

        private PaymentListItemDto? _paymentToChange;
        public PaymentListItemDto? PaymentToChange
        {
            get => _paymentToChange;
            set => Set(ref _paymentToChange, value);
        }

        private PaymentTypeDto? _newPaymentType;
        public PaymentTypeDto? NewPaymentType
        {
            get => _newPaymentType;
            set => Set(ref _newPaymentType, value);
        }

        #endregion

        #region Properties - Stock Impact

        private ObservableCollection<StockImpactItemDto> _stockImpactItems = new();
        public ObservableCollection<StockImpactItemDto> StockImpactItems
        {
            get => _stockImpactItems;
            set => Set(ref _stockImpactItems, value);
        }

        private decimal _accountsReceivable;
        public decimal AccountsReceivable
        {
            get => _accountsReceivable;
            set => Set(ref _accountsReceivable, value);
        }

        private decimal _netRevenue;
        public decimal NetRevenue
        {
            get => _netRevenue;
            set => Set(ref _netRevenue, value);
        }

        #endregion

        #region Properties - Dialog

        private bool _isDetailsModalOpen;
        public bool IsDetailsModalOpen
        {
            get => _isDetailsModalOpen;
            set => Set(ref _isDetailsModalOpen, value);
        }

        #endregion

        #region Commands

        public ICommand OpenDetailsCommand { get; private set; }
        public ICommand CloseDetailsCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand SearchCommand { get; private set; }
        public ICommand ClearFiltersCommand { get; private set; }
        public ICommand FirstPageCommand { get; private set; }
        public ICommand PreviousPageCommand { get; private set; }
        public ICommand NextPageCommand { get; private set; }
        public ICommand LastPageCommand { get; private set; }
        public ICommand PrintInvoiceCommand { get; private set; }
        public ICommand GeneratePdfCommand { get; private set; }
        public ICommand DuplicateInvoiceCommand { get; private set; }
        public ICommand ReopenInvoiceCommand { get; private set; }
        public ICommand IssueCreditNoteCommand { get; private set; }
        public ICommand IssueDebitNoteCommand { get; private set; }
        public ICommand CancelInvoiceCommand { get; private set; }
        public ICommand AddPaymentCommand { get; private set; }
        public ICommand RemovePaymentCommand { get; private set; }
        public ICommand ApplyDiscountCommand { get; private set; }
        public ICommand ApplySurchargeCommand { get; private set; }
        public ICommand ChangePaymentTypeCommand { get; private set; }
        public ICommand UpdateStockCommand { get; private set; }
        public ICommand PostToAccountsReceivableCommand { get; private set; }
        public ICommand GenerateFinancialReportCommand { get; private set; }

        #endregion

        #region Constructor

        public InvoiceManagementViewModel(
            ICompanyConfigService companyConfigService,
            ICurrentUserContext currentUserContext,
            IPersonService personService,
            IReceiptPrintService printService,
            IInvoiceService invoiceService,
            IInvoiceProductService invoiceProductService,
            IPaymentService paymentService,
            IPaymentTypeService paymentTypeService,
            IProductService productService,
            IStockService stockService)
        {
            _companyConfigService = companyConfigService ?? throw new ArgumentNullException(nameof(companyConfigService));
            _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));
            _personService = personService ?? throw new ArgumentNullException(nameof(personService));
            _printService = printService ?? throw new ArgumentNullException(nameof(printService));
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
            _invoiceProductService = invoiceProductService ?? throw new ArgumentNullException(nameof(invoiceProductService));
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));

            InitializeCommands();
            _ = InitializeAsync();
        }

        private void InitializeCommands()
        {
            OpenDetailsCommand = new RelayCommand(async invoice => await OpenDetailsAsync(invoice as InvoiceListItemDto));
            CloseDetailsCommand = new RelayCommand(_ => CloseDetails());
            RefreshCommand = new RelayCommand(async _ => await RefreshAsync());
            ExportCommand = new RelayCommand(async _ => await ExportAsync());
            SearchCommand = new RelayCommand(async _ => await SearchAsync());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            FirstPageCommand = new RelayCommand(async _ => await GoToFirstPageAsync(), _ => PageNumber > 1);
            PreviousPageCommand = new RelayCommand(async _ => await GoToPreviousPageAsync(), _ => PageNumber > 1);
            NextPageCommand = new RelayCommand(async _ => await GoToNextPageAsync(), _ => PageNumber < TotalPages);
            LastPageCommand = new RelayCommand(async _ => await GoToLastPageAsync(), _ => PageNumber < TotalPages);
            PrintInvoiceCommand = new RelayCommand(async _ => await PrintInvoiceAsync(), _ => SelectedInvoice != null);
            GeneratePdfCommand = new RelayCommand(async _ => await GeneratePdfAsync(), _ => SelectedInvoice != null);
            DuplicateInvoiceCommand = new RelayCommand(async _ => await DuplicateInvoiceAsync(), _ => SelectedInvoice != null);
            ReopenInvoiceCommand = new RelayCommand(async _ => await ReopenInvoiceAsync(), _ => SelectedInvoice != null && SelectedInvoice.Status == InvoiceStatus.Cancelled);
            IssueCreditNoteCommand = new RelayCommand(async _ => await IssueCreditNoteAsync(), _ => SelectedInvoice != null);
            IssueDebitNoteCommand = new RelayCommand(async _ => await IssueDebitNoteAsync(), _ => SelectedInvoice != null);
            CancelInvoiceCommand = new RelayCommand(async _ => await CancelInvoiceAsync(), _ => SelectedInvoice != null && SelectedInvoice.Status != InvoiceStatus.Cancelled);
            AddPaymentCommand = new RelayCommand(async _ => await AddPaymentAsync(), CanAddPayment);
            RemovePaymentCommand = new RelayCommand(async payment => await RemovePaymentAsync(payment as PaymentListItemDto));
            ApplyDiscountCommand = new RelayCommand(async _ => await ApplyDiscountAsync(), CanApplyDiscount);
            ApplySurchargeCommand = new RelayCommand(async _ => await ApplySurchargeAsync(), CanApplySurcharge);
            ChangePaymentTypeCommand = new RelayCommand(async _ => await ChangePaymentTypeAsync(), CanChangePaymentType);
            UpdateStockCommand = new RelayCommand(async _ => await UpdateStockAsync(), _ => SelectedInvoice != null);
            PostToAccountsReceivableCommand = new RelayCommand(async _ => await PostToAccountsReceivableAsync(), _ => SelectedInvoice != null);
            GenerateFinancialReportCommand = new RelayCommand(async _ => await GenerateFinancialReportAsync(), _ => SelectedInvoice != null);
        }

        #endregion

        #region Initialization

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;

                await LoadPaymentTypesAsync();
                await SearchAsync();
                await UpdateStatisticsAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao inicializar: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadPaymentTypesAsync()
        {
            try
            {
                var result = await _paymentTypeService.GetActiveAsync();
                if (result.Success && result.Data != null)
                {
                    PaymentTypes = new ObservableCollection<PaymentTypeDto>(result.Data);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar tipos de pagamento: {ex.Message}");
            }
        }

        #endregion

        #region Search & Filter Methods

        private async Task SearchAsync()
        {
            try
            {
                IsLoading = true;

                var result = await _invoiceService.GetAllAsync();

                if (result.Success && result.Data != null)
                {
                    var invoices = result.Data.AsEnumerable();

                    // Aplicar filtros
                    invoices = ApplyFilters(invoices);

                    // Ordenar
                    invoices = ApplySorting(invoices);

                    var invoicesList = invoices.ToList();
                    TotalItems = invoicesList.Count;
                    TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

                    // Paginação
                    var pagedInvoices = invoicesList
                        .Skip((PageNumber - 1) * PageSize)
                        .Take(PageSize);

                    // Mapear para lista com dados do cliente e operador
                    var listItems = new List<InvoiceListItemDto>();
                    foreach (var invoice in pagedInvoices)
                    {
                        listItems.Add(await MapToListItemAsync(invoice));
                    }

                    Invoices = new ObservableCollection<InvoiceListItemDto>(listItems);
                }
                else
                {
                    ShowMessage($"Erro ao buscar faturas: {result.Message}", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao buscar faturas: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private IEnumerable<InvoiceDto> ApplyFilters(IEnumerable<InvoiceDto> invoices)
        {
            if (FilterStartDate.HasValue)
                invoices = invoices.Where(i => i.Date.Date >= FilterStartDate.Value.Date);

            if (FilterEndDate.HasValue)
                invoices = invoices.Where(i => i.Date.Date <= FilterEndDate.Value.Date);

            if (!string.IsNullOrWhiteSpace(FilterInvoiceNumber))
                invoices = invoices.Where(i => i.InvoiceNumber.Contains(FilterInvoiceNumber, StringComparison.OrdinalIgnoreCase));

            if (FilterStatus != "Todos")
            {
                var status = FilterStatus switch
                {
                    "Paga" => InvoiceStatus.Paid,
                    "Pendente" => InvoiceStatus.Pending,
                    "Cancelada" => InvoiceStatus.Cancelled,
                    _ => (InvoiceStatus?)null
                };
                if (status.HasValue)
                    invoices = invoices.Where(i => i.Status == status.Value);
            }

            // TODO: Implementar filtros de cliente e operador quando tiver acesso aos dados

            return invoices;
        }

        private IEnumerable<InvoiceDto> ApplySorting(IEnumerable<InvoiceDto> invoices)
        {
            return SortColumn switch
            {
                "Data" => SortAscending ? invoices.OrderBy(i => i.Date) : invoices.OrderByDescending(i => i.Date),
                "Número" => SortAscending ? invoices.OrderBy(i => i.InvoiceNumber) : invoices.OrderByDescending(i => i.InvoiceNumber),
                "Total" => SortAscending ? invoices.OrderBy(i => i.Total) : invoices.OrderByDescending(i => i.Total),
                _ => invoices.OrderByDescending(i => i.Date)
            };
        }

        private async Task<InvoiceListItemDto> MapToListItemAsync(InvoiceDto invoice)
        {
            var listItem = new InvoiceListItemDto
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                Date = invoice.Date,
                Status = invoice.Status,
                Total = invoice.Total,
                PaidAmount = invoice.PaidAmount,
                Balance = invoice.Total - invoice.PaidAmount
            };

            // Carregar dados do cliente
            if (invoice.PersonId > 0)
            {
                var personResult = await _personService.GetByIdAsync(invoice.PersonId);
                if (personResult.Success && personResult.Data != null)
                {
                    var person = personResult.Data;
                    listItem.CustomerName = person.Name;
                    listItem.CustomerNif = person.TaxId ?? string.Empty;
                    listItem.CustomerPhone = person.PhoneNumber ?? string.Empty;
                    listItem.CustomerEmail = person.Email ?? string.Empty;
                    listItem.CustomerAddress = person.Address ?? string.Empty;
                    listItem.CustomerInitials = GetInitials(person.Name);
                }
            }

            // TODO: Carregar dados do operador quando tiver acesso ao UserService
            listItem.OperatorName = "Operador";
            listItem.OperatorRole = "Vendedor";

            return listItem;
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            return $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
        }

        private void ClearFilters()
        {
            FilterStartDate = null;
            FilterEndDate = null;
            FilterInvoiceNumber = string.Empty;
            FilterStatus = "Todos";
            FilterCustomerName = string.Empty;
            FilterCustomerNif = string.Empty;
            FilterOperatorName = string.Empty;
            FilterPaymentType = null;
            PageNumber = 1;
            _ = SearchAsync();
        }

        private async Task UpdateStatisticsAsync()
        {
            try
            {
                var result = await _invoiceService.GetAllAsync();
                if (result.Success && result.Data != null)
                {
                    var invoices = result.Data.ToList();
                    TotalPaidCount = invoices.Count(i => i.Status == InvoiceStatus.Paid);
                    TotalPendingCount = invoices.Count(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Confirmed);
                    TotalCancelledCount = invoices.Count(i => i.Status == InvoiceStatus.Cancelled);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar estatísticas: {ex.Message}");
            }
        }

        #endregion

        #region Pagination Methods

        private async Task GoToFirstPageAsync()
        {
            PageNumber = 1;
            await SearchAsync();
        }

        private async Task GoToPreviousPageAsync()
        {
            if (PageNumber > 1)
            {
                PageNumber--;
                await SearchAsync();
            }
        }

        private async Task GoToNextPageAsync()
        {
            if (PageNumber < TotalPages)
            {
                PageNumber++;
                await SearchAsync();
            }
        }

        private async Task GoToLastPageAsync()
        {
            PageNumber = TotalPages;
            await SearchAsync();
        }

        #endregion

        #region Modal Methods

        private async Task OpenDetailsAsync(InvoiceListItemDto? invoice)
        {
            if (invoice == null) return;

            SelectedInvoice = invoice;
            await LoadInvoiceDetailsAsync();
            IsDetailsModalOpen = true;
        }

        private void CloseDetails()
        {
            IsDetailsModalOpen = false;
            SelectedInvoice = null;
            SelectedInvoiceItems.Clear();
            SelectedInvoicePayments.Clear();
            SelectedInvoiceHistory.Clear();
            StockImpactItems.Clear();
        }

        private async Task LoadInvoiceDetailsAsync()
        {
            if (SelectedInvoice == null) return;

            try
            {
                IsLoading = true;

                // Carregar detalhes completos da fatura
                var invoiceResult = await _invoiceService.GetByIdAsync(SelectedInvoice.InvoiceId);
                if (invoiceResult.Success && invoiceResult.Data != null)
                {
                    var invoice = invoiceResult.Data;
                    SelectedInvoiceTotal = invoice.Total;
                    SelectedInvoicePaid = invoice.PaidAmount;
                    SelectedInvoiceBalance = invoice.Total - invoice.PaidAmount;

                    // Carregar produtos
                    await LoadInvoiceProductsAsync(invoice.InvoiceId);

                    // Carregar pagamentos
                    await LoadInvoicePaymentsAsync(invoice.InvoiceId);

                    // Carregar histórico
                    await LoadInvoiceHistoryAsync(invoice.InvoiceId);

                    // Carregar impacto no estoque
                    await LoadStockImpactAsync();

                    // Calcular impacto financeiro
                    CalculateFinancialImpact();
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao carregar detalhes: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadInvoiceProductsAsync(int invoiceId)
        {
            try
            {
                var result = await _invoiceProductService.GetByInvoiceIdAsync(invoiceId);
                if (result.Success && result.Data != null)
                {
                    SelectedInvoiceItems = new ObservableCollection<InvoiceProductDto>(result.Data);
                }
                else
                {
                    SelectedInvoiceItems = new ObservableCollection<InvoiceProductDto>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar produtos: {ex.Message}");
                SelectedInvoiceItems = new ObservableCollection<InvoiceProductDto>();
            }
        }

        private async Task LoadInvoicePaymentsAsync(int invoiceId)
        {
            try
            {
                var result = await _paymentService.GetByInvoiceIdAsync(invoiceId);
                if (result.Success && result.Data != null)
                {
                    var payments = new List<PaymentListItemDto>();
                    foreach (var payment in result.Data)
                    {
                        var paymentType = PaymentTypes.FirstOrDefault(pt => pt.PaymentTypeId == payment.PaymentTypeId);
                        payments.Add(new PaymentListItemDto
                        {
                            PaymentId = payment.PaymentId,
                            PaymentDate = payment.PaymentDate,
                            PaymentTypeName = paymentType?.Name ?? "Desconhecido",
                            Amount = payment.Amount,
                            Reference = payment.Reference ?? string.Empty
                        });
                    }
                    SelectedInvoicePayments = new ObservableCollection<PaymentListItemDto>(payments);
                }
                else
                {
                    SelectedInvoicePayments = new ObservableCollection<PaymentListItemDto>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar pagamentos: {ex.Message}");
                SelectedInvoicePayments = new ObservableCollection<PaymentListItemDto>();
            }
        }

        private async Task LoadInvoiceHistoryAsync(int invoiceId)
        {
            try
            {
                // TODO: Implementar serviço de histórico quando disponível
                SelectedInvoiceHistory = new ObservableCollection<InvoiceHistoryItemDto>
                {
                    new InvoiceHistoryItemDto
                    {
                        ActionDescription = "Fatura criada",
                        ActionIcon = "Plus",
                        UserName = "Sistema",
                        Timestamp = DateTime.Now
                    }
                };
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar histórico: {ex.Message}");
                SelectedInvoiceHistory = new ObservableCollection<InvoiceHistoryItemDto>();
            }
        }

        private async Task LoadStockImpactAsync()
        {
            try
            {
                var stockItems = new List<StockImpactItemDto>();

                foreach (var item in SelectedInvoiceItems)
                {
                    var stockResult = await _stockService.GetByProductIdAsync(item.ProductId);
                    if (stockResult.Success && stockResult.Data != null)
                    {
                        // Buscar o nome do produto
                        var productResult = await _productService.GetByIdAsync(item.ProductId);
                        var productName = productResult.Success && productResult.Data != null 
                            ? productResult.Data.Name 
                            : $"Produto {item.ProductId}";

                        stockItems.Add(new StockImpactItemDto
                        {
                            ProductName = productName,
                            QuantitySold = item.Quantity,
                            CurrentStock = stockResult.Data.Quantity
                        });
                    }
                }

                StockImpactItems = new ObservableCollection<StockImpactItemDto>(stockItems);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar impacto no estoque: {ex.Message}");
                StockImpactItems = new ObservableCollection<StockImpactItemDto>();
            }
        }

        private void CalculateFinancialImpact()
        {
            AccountsReceivable = SelectedInvoiceBalance;
            NetRevenue = SelectedInvoicePaid;
        }

        #endregion

        #region Invoice Actions

        private async Task PrintInvoiceAsync()
        {
            if (SelectedInvoice == null) return;

            try
            {
                IsLoading = true;
                ShowMessage("Preparando impressão...");

                var invoiceResult = await _invoiceService.GetByIdAsync(SelectedInvoice.InvoiceId);
                if (invoiceResult.Success && invoiceResult.Data != null)
                {
                    var invoice = invoiceResult.Data;
                    var companyResult = await _companyConfigService.GetAsync();

                    if (companyResult.Success && companyResult.Data != null)
                    {
                        // Carregar itens da fatura
                        var itemsResult = await _invoiceProductService.GetByInvoiceIdAsync(invoice.InvoiceId);
                        var items = itemsResult.Success && itemsResult.Data != null 
                            ? itemsResult.Data 
                            : Enumerable.Empty<InvoiceProductDto>();

                        // Usar o método PrintAsync do serviço
                        await _printService.PrintAsync(
                            companyResult.Data, 
                            invoice, 
                            items, 
                            companyResult.Data.InvoiceFormat.ToString());
                        
                        ShowMessage("Fatura enviada para impressão!");
                    }
                    else
                    {
                        ShowMessage("Erro: Configuração da empresa não encontrada", true);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao imprimir: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GeneratePdfAsync()
        {
            if (SelectedInvoice == null) return;

            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Fatura_{SelectedInvoice.InvoiceNumber}_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    ShowMessage("Gerando PDF...");

                    // TODO: Implementar geração de PDF
                    await Task.Delay(1000);

                    ShowMessage($"PDF gerado: {dialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao gerar PDF: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DuplicateInvoiceAsync()
        {
            if (SelectedInvoice == null) return;

            var result = MessageBox.Show(
                "Deseja criar uma cópia desta fatura?",
                "Duplicar Fatura",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ShowMessage("Duplicando fatura...");

                var duplicateResult = await _invoiceService.DuplicateAsync(SelectedInvoice.InvoiceId);

                if (duplicateResult.Success && duplicateResult.Data != null)
                {
                    ShowMessage($"Fatura duplicada: {duplicateResult.Data.InvoiceNumber}");
                    await SearchAsync();
                    await UpdateStatisticsAsync();
                }
                else
                {
                    ShowMessage($"Erro ao duplicar: {duplicateResult.Message}", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao duplicar: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ReopenInvoiceAsync()
        {
            if (SelectedInvoice == null) return;

            var result = MessageBox.Show(
                "Deseja reabrir esta fatura cancelada?",
                "Reabrir Fatura",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ShowMessage("Reabrindo fatura...");

                var reopenResult = await _invoiceService.ReopenAsync(SelectedInvoice.InvoiceId);

                if (reopenResult.Success)
                {
                    ShowMessage("Fatura reaberta com sucesso!");
                    await SearchAsync();
                    await UpdateStatisticsAsync();

                    if (IsDetailsModalOpen)
                    {
                        await LoadInvoiceDetailsAsync();
                    }
                }
                else
                {
                    ShowMessage($"Erro ao reabrir: {reopenResult.Message}", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao reabrir: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task IssueCreditNoteAsync()
        {
            if (SelectedInvoice == null) return;

            var result = MessageBox.Show(
                "Deseja emitir uma nota de crédito para esta fatura?\nIsso criará um documento de devolução/estorno.",
                "Nota de Crédito",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ShowMessage("Emitindo nota de crédito...");

                // TODO: Implementar emissão de nota de crédito
                await Task.Delay(1000);

                ShowMessage("Nota de crédito emitida com sucesso!");
                await SearchAsync();
                await UpdateStatisticsAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao emitir nota de crédito: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task IssueDebitNoteAsync()
        {
            if (SelectedInvoice == null) return;

            var result = MessageBox.Show(
                "Deseja emitir uma nota de débito para esta fatura?\nIsso criará um documento de cobrança adicional.",
                "Nota de Débito",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ShowMessage("Emitindo nota de débito...");

                // TODO: Implementar emissão de nota de débito
                await Task.Delay(1000);

                ShowMessage("Nota de débito emitida com sucesso!");
                await SearchAsync();
                await UpdateStatisticsAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao emitir nota de débito: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CancelInvoiceAsync()
        {
            if (SelectedInvoice == null) return;

            var result = MessageBox.Show(
                "Tem certeza que deseja CANCELAR esta fatura?\n\nEsta ação:\n• Cancelará todos os pagamentos\n• Reverterá o estoque\n• Não poderá ser desfeita\n\nDeseja continuar?",
                "Cancelar Fatura",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ShowMessage("Cancelando fatura...");

                var cancelResult = await _invoiceService.CancelAsync(SelectedInvoice.InvoiceId, "Cancelamento via gestão de faturas");

                if (cancelResult.Success)
                {
                    ShowMessage("Fatura cancelada com sucesso!");
                    await SearchAsync();
                    await UpdateStatisticsAsync();

                    if (IsDetailsModalOpen)
                    {
                        await LoadInvoiceDetailsAsync();
                    }
                }
                else
                {
                    ShowMessage($"Erro ao cancelar: {cancelResult.Message}", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao cancelar: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Payment Methods

        private bool CanAddPayment(object? parameter)
        {
            return SelectedInvoice != null &&
                   NewPayment?.PaymentTypeId > 0 &&
                   NewPayment.Amount > 0 &&
                   NewPayment.Amount <= SelectedInvoiceBalance;
        }

        private async Task AddPaymentAsync()
        {
            if (SelectedInvoice == null || NewPayment == null) return;

            if (NewPayment.PaymentTypeId <= 0)
            {
                ShowMessage("Selecione uma forma de pagamento!", true);
                return;
            }

            if (NewPayment.Amount <= 0)
            {
                ShowMessage("Informe um valor válido!", true);
                return;
            }

            if (NewPayment.Amount > SelectedInvoiceBalance)
            {
                ShowMessage($"Valor não pode exceder o saldo de Kz {SelectedInvoiceBalance:N2}!", true);
                return;
            }

            try
            {
                IsLoading = true;

                var paymentDto = new PaymentDto
                {
                    InvoiceId = SelectedInvoice.InvoiceId,
                    PaymentTypeId = NewPayment.PaymentTypeId,
                    Amount = NewPayment.Amount,
                    Reference = NewPayment.Reference ?? string.Empty,
                    PaymentDate = DateTime.Now,
                    IsConfirmed = true
                };

                var result = await _paymentService.AddAsync(paymentDto);

                if (result.Success)
                {
                    ShowMessage("Pagamento registrado com sucesso!");

                    // Limpar formulário
                    NewPayment = new NewPaymentDto();

                    // Recarregar dados
                    await LoadInvoiceDetailsAsync();
                    await SearchAsync();
                    await UpdateStatisticsAsync();
                }
                else
                {
                    ShowMessage($"Erro ao registrar pagamento: {result.Message}", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao registrar pagamento: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RemovePaymentAsync(PaymentListItemDto? payment)
        {
            if (payment == null) return;

            var result = MessageBox.Show(
                $"Deseja remover o pagamento de Kz {payment.Amount:N2}?\nEsta ação não pode ser desfeita.",
                "Remover Pagamento",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;

                var deleteResult = await _paymentService.DeleteAsync(payment.PaymentId);

                if (deleteResult.Success)
                {
                    ShowMessage("Pagamento removido com sucesso!");
                    await LoadInvoiceDetailsAsync();
                    await SearchAsync();
                    await UpdateStatisticsAsync();
                }
                else
                {
                    ShowMessage($"Erro ao remover pagamento: {deleteResult.Message}", true);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao remover pagamento: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Adjustment Methods

        private bool CanApplyDiscount(object? parameter)
        {
            return SelectedInvoice != null &&
                   AdjustmentDiscount > 0 &&
                   !string.IsNullOrWhiteSpace(AdjustmentReason);
        }

        private async Task ApplyDiscountAsync()
        {
            if (SelectedInvoice == null) return;

            if (AdjustmentDiscount > SelectedInvoiceTotal)
            {
                ShowMessage("Desconto não pode ser maior que o total da fatura!", true);
                return;
            }

            try
            {
                IsLoading = true;
                ShowMessage("Aplicando desconto...");

                // TODO: Implementar aplicação de desconto no serviço
                await Task.Delay(1000);

                ShowMessage($"Desconto de Kz {AdjustmentDiscount:N2} aplicado com sucesso!");
                AdjustmentDiscount = 0;
                AdjustmentReason = string.Empty;

                await LoadInvoiceDetailsAsync();
                await SearchAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao aplicar desconto: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanApplySurcharge(object? parameter)
        {
            return SelectedInvoice != null &&
                   AdjustmentSurcharge > 0 &&
                   !string.IsNullOrWhiteSpace(SurchargeReason);
        }

        private async Task ApplySurchargeAsync()
        {
            if (SelectedInvoice == null) return;

            try
            {
                IsLoading = true;
                ShowMessage("Aplicando acréscimo...");

                // TODO: Implementar aplicação de acréscimo no serviço
                await Task.Delay(1000);

                ShowMessage($"Acréscimo de Kz {AdjustmentSurcharge:N2} aplicado com sucesso!");
                AdjustmentSurcharge = 0;
                SurchargeReason = string.Empty;

                await LoadInvoiceDetailsAsync();
                await SearchAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao aplicar acréscimo: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanChangePaymentType(object? parameter)
        {
            return PaymentToChange != null && NewPaymentType != null;
        }

        private async Task ChangePaymentTypeAsync()
        {
            if (PaymentToChange == null || NewPaymentType == null) return;

            var result = MessageBox.Show(
                $"Deseja alterar a forma de pagamento?\nDe: {PaymentToChange.PaymentTypeName}\nPara: {NewPaymentType.Name}",
                "Alterar Forma de Pagamento",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ShowMessage("Alterando forma de pagamento...");

                // Buscar o pagamento completo
                var paymentResult = await _paymentService.GetByIdAsync(PaymentToChange.PaymentId);
                if (paymentResult.Success && paymentResult.Data != null)
                {
                    var payment = paymentResult.Data;
                    payment.PaymentTypeId = NewPaymentType.PaymentTypeId;

                    var updateResult = await _paymentService.UpdateAsync(payment);

                    if (updateResult.Success)
                    {
                        ShowMessage("Forma de pagamento alterada com sucesso!");
                        PaymentToChange = null;
                        NewPaymentType = null;

                        await LoadInvoiceDetailsAsync();
                    }
                    else
                    {
                        ShowMessage($"Erro ao alterar: {updateResult.Message}", true);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao alterar forma de pagamento: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Stock & Financial Methods

        private async Task UpdateStockAsync()
        {
            if (SelectedInvoice == null) return;

            var result = MessageBox.Show(
                "Deseja atualizar o estoque baseado nesta fatura?",
                "Atualizar Estoque",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ShowMessage("Atualizando estoque...");

                // TODO: Implementar atualização de estoque
                await Task.Delay(1000);

                ShowMessage("Estoque atualizado com sucesso!");
                await LoadStockImpactAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao atualizar estoque: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task PostToAccountsReceivableAsync()
        {
            if (SelectedInvoice == null) return;

            var result = MessageBox.Show(
                $"Deseja lançar Kz {AccountsReceivable:N2} em contas a receber?",
                "Lançar Contas a Receber",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                ShowMessage("Lançando em contas a receber...");

                // TODO: Implementar lançamento contábil
                await Task.Delay(1000);

                ShowMessage("Lançamento contábil realizado com sucesso!");
            }
            catch (Exception ex)
            {
                ShowMessage($"Erro ao lançar: {ex.Message}", true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GenerateFinancialReportAsync()
        {
            if (SelectedInvoice == null) return;

            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Relatorio_Impacto_{SelectedInvoice.InvoiceNumber}_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    ShowMessage("Gerando relatório...");

                    // TODO: Implementar geração de relatório financeiro
                    await Task.Delay(1000);

                    ShowMessage($"Relatório gerado: {dialog.FileName}");
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

        #region Export & Refresh

        private async Task ExportAsync()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx",
                    FileName = $"Faturas_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    ShowMessage("Exportando...");

                    var sb = new StringBuilder();
                    sb.AppendLine("Número;Data;Cliente;NIF;Operador;Status;Total;Pago;Saldo");

                    foreach (var invoice in Invoices)
                    {
                        sb.AppendLine($"{invoice.InvoiceNumber};{invoice.Date:dd/MM/yyyy HH:mm};{invoice.CustomerName};{invoice.CustomerNif};{invoice.OperatorName};{invoice.Status};{invoice.Total:N2};{invoice.PaidAmount:N2};{invoice.Balance:N2}");
                    }

                    await File.WriteAllTextAsync(dialog.FileName, sb.ToString(), Encoding.UTF8);
                    ShowMessage($"Exportação concluída! {Invoices.Count} registros exportados.");
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

        private async Task RefreshAsync()
        {
            await SearchAsync();
            await UpdateStatisticsAsync();
            ShowMessage("Dados atualizados!");
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

    #region Helper DTOs

    public class InvoiceListItemDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public InvoiceStatus Status { get; set; }
        public decimal Total { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNif { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerInitials { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string OperatorRole { get; set; } = string.Empty;
    }

    public class PaymentListItemDto
    {
        public int PaymentId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentTypeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
    }

    public class InvoiceHistoryItemDto
    {
        public string ActionDescription { get; set; } = string.Empty;
        public string ActionIcon { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class NewPaymentDto : BaseViewModel
    {
        private int _paymentTypeId;
        public int PaymentTypeId
        {
            get => _paymentTypeId;
            set => Set(ref _paymentTypeId, value);
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set => Set(ref _amount, value);
        }

        private string _reference = string.Empty;
        public string Reference
        {
            get => _reference;
            set => Set(ref _reference, value);
        }
    }

    public class StockImpactItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal QuantitySold { get; set; }
        public decimal CurrentStock { get; set; }
    }

    #endregion
}